using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;

public class TesseractDemoScript : MonoBehaviour
{
    [SerializeField] private RawImage rawImageToRecognize;  // input of rawimage for recognizing

    [SerializeField] private Text displayText;              // output dump the recognize string and debug info
    [SerializeField] private RawImage outputImage;          // output image with original and squareoverlays
    [SerializeField] private RawImage outputSearchImage;    // output Search image with original and overlays
    [SerializeField] private Text searchText;               // search in the recognized text for this expression
    private TesseractDriver _tesseractDriver;               // instamce of tesseract driver
    private string _text = "";                              // local copy of text which would be dumped on displaytext if pressent
    private Texture2D _texture;                             // local reference of inputTexture converted
    private Texture2D _oriTexture;                          // keep the original for debug use

    private bool fSetupComplete = false;                         // barrier to start recognizing only when ready
    private bool fRecognizing = false;

    // for result data storing maybe an array of words with boundingbox coordinates

    private string lastfoundString = "";
    private string lastfoundError = "";

    private void Start()
    {
        _tesseractDriver = new TesseractDriver();
        AddToTextDisplay(_tesseractDriver.CheckTessVersion());
        _tesseractDriver.Setup(OnSetupCompleteRecognize);
    }

    // Update is called once per frame
    void Update()
    {
        if (fSetupComplete == true)
        {
            if (fRecognizing == false)
            {
                fRecognizing = true;
                Texture2D texture2D = ConvertRawToTexture2D(rawImageToRecognize);
                if (texture2D)
                {
                    Recoginze(texture2D);
                    Search(searchText);
                }
            }
        }
    }

    private Texture2D ConvertRawToTexture2D(RawImage rawImageToRecognize){
        // Texture2D texture = new Texture2D(rawImageToRecognize.width, rawImageToRecognize.height, TextureFormat.ARGB32, false);
        //Texture2D texture = rawImageToRecognize.texture as Texture2D;
        Texture texture = rawImageToRecognize.texture;
        Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
        Graphics.Blit(texture, renderTexture);

        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(renderTexture);

        if (texture2D)
        {
            texture2D.SetPixels32(texture2D.GetPixels32());
            texture2D.Apply();
        }
        else
        {
            Debug.LogError("could not convert texture from raw");
        }
        return texture2D;
    }

    private void Recoginze(Texture2D inputTexture)
    {
        _texture = inputTexture;
        _oriTexture = new Texture2D(inputTexture.width, inputTexture.height, TextureFormat.RGBA32, false);
        Graphics.CopyTexture(inputTexture, _oriTexture);

        string recRes = _tesseractDriver.Recognize(_texture);
        string errRes = _tesseractDriver.GetErrorMessage();

        if (lastfoundString != recRes)
        {
            ClearTextDisplay();
            AddToTextDisplay("recRes: " + recRes);
        }
        lastfoundString = recRes;

        if (errRes != null && errRes != "" && lastfoundError != errRes)
        {
            AddToTextDisplay("found Error: " + errRes, true);
            lastfoundError = errRes;
        }
        else
        {
            SetProcessedImageDisplay();
        }
        fRecognizing = false;
    }

    private void Search(Text searchText)
    {
        // get text from Text field
        string text = searchText.text;

        // get string from Recognize result 
        string [] foundwords = _tesseractDriver.GetWords();
        
        // try to find a match (index)
        List<int> foundidxs = new List<int>();
        if (text.Length > 0)
        {
            for (int i = 0; i < foundwords.Length; i++ )
            {
                string foundword = foundwords[i];
                if (foundword.Contains(text))
                {
                    ClearTextDisplay();
                    AddToTextDisplay("!!!! Found: " + text);      
                    foundidxs.Add(i);
                }
            }
        }
        // get points of match 
        // List<WordDetails> details = new WordDetails();
        foreach (var idx in foundidxs)
        {
            WordDetails wd = _tesseractDriver.GetDetails(idx);
            Box box = wd.box;
            // details.Add(_tesseractDriver.GetDetails(idx));
            // TODO: make something nicer and more intuitive maybe choose a scale base on confidence
            DrawLines(_oriTexture,
                new Rect(box.x, _oriTexture.height - box.y - box.h, box.w, box.h),
                Color.yellow);

            // draw rect highlight
            // if (confidence[index] >= MinimumConfidence)

            //DrawLines(_oriTexture,
            //    new Rect(box.x, _oriTexture.height - box.y - box.h, box.w, box.h),
            //    Color.green);
        }
        SetSearchImageDisplay();
    }

    private void DrawLines(Texture2D texture, Rect boundingRect, Color color, int thickness = 3)
    {
        int x1 = (int) boundingRect.x;
        int x2 = (int) (boundingRect.x + boundingRect.width);
        int y1 = (int) boundingRect.y;
        int y2 = (int) (boundingRect.y + boundingRect.height);

        for (int x = x1; x <= x2; x++)
        {
            for (int i = 0; i < thickness; i++)
            {
                texture.SetPixel(x, y1 + i, color);
                texture.SetPixel(x, y2 - i, color);
            }
        }

        for (int y = y1; y <= y2; y++)
        {
            for (int i = 0; i < thickness; i++)
            {
                texture.SetPixel(x1 + i, y, color);
                texture.SetPixel(x2 - i, y, color);
            }
        }

        texture.Apply();
    }

    private void OnSetupCompleteRecognize()
    {
        AddToTextDisplay("Setup Complete");
        fSetupComplete = true;
    }

    private void ClearTextDisplay()
    {
        _text = "";
    }

    private void AddToTextDisplay(string text, bool isError = false)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        _text += (string.IsNullOrWhiteSpace(displayText.text) ? "" : "\n") + text;
        if (isError)
            Debug.LogError(text);
        else
            Debug.Log(text);
    }

    private void LateUpdate()
    {
        displayText.text = _text;
    }

    private void SetProcessedImageDisplay()
    {
        if (outputImage)
        {
            //RectTransform rectTransform = outputImage.GetComponent<RectTransform>();
            //rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
            //    rectTransform.rect.width * _tesseractDriver.GetHighlightedTexture().height / _tesseractDriver.GetHighlightedTexture().width);
            outputImage.texture = _tesseractDriver.GetHighlightedTexture();
        }
    }
    private void SetSearchImageDisplay()
    {
        if (outputSearchImage)
        {
            outputSearchImage.texture = _oriTexture;
        }
    }
}