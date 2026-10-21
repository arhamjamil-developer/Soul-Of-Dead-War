using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using SoulCrystalDefense.UI;
using SoulCrystalDefense.Systems;

namespace SoulCrystalDefense.EditorTools
{
    public static class ExploreHUDArtAuthoring
    {
        const string Art="Assets/UI/Production art/";
        static Transform Find(string n)=>Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(t=>t.name==n);
        static Sprite Sprite(string path)=>AssetDatabase.LoadAssetAtPath<Sprite>(Art+path+".png");
        static T Ensure<T>(GameObject go) where T:Component{var c=go.GetComponent<T>();return c?c:go.AddComponent<T>();}
        [MenuItem("SoulCrystalDefense/Presentation/Install Collapse HUD and Neighbors")]
        public static void Install()
        {
            if(EditorApplication.isPlaying)throw new System.Exception("Stop Play before authoring");
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(scene.name!="SoulCrystalDefenseGameplay")throw new System.Exception("Open gameplay scene first");
            foreach(var folder in new[]{Art+"Neighbors",Art+"Collapse UI"})foreach(var file in System.IO.Directory.GetFiles(folder,"*.png"))
            {
                var path=file.Replace('\\','/');AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var i=(TextureImporter)AssetImporter.GetAtPath(path);i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;i.alphaIsTransparency=true;i.mipmapEnabled=false;i.maxTextureSize=2048;i.textureCompression=TextureImporterCompression.Uncompressed;i.filterMode=FilterMode.Bilinear;i.spritePixelsPerUnit=100;
                i.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="iPhone",overridden=true,maxTextureSize=2048,format=TextureImporterFormat.ASTC_4x4,compressionQuality=100});i.SaveAndReimport();
            }
            var backdrop=Find("GraveyardBackdrop").GetComponent<SpriteRenderer>();var bounds=backdrop.bounds;
            var old=GameObject.Find("NeighborHomes");if(old)Object.DestroyImmediate(old);
            var world=new GameObject("NeighborHomes");
            var ls=WorldSprite("GuardianCottage_Left",world.transform,Sprite("Neighbors/Guardian-Cottage"),bounds.center+Vector3.left*bounds.size.x,bounds.size,backdrop);
            var rs=WorldSprite("ChefCottage_Right",world.transform,Sprite("Neighbors/Chef-Cottage"),bounds.center+Vector3.right*bounds.size.x,bounds.size,backdrop);
            var domain=bounds;domain.Encapsulate(ls.bounds);domain.Encapsulate(rs.bounds);
            var pan=Object.FindFirstObjectByType<CinemachineCameraPanController>();
            if(!pan)throw new System.Exception("Existing Cinemachine pan controller missing");
            var ps=new SerializedObject(pan);ps.FindProperty("neighborPageWidth").floatValue=bounds.size.x;ps.FindProperty("neighborFocusY").floatValue=2;ps.ApplyModifiedPropertiesWithoutUndo();
            var vcam=pan.CinemachineCam;var confiner=vcam.GetComponent<CinemachineConfiner2D>();
            var shape=confiner.BoundingShape2D as PolygonCollider2D;
            if(!shape)throw new System.Exception("Existing polygon camera domain missing");
            shape.pathCount=1;shape.SetPath(0,new[]{Local(shape,domain.min),Local(shape,new Vector3(domain.min.x,domain.max.y)),Local(shape,domain.max),Local(shape,new Vector3(domain.max.x,domain.min.y))});
            confiner.InvalidateBoundingShapeCache();confiner.BakeBoundingShape(vcam,.5f);EditorUtility.SetDirty(shape);
            var composition=Find("GameplayComposition");var top=Find("TopHUD").GetComponent<RectTransform>();var bottom=Find("BottomTray").GetComponent<RectTransform>();
            var existing=composition.Find("HUDExploreControls");if(existing)Object.DestroyImmediate(existing.gameObject);
            var root=Rect("HUDExploreControls",composition,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;
            var controller=root.gameObject.AddComponent<CollapsibleBattleHUD>();controller.layoutRoot=composition.GetComponent<RectTransform>();controller.topPanel=top;controller.bottomPanel=bottom;
            controller.topGroup=Ensure<CanvasGroup>(top.gameObject);controller.bottomGroup=Ensure<CanvasGroup>(bottom.gameObject);
            controller.up=Sprite("Collapse UI/Arrow-Up");controller.down=Sprite("Collapse UI/Arrow-Down");controller.upPressed=Sprite("Collapse UI/Arrow-Up-Pressed");controller.downPressed=Sprite("Collapse UI/Arrow-Down-Pressed");
            controller.topButton=Button("TopHUDHandle",root,new Vector2(.5f,1),Vector2.zero,controller.up,new Vector2(168,76));
            controller.bottomButton=Button("BottomTrayHandle",root,new Vector2(.5f,0),Vector2.zero,controller.down,new Vector2(168,76));
            controller.topHandle=controller.topButton.GetComponent<RectTransform>();controller.bottomHandle=controller.bottomButton.GetComponent<RectTransform>();
            controller.topArtwork=controller.topButton.transform.Find("Artwork").GetComponent<Image>();controller.bottomArtwork=controller.bottomButton.transform.Find("Artwork").GetComponent<Image>();
            UnityEventTools.AddPersistentListener(controller.topButton.onClick,controller.ToggleTop);UnityEventTools.AddPersistentListener(controller.bottomButton.onClick,controller.ToggleBottom);
            var pause=Button("FloatingPause",root,new Vector2(0,1),new Vector2(82,-76),AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Final/Production/Icons/Pause.png"),new Vector2(100,100));
            controller.floatingPauseGroup=Ensure<CanvasGroup>(pause.gameObject);UnityEventTools.AddPersistentListener(pause.onClick,controller.Pause);
            var back=Button("ReturnToBattle",root,new Vector2(0,0),new Vector2(82,76),AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Final/Production/Icons/Crystal.png"),new Vector2(95,95));
            controller.returnBattleGroup=Ensure<CanvasGroup>(back.gameObject);UnityEventTools.AddPersistentListener(back.onClick,controller.ReturnToBattle);
            Label("ReturnLabel",back.transform,"BATTLE",new Vector2(0,-50),new Vector2(140,26),21);
            controller.screenFlow=Object.FindFirstObjectByType<GameScreenFlow>();
            var hint=Label("ExploreHint",root,"SWIPE LEFT / RIGHT TO EXPLORE",new Vector2(0,154),new Vector2(730,36),24);hint.rectTransform.anchorMin=hint.rectTransform.anchorMax=new Vector2(.5f,0);controller.exploreHint=hint;hint.alpha=0;
            controller.PreviewAuthoredLayout();controller.floatingPauseGroup.gameObject.SetActive(false);controller.returnBattleGroup.gameObject.SetActive(false);
            foreach(var button in root.GetComponentsInChildren<UnityEngine.UI.Button>(true)){var n=button.navigation;n.mode=Navigation.Mode.None;button.navigation=n;}
            foreach(var obj in new Object[]{controller,controller.topGroup,controller.bottomGroup,pan,shape,vcam})EditorUtility.SetDirty(obj);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log($"EXPLORE_HUD|Saved measured top travel {controller.TopTravel:F1}, bottom {controller.BottomTravel:F1}, 180x136 touch targets, two neighbors width {bounds.size.x:F2}");
        }
        static Vector2 Local(Collider2D c,Vector3 p)=>c.transform.InverseTransformPoint(p);
        static SpriteRenderer WorldSprite(string name,Transform parent,Sprite sprite,Vector3 position,Vector3 size,SpriteRenderer reference)
        {
            var go=new GameObject(name,typeof(SpriteRenderer));go.transform.SetParent(parent,false);go.transform.position=position;
            var r=go.GetComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=reference.sharedMaterial;r.sortingLayerID=reference.sortingLayerID;r.sortingOrder=reference.sortingOrder;
            go.transform.localScale=new Vector3(size.x/sprite.bounds.size.x,size.y/sprite.bounds.size.y,1);return r;
        }
        static RectTransform Rect(string name,Transform parent,Vector2 anchor,Vector2 position,Vector2 size)
        {
            var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=anchor;r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=position;r.sizeDelta=size;return r;
        }
        static Button Button(string name,Transform parent,Vector2 anchor,Vector2 position,Sprite sprite,Vector2 visibleSize)
        {
            var r=Rect(name,parent,anchor,position,new Vector2(180,136));var hit=r.gameObject.AddComponent<Image>();hit.color=new Color(1,1,1,.001f);hit.raycastTarget=true;
            var artwork=Rect("Artwork",r,new Vector2(.5f,.5f),Vector2.zero,visibleSize).gameObject.AddComponent<Image>();artwork.sprite=sprite;artwork.preserveAspect=true;artwork.raycastTarget=false;
            var b=r.gameObject.AddComponent<Button>();b.targetGraphic=artwork;return b;
        }
        static TextMeshProUGUI Label(string name,Transform parent,string text,Vector2 position,Vector2 size,float fontSize)
        {
            var r=Rect(name,parent,new Vector2(.5f,.5f),position,size);var label=r.gameObject.AddComponent<TextMeshProUGUI>();label.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Final/PaybAck SDF.asset");label.text=text;label.fontSize=label.fontSizeMax=fontSize;label.fontSizeMin=fontSize*.8f;label.enableAutoSizing=true;label.alignment=TextAlignmentOptions.Center;label.color=new Color(.02f,.13f,.3f);label.raycastTarget=false;return label;
        }
    }
}
