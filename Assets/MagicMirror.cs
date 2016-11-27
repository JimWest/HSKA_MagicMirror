using UnityEngine;
using System.Collections;
using OpenCvSharp;
using System.IO;

public class MagicMirror : MonoBehaviour
{

    public GameObject background;

    VideoCapture capture;
    Mat webcamMat;
    Material backgroundMaterial;
    

    // Use this for initialization
    void Start()
    {        
        backgroundMaterial = background.GetComponent<Renderer>().material;

        webcamMat = new Mat();
        capture = new VideoCapture();

        capture.Open(0);
        if (!capture.IsOpened())
        {
            Debug.LogError("Unable to open the Camera");
            this.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        using (Mat tempMat = capture.RetrieveMat())
        {
            // mirror image on y axis that its like a mirror
            Cv2.Flip(tempMat, webcamMat, FlipMode.Y);
            //Cv2.ImShow("Test", webcamMat);

            Texture2D tex = OpenCvUtilities.MatToTexture2D(webcamMat);
            
            //File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", tex.EncodeToPNG());

            backgroundMaterial.mainTexture = tex;
        }

    }

    void OnDestroy()
    {
        webcamMat.Dispose();
        capture.Release();
        capture.Dispose();
    }

}
