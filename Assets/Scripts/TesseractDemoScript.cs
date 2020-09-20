using UnityEngine;
using UnityEngine.UI;

public class TesseractDemoScript : MonoBehaviour
{
    [SerializeField] private RawImage rawImageToRecognize;  // input of rawimage for recognizing

    [SerializeField] private Text displayText;              // output dump the recognize string and debug info
    [SerializeField] private RawImage outputImage;          // output image with original and squareoverlays
    private TesseractDriver _tesseractDriver;               // instamce of tesseract driver
    private string _text = "";                              // local copy of text which would be dumped on displaytext if pressent
    private Texture2D _texture;                             // local reference of inputTexture converted

    private int fSetupComplete = 0;                         // barrier to start recognizing only when ready

    private string lastfoundString = "";


    private void Start()
    {
        _tesseractDriver = new TesseractDriver();
        AddToTextDisplay(_tesseractDriver.CheckTessVersion());
        _tesseractDriver.Setup(OnSetupCompleteRecognize);
    }

    // Update is called once per frame
    void Update()
    {
        if (fSetupComplete == 1)
        {
            AddToTextDisplay( "Update");
            Texture2D texture2D = ConvertRawToTexture2D(rawImageToRecognize);
            if (texture2D)
                Recoginze(texture2D);
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
            texture2D = null;
        }
        return texture2D;
    }

    private void Recoginze(Texture2D inputTexture)
    {
        _texture = inputTexture;
        //ClearTextDisplay();

        string recRes = _tesseractDriver.Recognize(_texture);
        string errRes = _tesseractDriver.GetErrorMessage();

        if (lastfoundString != recRes)
            AddToTextDisplay("recRes: "+ recRes);
        lastfoundString = recRes;

        if(errRes != null && errRes != "") {
            AddToTextDisplay("found Error: " + errRes, true);
        }   
        else {
            SetImageDisplay();
        }
    }

    private void OnSetupCompleteRecognize()
    {
        AddToTextDisplay("Setup Complete");
        fSetupComplete = 1;
    }

    private void ClearTextDisplay()
    {
       // _text = "";
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

    private void SetImageDisplay()
    {
        if (outputImage)
        {
            AddToTextDisplay("outputImage: "+ outputImage);
            RectTransform rectTransform = outputImage.GetComponent<RectTransform>();
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                rectTransform.rect.width * _tesseractDriver.GetHighlightedTexture().height / _tesseractDriver.GetHighlightedTexture().width);
            outputImage.texture = _tesseractDriver.GetHighlightedTexture();
        }
    }
}