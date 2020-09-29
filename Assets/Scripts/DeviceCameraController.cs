     using UnityEngine;
     using UnityEngine.UI;
     using System.Linq;
     using System.Collections;
     
     public class DeviceCameraController : MonoBehaviour
     {
         public RawImage image;
         public RectTransform imageParent;
         public AspectRatioFitter imageFitter;
     
         // Device cameras
         WebCamDevice frontCameraDevice;
         WebCamDevice backCameraDevice;
         WebCamDevice activeCameraDevice;
     
         WebCamTexture frontCameraTexture;
         WebCamTexture backCameraTexture;
         WebCamTexture activeCameraTexture;
     
         // Image rotation
         Vector3 rotationVector = new Vector3(0f, 0f, 0f);
     
         // Image uvRect
         Rect defaultRect = new Rect(0f, 0f, 1f, 1f);
         Rect fixedRect = new Rect(0f, 1f, 1f, -1f);
     
         // Image Parent's scale
         Vector3 defaultScale = new Vector3(1f, 1f, 1f);
         Vector3 fixedScale = new Vector3(-1f, 1f, 1f);
     
     
         void Start()
         {
             // Check for device cameras
             if (WebCamTexture.devices.Length == 0)
             {
                 Debug.Log("No devices cameras found");
                 return;
             }
     
             // Get the device's cameras and create WebCamTextures with them
             frontCameraDevice = WebCamTexture.devices.Last();
             backCameraDevice = WebCamTexture.devices.First();
     
             frontCameraTexture = new WebCamTexture(frontCameraDevice.name);
             backCameraTexture = new WebCamTexture(backCameraDevice.name);
     
             // Set camera filter modes for a smoother looking image
             frontCameraTexture.filterMode = FilterMode.Trilinear;
             backCameraTexture.filterMode = FilterMode.Trilinear;
     
             // Set the camera to use by default
             SetActiveCamera(frontCameraTexture);
         }
     
         // Set the device camera to use and start it
         public void SetActiveCamera(WebCamTexture cameraToUse)
         {
             if (activeCameraTexture != null)
             {
                 activeCameraTexture.Stop();
             }
                 
             activeCameraTexture = cameraToUse;
             activeCameraDevice = WebCamTexture.devices.FirstOrDefault(device => 
                 device.name == cameraToUse.deviceName);
     
             image.texture = activeCameraTexture;
             image.material.mainTexture = activeCameraTexture;
     
             activeCameraTexture.Play();
         }
     
         // Switch between the device's front and back camera
         public void SwitchCamera()
         {
             SetActiveCamera(activeCameraTexture.Equals(frontCameraTexture) ? 
                 backCameraTexture : frontCameraTexture);
         }
             
         // Make adjustments to image every frame to be safe, since Unity isn't 
         // guaranteed to report correct data as soon as device camera is started
         void Update()
         {
             // Skip making adjustment for incorrect camera data
             if (activeCameraTexture.width < 100)
             {
                 Debug.Log("Still waiting another frame for correct info...");
                 return;
             }
     
             // Rotate image to show correct orientation 
             rotationVector.z = -activeCameraTexture.videoRotationAngle;
             image.rectTransform.localEulerAngles = rotationVector;
     
             // Set AspectRatioFitter's ratio
             float videoRatio = 
                 (float)activeCameraTexture.width / (float)activeCameraTexture.height;
             imageFitter.aspectRatio = videoRatio;
     
             // Unflip if vertically flipped
             image.uvRect = 
                 activeCameraTexture.videoVerticallyMirrored ? fixedRect : defaultRect;
     
             // Mirror front-facing camera's image horizontally to look more natural
             imageParent.localScale = 
                 activeCameraDevice.isFrontFacing ? fixedScale : defaultScale;
         }
     }

     // fatties answer 
     private void Update()
   {
   if ( wct.width < 100 )
     {
     Debug.Log("Still waiting another frame for correct info...");
     return;
     }
   
   // change as user rotates iPhone or Android:
   
   int cwNeeded = wct.videoRotationAngle;
   // Unity helpfully returns the _clockwise_ twist needed
   // guess nobody at Unity noticed their product works in counterclockwise:
   int ccwNeeded = -cwNeeded;
   
   // IF the image needs to be mirrored, it seems that it
   // ALSO needs to be spun. Strange: but true.
   if ( wct.videoVerticallyMirrored ) ccwNeeded += 180;
   
   // you'll be using a UI RawImage, so simply spin the RectTransform
   rawImageRT.localEulerAngles = new Vector3(0f,0f,ccwNeeded);
   
   float videoRatio = (float)wct.width/(float)wct.height;
   
   // you'll be using an AspectRatioFitter on the Image, so simply set it
   rawImageARF.aspectRatio = videoRatio;
   
   // alert, the ONLY way to mirror a RAW image, is, the uvRect.
   // changing the scale is completely broken.
   if ( wct.videoVerticallyMirrored )
     rawImage.uvRect = new Rect(1,0,-1,1);  // means flip on vertical axis
   else
     rawImage.uvRect = new Rect(0,0,1,1);  // means no flip
   
   // devText.text =
   //  videoRotationAngle+"/"+ratio+"/"+wct.videoVerticallyMirrored;
   }

//Comments on Fattie's Answer:
//So I'm pretty sure at this point that "verticallyMirrored" means that a rotation about the horizontal axis is needed. So I've modified the code in Fattie's answer to:

        int ccwNeeded = -wct.videoRotationAngle;
        
        rawImageRT.localEulerAngles = new Vector3(0f,0f,ccwNeeded);
        
        float videoRatio = (float)wct.width/(float)wct.height;
        
        rawImageARF.aspectRatio = videoRatio;
        
        if ( wct.videoVerticallyMirrored )
          rawImage.uvRect = new Rect(0,1,1,-1);  // flip on HORIZONTAL axis
        else
          rawImage.uvRect = new Rect(0,0,1,1); // no flip

        