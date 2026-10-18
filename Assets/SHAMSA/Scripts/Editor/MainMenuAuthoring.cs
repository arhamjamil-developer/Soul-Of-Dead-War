using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using SoulCrystalDefense.UI;

namespace SoulCrystalDefense.EditorTools
{
    public static class MainMenuAuthoring
    {
        const string ScenePath = "Assets/Scenes/MainMenu.unity";
        const string Art = "Assets/UI/";
        static readonly Color Ink = new Color(.035f,.045f,.11f,.98f);
        static readonly Color Cream = new Color(1f,.92f,.72f);

        [MenuItem("SoulCrystalDefense/Main Menu/Author Saved Canvas")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Stop Play Mode first.");
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != ScenePath)
            {
                if (scene.isDirty) throw new System.InvalidOperationException("Save the current scene first.");
                scene = EditorSceneManager.OpenScene(ScenePath);
            }
            if (scene.GetRootGameObjects().Any(g=>g.name=="MainMenuCanvas"))
                throw new System.InvalidOperationException("MainMenuCanvas already exists. Edit its saved hierarchy directly.");
            Import("Background.png", false);
            foreach (string file in new[]{"Logo.png","Button_Play.png","Button_Shop.png","Button_Settings.png","Button_HowToPlay.png","Button_Exit.png"}) Import(file,true);
            var canvasGO = new GameObject("MainMenuCanvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            var canvas=canvasGO.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasGO.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1080,1920); scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; scaler.matchWidthOrHeight=.5f;
            var bg=Picture("Background_FullBleed",canvasGO.transform,"Background.png");
            bg.color=new Color(.45f,.45f,.55f,1);
            Stretch(bg.rectTransform);
            var cover=bg.gameObject.AddComponent<AspectRatioFitter>(); cover.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent; cover.aspectRatio=(float)bg.sprite.texture.width/bg.sprite.texture.height;
            var safe=Rect("SafeArea",canvasGO.transform); Stretch(safe); safe.gameObject.AddComponent<SafeAreaFitter>();
            var board=Rect("PortraitComposition_1080x1920",safe); Stretch(board);
            var fit=board.gameObject.AddComponent<AspectRatioFitter>(); fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=1080f/1920f;
            var composition=Picture("Background_PortraitComposition",board,"Background.png");Stretch(composition.rectTransform);
            // A fixed design coordinate system inside a fitted portrait surface.
            var space=Rect("DesignSpace",board); Stretch(space);
            // Child positions use normalized anchors so no runtime menu builder is needed.
            var logo=Picture("Logo",space,"Logo.png"); Place(logo.rectTransform,540,280,850,390);
            Label("MainMenuHeading",space,"MAIN MENU",540,1170,800,52,40);
            var controller=canvasGO.AddComponent<MainMenuController>();
            controller.playButton=ArtButton(space,"Play","Button_Play.png",540,1290,820,182,controller.Play);
            ArtButton(space,"Shop","Button_Shop.png",325,1465,390,150,controller.OpenShop);
            ArtButton(space,"Settings","Button_Settings.png",755,1465,390,150,controller.OpenSettings);
            ArtButton(space,"HowToPlay","Button_HowToPlay.png",540,1630,820,182,controller.OpenHowToPlay);
            ArtButton(space,"Exit","Button_Exit.png",540,1790,450,144,controller.OpenExit);
            controller.settingsPanel=Panel(space,"SettingsPanel","SETTINGS",controller);
            controller.volumeLabel=Label("VolumeLabel",controller.settingsPanel.transform,"MASTER VOLUME  100%",540,760,760,90,40);
            controller.volumeSlider=Volume(controller.settingsPanel.transform,controller);
            Label("SavedHint",controller.settingsPanel.transform,"Your audio preference is saved automatically.",540,1100,720,150,32);
            controller.shopPanel=Panel(space,"ShopPanel","SOUL SHOP",controller);
            Label("Catalog",controller.shopPanel.transform,"YOUR DEFENDER COLLECTION\n\nHAMMER DWARF\nFront-line hammer strikes\n\nBATTLE CHEF\nFrying-pan defender\n\nROYAL ELEPHANT\nArmored heavy defender\n\nSOUL TRAIN\nEnchanted locomotive\n\nAll four are included in your squad.\nNo purchases are available yet.",540,1030,720,920,33);
            controller.howToPlayPanel=Panel(space,"HowToPlayPanel","HOW TO PLAY",controller);
            Label("Instructions",controller.howToPlayPanel.transform,"DEFEND THE SOUL CRYSTAL\n\n1. Choose a defender card.\n\n2. Tap an empty board tile to deploy.\nPlacement costs Soul Energy.\n\n3. Defenders attack automatically.\nEnemies advance from the top.\n\n4. Keep enemies away from the crystal\nand survive each wave.\n\nSoul Energy regenerates over time.\nWatch your energy and crystal health.",540,1020,720,920,36);
            controller.exitPanel=Panel(space,"ExitPanel","LEAVE THE GAME?",controller);
            controller.exitMessage=Label("ExitMessage",controller.exitPanel.transform,"Ready to leave the citadel?",540,900,710,220,44);
            PlainButton("ConfirmExit",controller.exitPanel.transform,"EXIT",540,1180,650,130,controller.ConfirmExit);
            controller.ClosePanels();
            if (!Object.FindAnyObjectByType<EventSystem>())
            {
                var events=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            var camera=Camera.main;
            if(camera) {camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Ink;}
            var scenes=EditorBuildSettings.scenes.Where(s=>s.path!=ScenePath).ToList();
            scenes.Insert(0,new EditorBuildSettingsScene(ScenePath,true));
            EditorBuildSettings.scenes=scenes.ToArray();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets(); Selection.activeGameObject=canvasGO;
            Debug.Log("MAINMENU|AUTHORED|Saved Canvas, separate sprites, five persistent Button events, safe-area portrait composition.");
        }

        static void Import(string name,bool alpha)
        {
            var path=Art+name;
            if(!File.Exists(path)) throw new FileNotFoundException(path);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.alphaIsTransparency=alpha;importer.mipmapEnabled=false;importer.isReadable=false;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=4096;
            importer.npotScale=TextureImporterNPOTScale.None;importer.SaveAndReimport();
        }
        static RectTransform Rect(string name,Transform parent)
        {
            var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);return go.GetComponent<RectTransform>();
        }
        static void Stretch(RectTransform r) {r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
        static void Place(RectTransform r,float x,float y,float w,float h)
        {
            r.anchorMin=new Vector2((x-w/2)/1080,1-(y+h/2)/1920);
            r.anchorMax=new Vector2((x+w/2)/1080,1-(y-h/2)/1920);
            r.offsetMin=r.offsetMax=Vector2.zero;
        }
        static Image Picture(string name,Transform parent,string file)
        {
            var r=Rect(name,parent);var im=r.gameObject.AddComponent<Image>(); im.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Art+file);im.raycastTarget=false;return im;
        }
        static Text Label(string name,Transform parent,string text,float x,float y,float w,float h,float size)
        {
            var r=Rect(name,parent);Place(r,x,y,w,h);var t=r.gameObject.AddComponent<Text>();
            t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=text;t.color=Cream;t.alignment=TextAnchor.MiddleCenter;
            t.fontSize=(int)size;t.resizeTextForBestFit=true;t.resizeTextMinSize=(int)(size*.65f);t.resizeTextMaxSize=(int)size;
            t.raycastTarget=false;return t;
        }
        static Button ArtButton(Transform parent,string name,string file,float x,float y,float w,float h,UnityAction action)
        {
            var image=Picture("Button_"+name,parent,file);Place(image.rectTransform,x,y,w,h);image.raycastTarget=true;
            var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;
            var c=b.colors;c.normalColor=Color.white;c.highlightedColor=new Color(1,.94f,.8f);c.pressedColor=new Color(.65f,.75f,.8f);c.selectedColor=Color.white;b.colors=c;
            b.navigation=new Navigation{mode=Navigation.Mode.None};UnityEventTools.AddPersistentListener(b.onClick,action);return b;
        }
        static GameObject Panel(Transform parent,string name,string title,MainMenuController controller)
        {
            var root=Rect(name,parent);Stretch(root);var shade=root.gameObject.AddComponent<Image>();shade.color=new Color(0,0,.03f,.82f);
            var card=Rect("PanelSurface",root);Place(card,540,1010,880,1450);var im=card.gameObject.AddComponent<Image>();im.color=Ink;
            var border=card.gameObject.AddComponent<Outline>();border.effectColor=new Color(.7f,.48f,.17f);border.effectDistance=new Vector2(3,-3);
            Label("Heading",root,title,540,450,760,140,60);
            PlainButton("Close",root,"BACK TO MENU",540,1580,700,135,controller.ClosePanels);
            return root.gameObject;
        }
        static Button PlainButton(string name,Transform parent,string text,float x,float y,float w,float h,UnityAction action)
        {
            var r=Rect(name,parent);Place(r,x,y,w,h);var im=r.gameObject.AddComponent<Image>();im.color=new Color(.13f,.23f,.35f);
            var button=r.gameObject.AddComponent<Button>();button.targetGraphic=im;UnityEventTools.AddPersistentListener(button.onClick,action);
            var label=Label("Label",r,text,540,960,1000,1400,42);Stretch(label.rectTransform);
            button.navigation=new Navigation{mode=Navigation.Mode.None};return button;
        }
        static Slider Volume(Transform parent,MainMenuController controller)
        {
            var r=Rect("MasterVolume",parent);Place(r,540,930,700,110);
            var track=r.gameObject.AddComponent<Image>();track.color=new Color(.15f,.19f,.3f);
            var fill=Rect("Fill",r);Stretch(fill);var image=fill.gameObject.AddComponent<Image>();image.color=new Color(.08f,.75f,.7f);
            var handle=Rect("Handle",r);handle.sizeDelta=new Vector2(60,120);var hi=handle.gameObject.AddComponent<Image>();hi.color=Cream;
            var slider=r.gameObject.AddComponent<Slider>();slider.minValue=0;slider.maxValue=1;slider.value=1;
            slider.fillRect=fill;slider.handleRect=handle;slider.targetGraphic=hi;
            UnityEventTools.AddPersistentListener(slider.onValueChanged,controller.SetVolume);return slider;
        }
    }
}

