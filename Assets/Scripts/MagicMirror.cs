using UnityEngine;
using System.Collections;
using OpenCvSharp;

using System.IO;
using UnityEditor;

public class MagicMirror : MonoBehaviour
{

    [Header("Basic settings")]
    public GameObject background;
    public Vector2 camResolution = new Vector2(640, 480);
    public bool detectFaces = true;

    [Header("Face detect settings")]
    public TextAsset cascadeFaceFile;
    public float faceScale = 1.4f;
    public int faceMinNeightbors = 5;
    public HaarDetectionType faceDetectType = HaarDetectionType.ScaleImage;
    public Vector2 faceSize = new Vector2(50, 50);

    [Header("Eye detect settings")]
    public TextAsset cascadeEyeFile;
    public float eyeScale = 1.5f;
    public int eyeMinNeightbors = 5;
    public HaarDetectionType eyeDetectType = HaarDetectionType.ScaleImage;
    public Vector2 eyeSize = new Vector2(20, 20);

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
            if (detectFaces)
            {
                Mat haarResult = DetectFace(camImage);

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
        if (capture != null)
        {
            capture.Release();
            capture.Dispose();
        }

        if (faceCascade != null)
            faceCascade.Dispose();

        if (eyeCascade != null)
            eyeCascade.Dispose();
        
    }

    private Mat DetectFace(Mat src)
    {
        Mat result;

        using (Mat gray = new Mat())
        {
            Cv2.EqualizeHist(gray, gray);

            result = src.Clone();
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

            // Detect faces
            OpenCvSharp.Rect[] faces = faceCascade.DetectMultiScale(
                gray, faceScale, faceMinNeightbors, faceDetectType, new Size(faceSize.x, faceSize.y));

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
                    Width = (int)(face.Width * 0.4),
                    Height = (int)(face.Height * 0.5)
                };

                using (Mat faceROI = new Mat(gray, face))
                {

                    // Detect eyes in the faces
                    OpenCvSharp.Rect[] eyes = eyeCascade.DetectMultiScale(
                        faceROI, eyeScale, eyeMinNeightbors, eyeDetectType, new Size(eyeSize.x, eyeSize.y));
                    
                    // Render all eyes
                    foreach (OpenCvSharp.Rect eye in eyes)
                    {
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

                // draw ellipse at the face
                Cv2.Ellipse(result, faceCenter, faceAxes, 0, 0, 360, new Scalar(255, 0, 255), 4);
            }
        }
        return result;
    }



}


