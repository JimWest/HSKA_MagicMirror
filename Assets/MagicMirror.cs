using UnityEngine;
using System.Collections;
using OpenCvSharp;

using System.IO;

public class MagicMirror : MonoBehaviour
{

    public GameObject background;

    public Vector2 camResolution = new Vector2(640, 480);

    VideoCapture capture;
    Material backgroundMaterial;    

    // Use this for initialization
    void Start()
    {        
        backgroundMaterial = background.GetComponent<Renderer>().material;
        capture = new VideoCapture();

        capture.Open(0);
        if (!capture.IsOpened())
        {
            Debug.LogError("Unable to open the Camera");
            this.gameObject.SetActive(false);
            return;
        }

        capture.Set(CaptureProperty.FrameHeight, camResolution.y);
        capture.Set(CaptureProperty.FrameWidth, camResolution.x);
    }

    // Update is called once per frame
    void Update()
    {

        

        // Get the image of the camera
        using (Mat camMat = capture.RetrieveMat())
        {
            // mirror image on y axis that its like a mirror
            Cv2.Flip(camMat, camMat, FlipMode.Y);

            // convert it to a Unity Texture2D
            Texture2D tex = OpenCvUtilities.MatToTexture2D(camMat);

            // For testing: save the image
            //File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", tex.EncodeToPNG());

            // destroy old if exists, Unity wont clean it automatically
            if (backgroundMaterial.mainTexture != null)
            {
                Texture2D.DestroyImmediate(backgroundMaterial.mainTexture, true);
            }

            backgroundMaterial.mainTexture = tex;
        }

    }

    void OnDestroy()
    {
        capture.Release();
        capture.Dispose();
    }
    

}
