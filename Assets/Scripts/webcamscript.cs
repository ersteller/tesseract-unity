using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class webcamscript : MonoBehaviour
{
    public RawImage outputImage;
    private WebCamTexture webcamTexture;
    public Text displayText;

    private string _text = "";

    private int camIdx = 0;

    private int camLenght = 0;


    public void ChangeCam()
    {

        camIdx++;
        camIdx %= camLenght;

        AddToTextDisplay("Stop Cam: " + webcamTexture.deviceName);
        webcamTexture.Stop();
        webcamTexture.deviceName = WebCamTexture.devices[camIdx].name;
        AddToTextDisplay("Start Cam: " + webcamTexture.deviceName);
        webcamTexture.Play();

    }

    // Start is called before the first frame update
    void Start()
    {
        AddToTextDisplay("Start()");

        webcamTexture = new WebCamTexture();

        WebCamDevice[] devices = WebCamTexture.devices;
        camLenght = devices.Length;

        for (int i = 0; i < camLenght; i++)
            AddToTextDisplay(devices[i].name);

        if (devices.Length > 0)
        {
            AddToTextDisplay("Start Cam: " + webcamTexture.deviceName);
            webcamTexture.deviceName = devices[0].name;
            webcamTexture.Play();
        }

        SetImageDisplay();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void SetImageDisplay()
    {
        if (outputImage)
        {
            outputImage.texture = this.webcamTexture;
        }
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
