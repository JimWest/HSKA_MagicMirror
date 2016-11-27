using UnityEngine;
using System.Collections;
using OpenCvSharp;

using System.IO;
using UnityEditor;

public class MagicMirror : MonoBehaviour
{

    public GameObject background;
    public Vector2 camResolution = new Vector2(640, 480);
    public TextAsset cascadeFaceFile;
    public TextAsset cascadeEyeFile;

    VideoCapture capture;
    Material backgroundMaterial;
    CascadeClassifier faceCascade;
    CascadeClassifier eyeCascade;

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


        string cascadePath = AssetDatabase.GetAssetPath(cascadeFaceFile);
        cascadePath = cascadePath.Remove(0, 7);
        faceCascade = new CascadeClassifier(Application.dataPath + "/" + cascadePath);

        cascadePath = AssetDatabase.GetAssetPath(cascadeEyeFile);
        cascadePath = cascadePath.Remove(0, 7);
        eyeCascade = new CascadeClassifier(Application.dataPath + "/" + cascadePath);
    }

    // Update is called once per frame
    void Update()
    {

        // Get the image of the camera
        using (Mat camImage = capture.RetrieveMat())
        {
            Texture2D tex;

            // mirror image on y axis that its like a mirror
            Cv2.Flip(camImage, camImage, FlipMode.Y);

            // detect faces
            Mat haarResult = DetectFace(camImage);
            if (haarResult != null)
            {
                // convert it to a Unity Texture2D
                tex = OpenCvUtilities.MatToTexture2D(haarResult);
            }
            else
            {
                tex = OpenCvUtilities.MatToTexture2D(camImage);
            }

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

    private Mat DetectFace(Mat src)
    {
        Mat result;

        using (var gray = new Mat())
        {
            result = src.Clone();
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

            // Detect faces
            OpenCvSharp.Rect[] faces = faceCascade.DetectMultiScale(
                gray, 1.3, 5, HaarDetectionType.ScaleImage, new Size(20, 20));
                       

            bool faceFound = false;

            // Render all detected faces
            foreach (OpenCvSharp.Rect face in faces)
            {
                var faceCenter = new Point
                {
                    X = (int)(face.X + face.Width * 0.5),
                    Y = (int)(face.Y + face.Height * 0.5)
                };
                var faceAxes = new Size
                {
                    Width = (int)(face.Width * 0.5),
                    Height = (int)(face.Height * 0.5)
                };
                                
                using (Mat faceROI = new Mat(gray, face))
                {

                    // Detect eyes in the faces
                    OpenCvSharp.Rect[] eyes = eyeCascade.DetectMultiScale(
                        faceROI, 1.2, 5, HaarDetectionType.DoRoughSearch, new Size(10, 10));

                    // Render all detected faces
                    foreach (OpenCvSharp.Rect eye in eyes)
                    {
                        faceFound = true;
                        var eyeCenter = new Point
                        {
                            X = (int)(face.X + eye.X + eye.Width * 0.5),
                            Y = (int)(face.Y + eye.Y + eye.Height * 0.5)
                        };
                        var eyeAxes = new Size
                        {
                            Width = (int)(eye.Width * 0.5),
                            Height = (int)(eye.Height * 0.5)
                        };
                        // draw ellipse at the eyes
                        Cv2.Ellipse(result, eyeCenter, eyeAxes, 0, 0, 360, new Scalar(0, 0, 255), 4);
                    }
                }
                
                if (faceFound)
                    // draw ellipse at the face
                    Cv2.Ellipse(result, faceCenter, faceAxes, 0, 0, 360, new Scalar(255, 0, 255), 4);
            }
        }
        return result;
    }



}


