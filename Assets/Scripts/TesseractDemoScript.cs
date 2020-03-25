using UnityEngine;
using UnityEngine.UI;

public class TesseractDemoScript : MonoBehaviour
{
    [SerializeField] private Texture2D imageToRecognize;
    [SerializeField] private RawImage rawImageToRecognize;
    [SerializeField] private Text displayText;
    [SerializeField] private RawImage outputImage;
    private TesseractDriver _tesseractDriver;
    private string _text = "";
    private Texture2D _texture;

    private void Start()
    {
        if (imageToRecognize)
        {
            Texture2D texture = new Texture2D(imageToRecognize.width, imageToRecognize.height, TextureFormat.ARGB32, false);
            texture.SetPixels32(imageToRecognize.GetPixels32());
            texture.Apply();

            _tesseractDriver = new TesseractDriver();
            Recoginze(texture);
            SetImageDisplay();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (rawImageToRecognize)
        {
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

                _tesseractDriver = new TesseractDriver();
                Recoginze(texture2D);
                SetImageDisplay();
            }
            else
            {
                Debug.LogError("could not convert texture from raw");
            }
        }


    }



    private void Recoginze(Texture2D outputTexture)
    {

        _texture = outputTexture;
        ClearTextDisplay();
        AddToTextDisplay(_tesseractDriver.CheckTessVersion());
        _tesseractDriver.Setup(OnSetupCompleteRecognize);
    }

    private void OnSetupCompleteRecognize()
    {
        AddToTextDisplay(_tesseractDriver.Recognize(_texture));
        AddToTextDisplay(_tesseractDriver.GetErrorMessage(), true);
        SetImageDisplay();
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

    private void SetImageDisplay()
    {
        if (outputImage)
        {
            RectTransform rectTransform = outputImage.GetComponent<RectTransform>();
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                rectTransform.rect.width * _tesseractDriver.GetHighlightedTexture().height / _tesseractDriver.GetHighlightedTexture().width);
            outputImage.texture = _tesseractDriver.GetHighlightedTexture();
        }
    }
}