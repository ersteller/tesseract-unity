using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class RawImageToTexture : MonoBehaviour
{
    public RawImage Test;
    public Texture2D TheTexture;
    void Start()
    {
        TheTexture = Test.texture as Texture2D;
    }
}

/*   using System.Collections;
    using System.Collections.Generic;
    using UnityEngine; 
 *    
 *    public class RawImageToTexture : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}*/
