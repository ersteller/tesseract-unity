using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;

using System;
using System.Threading;
using System.Threading.Tasks;

public class TesseractDemoScript : MonoBehaviour
{
    [SerializeField] private RawImage rawImageToRecognize;  // input of rawimage for recognizing

    [SerializeField] private Text displayText;              // output dump the recognize string and debug info
    [SerializeField] private RawImage outputSearchImage;    // output Search image with original and overlays
    [SerializeField] private Text searchText;               // search in the recognized text for this expression
    private TesseractDriver _tesseractDriver;               // instamce of tesseract driver
    private string _text = "";                              // local copy of text which would be dumped on displaytext if pressent
    private Texture2D _texture;                             // local reference of inputTexture converted
    private Texture2D _oriTexture;                          // keep the original for debug use

    private bool fSetupComplete = false;                    // barrier to start recognizing only when ready
    private bool fRecognizing = false;                      // this is set when starting the thread and is unset in callback
    private bool fSearch = false;                           // this is set in callback when recognizing is done

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
                    // any unity api stuff needs to be in the main thread
                    // seperate Threading only for Tesseract stuff
                    Recoginze(texture2D); // frecognizing=false and fSearch=true is set in callback 
                }
            }
            if (fSearch)
            {
                // only search when there is a new result in the main thread (set in callback)
                // (unity api is not threadsafe and forbids calling in other threads)
                fSearch = false; 
                Search(searchText);
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
        if (   _oriTexture == null 
            || _oriTexture.width != inputTexture.width 
            || _oriTexture.height != inputTexture.height)
        {
            _oriTexture = new Texture2D(inputTexture.width, inputTexture.height, TextureFormat.RGBA32, false);
        }
        
        Graphics.CopyTexture(inputTexture, _oriTexture);

        _tesseractDriver.RecognizeThreaded(_texture, DelegateCallback);
    }

    public void DelegateCallback (){
        fRecognizing = false;
        fSearch = true;
    }

    private void Search(Text searchText)
    {
        // get text from Text field
        string text = searchText.text;

        // get string from Recognize result 
        string [] foundwords = _tesseractDriver.GetWords();
        // get texture from Recognize result
        Texture2D texture = _tesseractDriver.GetTextureProcessed();
        
        // try to find a match (index)
        List<int> foundidxs = new List<int>();
        if (text.Length > 0)
        {
            for (int i = 0; i < foundwords.Length; i++ )
            {
                string foundword = foundwords[i];
                string foundwordlower = foundword.ToLower();
                if (foundword.Contains(text) || foundwordlower.Contains(text))
                {
                    ClearTextDisplay();
                    AddToTextDisplay("!!!! Found: " + text + " " +  foundidxs.Count + " times." );      
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
            // TODO: make something nicer and more intuitive maybe choose a colorscale base on confidence
            DrawLines(texture,
                new Rect(box.x, texture.height - box.y - box.h, box.w, box.h),
                Color.yellow);
        }
        if (outputSearchImage)
        {
            outputSearchImage.texture = texture;
        }
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
}