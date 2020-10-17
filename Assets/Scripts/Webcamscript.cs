using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Webcamscript : MonoBehaviour
{
    public RawImage outputImage;
    public RawImage outputImagePreview;
    public Text displayText;

    private WebCamTexture webcamTexture;

    private string _text = "";

    private int camIdx = 0;

    private enum ResName
    {
        uninit,
        HD,
        UHD,
    }
    private ResName resName = ResName.HD;    // we start with fullHD resolution 

    private int GetResolutionIdx(ResName res){
        if (res == ResName.uninit) return -1;

        int height = 0;
        int width = 0;
        switch (res)
        {
            case ResName.HD: 
                height = 1080;
                width = 1920;
                break;
            case ResName.UHD:
                height = 2160;
                width = 3840;
                break;
            default:
                break;
        }

        for (int i =0; i < WebCamTexture.devices[camIdx].availableResolutions.Length; i++ )
        {
            Resolution reqres = WebCamTexture.devices[camIdx].availableResolutions[i];
            if (reqres.width == width && reqres.height == height)
                return i;
        }
        Debug.LogError("Resolution not found: " + width + " x " + height);
        return 0;
    }

    public void ChangeCam()
    {
        // we want the next cam or wrapparound
        camIdx++;
        camIdx %= WebCamTexture.devices.Length;
        
        ChangeSetup();
    }  

    public void ChangeSetup(){

        // in case something is already running
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            AddToTextDisplay("Stop Cam: " + webcamTexture.deviceName);
            webcamTexture.Stop();
        }
        // check for requested resolution index of the device to be used if not found just take first resolution (0)
        Resolution reqres = WebCamTexture.devices[camIdx].availableResolutions[GetResolutionIdx(resName)];
        // make texture with device and resolution
        webcamTexture = new WebCamTexture(WebCamTexture.devices[camIdx].name, reqres.width, reqres.height);
        
        // do some logging and start Cam
        ClearTextDisplay();
        AddToTextDisplay("Start Cam: " + webcamTexture.deviceName + "Resolution: " + reqres);
        webcamTexture.Play();
        // update target objects with new texture 
        SetImageOutput();
        SetImageDisplay();
    }

    public void ChangeRes()
    {
        // toggle between hd and uhd
        if (resName == ResName.HD){
            resName = ResName.UHD;
        } else{
            resName = ResName.HD;
        }
        ChangeSetup();
    }

    // Start is called before the first frame update
    void Start()
    {
        //AddToTextDisplay("Start()");
        ChangeSetup();    // this also initializes the first
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void SetImageDisplay()
    {
        if (outputImagePreview)
        {
            outputImagePreview.texture = this.webcamTexture;
        }
    }
    private void SetImageOutput()
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
