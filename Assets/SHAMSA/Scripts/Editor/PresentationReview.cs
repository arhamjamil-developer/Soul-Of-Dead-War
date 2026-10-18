using UnityEditor;
using UnityEngine;
using SoulCrystalDefense.Systems;

namespace SoulCrystalDefense.EditorTools
{
    public static class PresentationReview
    {
        // Render the real camera and live uGUI together at a device resolution.
        public static string Capture(string path, int width=540, int height=960)
        {
            var camera=Camera.main;
            var canvas=GameObject.Find("HUDCanvas").GetComponent<Canvas>();
            var oldMode=canvas.renderMode;var oldCamera=canvas.worldCamera;float oldDistance=canvas.planeDistance;
            var oldTarget=camera.targetTexture;float oldSize=camera.orthographicSize;
            var oldActive=RenderTexture.active;
            var rt=new RenderTexture(width,height,24);
            var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=rt;
                camera.GetComponent<ResponsiveCamera>().ApplyResponsiveSize();
                canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
                Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;
                texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                System.IO.File.WriteAllBytes(path,texture.EncodeToPNG());
                return path;
            }
            finally
            {
                camera.targetTexture=oldTarget;camera.orthographicSize=oldSize;
                canvas.renderMode=oldMode;canvas.worldCamera=oldCamera;canvas.planeDistance=oldDistance;
                RenderTexture.active=oldActive;Object.DestroyImmediate(texture);Object.DestroyImmediate(rt);
            }
        }
    }
}
