using UnityEngine;
using System.Collections;
using OpenCvSharp;
using System;

public static class OpenCvUtilities
{

    public static Texture2D MatToTexture2D(Mat mat)
    {
        Texture2D tex = new Texture2D(mat.Width, mat.Height, TextureFormat.RGB24, false);

        // needs to be flipped at x axis                        
        using (Mat tempMat = mat.Flip(FlipMode.X))
        {
            // also wrong color (BGR instead of RGB), write it back to rgbMat
            Cv2.CvtColor(tempMat, tempMat, ColorConversionCodes.BGR2RGB);

            int size = (int)tempMat.Total() * tempMat.ElemSize();
            tex.LoadRawTextureData(tempMat.Data, size);
            tex.Apply();
        }

        return tex;
    }

    public static Mat MatToTexture2D(Texture2D tex)
    {
        Mat mat = new Mat(tex.height, tex.width, MatType.CV_8UC3, tex.GetRawTextureData());
        // needs to be flipped at x axis                        
        using (Mat tempMat = mat.Flip(FlipMode.X))
        {
            // also wrong color (BGR instead of RGB), write it back to rgbMat
            Cv2.CvtColor(tempMat, tempMat, ColorConversionCodes.RGB2BGR);
        }

        return mat;
    }

}
