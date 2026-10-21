using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace SoulCrystalDefense.EditorTools
{
    // Art-stage only: builds a standalone editable prefab and renders a review sheet.
    // Does not alter the gameplay scene or attach a tutorial controller.
    public static class TutorialArtAuthoring
    {
        const string Root = "Assets/UI/Totorial/";
        static TMP_FontAsset font;
        static Sprite frame;
        static readonly Color Ink = new Color(.025f,.12f,.28f);
        [MenuItem("SoulCrystalDefense/Tutorial/Build Art Preview")]
        public static void Build()
        {
            foreach (var file in Directory.GetFiles(Root,"*.png",SearchOption.AllDirectories))
            {
                var path=file.Replace('\\','/');
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var imp=(TextureImporter)AssetImporter.GetAtPath(path);
                imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;
                imp.alphaIsTransparency=true;imp.mipmapEnabled=false;imp.maxTextureSize=2048;
                imp.textureCompression=TextureImporterCompression.Uncompressed;imp.spritePixelsPerUnit=100;
                if(path.EndsWith("TutorialFrame.png"))imp.spriteBorder=new Vector4(135,135,135,135);
                imp.SaveAndReimport();
            }
            font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Final/PaybAck SDF.asset");
            frame=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Art/TutorialFrame.png");
            if(!font||!frame)throw new System.Exception("Tutorial frame or PAYBACK font missing");
            var panel=MakePanel("TutorialDialogue",null,"Dwarf","GUARDIAN","PROTECT OUR SOUL CRYSTAL!\nLET'S PREPARE FOR WAVE ONE.","1 / 7");
            PrefabUtility.SaveAsPrefabAsset(panel.gameObject,Root+"Prefabs/TutorialDialogue.prefab");
            Object.DestroyImmediate(panel.gameObject);
            RenderReview();
            AssetDatabase.SaveAssets();
            Debug.Log("TUTORIAL_ART|Saved white/cyan border, editable PAYBACK prefab, and review PNG. Gameplay unchanged.");
        }
        static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h)
        {
            var go=new GameObject(name,typeof(RectTransform));if(parent)go.transform.SetParent(parent,false);
            var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);
            r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
        }
        static Image Picture(string name,Transform parent,Sprite s,float x,float y,float w,float h,bool sliced=false)
        {
            var r=Rect(name,parent,x,y,w,h);var im=r.gameObject.AddComponent<Image>();im.sprite=s;
            im.type=sliced?Image.Type.Sliced:Image.Type.Simple;im.pixelsPerUnitMultiplier=sliced?2:1;im.preserveAspect=!sliced;im.raycastTarget=false;return im;
        }
        static TextMeshProUGUI Label(string name,Transform parent,string words,float x,float y,float w,float h,float size)
        {
            var r=Rect(name,parent,x,y,w,h);var t=r.gameObject.AddComponent<TextMeshProUGUI>();
            t.font=font;t.text=words;t.color=Ink;t.fontSize=t.fontSizeMax=size;t.fontSizeMin=size*.8f;
            t.enableAutoSizing=true;t.textWrappingMode=TextWrappingModes.Normal;t.overflowMode=TextOverflowModes.Ellipsis;
            t.alignment=TextAlignmentOptions.MidlineLeft;t.margin=new Vector4(4,4,4,4);t.raycastTarget=false;return t;
        }
        static RectTransform MakePanel(string name,Transform parent,string portrait,string speaker,string words,string step)
        {
            var r=Rect(name,parent,0,0,1000,500);
            Picture("DialogueBorder",r,frame,0,130,1000,280,true);
            Picture("PortraitHolder",r,frame,22,8,245,280,true);
            Picture("GuidePortrait",r,AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Portraits/"+portrait+".png"),36,20,218,244);
            Label("Speaker_PAYBACK",r,speaker,292,162,650,55,37);
            Label("Instruction_PAYBACK",r,words,290,230,650,140,32);
            Label("Progress_PAYBACK",r,step,34,318,225,44,26).alignment=TextAlignmentOptions.Center;
            var continueImage=Picture("ContinueButton",r,frame,737,423,260,72,true);continueImage.pixelsPerUnitMultiplier=6;continueImage.raycastTarget=true;
            var button=continueImage.gameObject.AddComponent<Button>();button.targetGraphic=continueImage;
            Label("Continue_PAYBACK",continueImage.transform,"CONTINUE",14,7,232,58,28).alignment=TextAlignmentOptions.Center;
            var skipImage=Picture("SkipButton",r,frame,20,423,180,72,true);skipImage.pixelsPerUnitMultiplier=6;skipImage.raycastTarget=true;
            var skip=skipImage.gameObject.AddComponent<Button>();skip.targetGraphic=skipImage;
            Label("Skip_PAYBACK",skipImage.transform,"SKIP",12,7,156,58,26).alignment=TextAlignmentOptions.Center;
            return r;
        }
        static void RenderReview()
        {
            var scene=EditorSceneManager.NewPreviewScene();
            var canvasGo=new GameObject("TutorialArtReview",typeof(RectTransform),typeof(Canvas));
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(canvasGo,scene);
            var canvas=canvasGo.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
            var cr=canvasGo.GetComponent<RectTransform>();cr.sizeDelta=new Vector2(1080,1920);cr.localScale=Vector3.one*.01f;
            cr.position=Vector3.zero;
            Label("ReviewTitle",cr,"FIRST WAVE TUTORIAL",50,55,980,90,49).alignment=TextAlignmentOptions.Center;
            Label("ReviewSubtitle",cr,"WHITE / CYAN UI  -  PAYBACK TEXT",50,145,980,60,28).alignment=TextAlignmentOptions.Center;
            var a=MakePanel("Welcome",cr,"Dwarf","GUARDIAN","PROTECT OUR SOUL CRYSTAL!\nLET'S PREPARE FOR WAVE ONE.","1 / 7");a.anchoredPosition=new Vector2(40,-260);
            var b=MakePanel("Placement",cr,"Archer","ARCHER","SELECT A GUARDIAN CARD.\nTAP A FREE PATCH IN THE CENTER LANE.","3 / 7");b.anchoredPosition=new Vector2(40,-800);
            var c=MakePanel("WaveClear",cr,"Mage","MAGE","WAVE ONE CLEARED!\nKEEP DEFENDING. THE WAVES ARE ENDLESS.","7 / 7");c.anchoredPosition=new Vector2(40,-1340);
            var cameraGo=new GameObject("ReviewCamera",typeof(Camera));UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraGo,scene);
            var camera=cameraGo.GetComponent<Camera>();camera.scene=scene;
            camera.overrideSceneCullingMask=EditorSceneManager.GetSceneCullingMask(scene);
            canvas.worldCamera=camera;camera.orthographic=true;camera.orthographicSize=9.6f;
            camera.transform.position=new Vector3(0,0,-20);camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.12f,.59f,.88f);camera.nearClipPlane=.1f;camera.farClipPlane=50;
            var rt=new RenderTexture(1080,1920,24);camera.targetTexture=rt;
            Canvas.ForceUpdateCanvases();foreach(var text in canvasGo.GetComponentsInChildren<TMP_Text>())text.ForceMeshUpdate();
            camera.Render();var previous=RenderTexture.active;RenderTexture.active=rt;
            var texture=new Texture2D(1080,1920,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1080,1920),0,0);texture.Apply();
            Directory.CreateDirectory("ArtReview/Tutorial");File.WriteAllBytes("ArtReview/Tutorial/Tutorial-PAYBACK-Review.png",texture.EncodeToPNG());
            RenderTexture.active=previous;camera.targetTexture=null;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(texture);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }
}
