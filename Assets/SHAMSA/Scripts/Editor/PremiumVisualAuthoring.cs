using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using SoulCrystalDefense.UI;
using SoulCrystalDefense.Systems;

namespace SoulCrystalDefense.EditorTools
{
    public static class PremiumVisualAuthoring
    {
        const string A="Assets/UI/Premium/";
        static Font font;
        static readonly Color Cream=new Color(1,.93f,.76f),Cyan=new Color(.25f,.9f,1),Gold=new Color(.9f,.72f,.4f);
        static Transform Find(string n)=>UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(t=>t.name==n);
        static Sprite S(string p)=>AssetDatabase.LoadAssetAtPath<Sprite>(A+p+".png");
        public static void ImportKit()
        {
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            foreach(string path in System.IO.Directory.GetFiles("Assets/UI/Premium","*.png",System.IO.SearchOption.AllDirectories))
            {
                string p=path.Replace('\\','/');AssetDatabase.ImportAsset(p,ImportAssetOptions.ForceSynchronousImport);
                var i=(TextureImporter)AssetImporter.GetAtPath(p);i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;i.spritePixelsPerUnit=p.Contains("/VFX/")?256:100;i.alphaIsTransparency=true;i.mipmapEnabled=false;i.textureCompression=TextureImporterCompression.Uncompressed;i.maxTextureSize=2048;
                if(p.Contains("/Frames/")){var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(p);float border=Mathf.Min(tex.width,tex.height)*.2f;i.spriteBorder=new Vector4(border,border,border,border);}
                i.SaveAndReimport();
            }
        }
        [MenuItem("SoulCrystalDefense/Premium Visuals/Author Menu Screens")]
        public static void Menu()
        {
            if(EditorApplication.isPlaying)throw new Exception("Stop Play Mode first");ImportKit();
            var current=EditorSceneManager.GetActiveScene();if(current.isDirty)EditorSceneManager.SaveScene(current);
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
            var old=GameObject.Find("MainMenuCanvas");if(old)UnityEngine.Object.DestroyImmediate(old);
            var go=new GameObject("MainMenuCanvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            go.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1080,1920);scaler.matchWidthOrHeight=.5f;
            var bg=Img("FullBleedBackdrop",go.transform,AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Background.png"),0,0,1080,1920);Stretch(bg.rectTransform);
            var cover=bg.gameObject.AddComponent<AspectRatioFitter>();cover.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;cover.aspectRatio=9f/16;
            var safe=R("SafeArea",go.transform);Stretch(safe);
            var root=R("MenuComposition",safe);root.anchorMin=root.anchorMax=root.pivot=new Vector2(.5f,.5f);root.sizeDelta=new Vector2(1080,1920);
            Img("MenuBackground",root,AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Background.png"),0,0,1080,1920);
            Img("Logo",root,AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Logo.png"),125,100,830,395).preserveAspect=false;
            var controller=go.AddComponent<MainMenuController>();
            T("MenuLabel",root,"MAIN MENU",150,1050,780,60,40,Gold);
            controller.playButton=Button("Play",root,"PLAY","Play",150,1130,780,165,controller.Play);
            Button("Shop",root,"SOUL SHOP","Owned",110,1330,410,125,controller.OpenShop);
            Button("Settings",root,"SETTINGS","Settings",560,1330,410,125,controller.OpenSettings);
            Button("HowToPlay",root,"HOW TO PLAY","Book",150,1490,780,130,controller.OpenHowToPlay);
            Button("Exit",root,"EXIT","Exit",300,1670,480,110,controller.OpenExit);
            controller.settingsPanel=Settings(root,controller.ClosePanels,out var settings);
            controller.volumeSlider=settings.master;controller.volumeLabel=settings.masterValue;
            controller.howToPlayPanel=Tutorial(root,controller.ClosePanels);
            controller.shopPanel=Collection(root,controller.ClosePanels);
            var exit=Screen("ExitPanel",root);Frame(exit,"ConfirmSurface",120,570,840,760);Header(exit,"LEAVE THE GAME?",170,620,740,150);
            controller.exitMessage=T("ExitMessage",exit,"Your guardians will await your return.",200,850,680,180,36,Cream);
            Button("ConfirmExit",exit,"EXIT","Exit",230,1070,620,120,controller.ConfirmExit);
            Button("CancelExit",exit,"BACK","Back",280,1220,520,105,controller.ClosePanels);controller.exitPanel=exit.gameObject;
            controller.ClosePanels();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("PREMIUM_UI|Menu, settings, tutorial, collection authored as individual sprite and text objects.");
        }
        [MenuItem("SoulCrystalDefense/Premium Visuals/Style Gameplay")]
        public static void Gameplay()
        {
            if(EditorApplication.isPlaying)throw new Exception("Stop Play Mode first");ImportKit();
            var current=EditorSceneManager.GetActiveScene();if(current.isDirty)EditorSceneManager.SaveScene(current);
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/SoulCrystalDefenseGameplay.unity");
            var root=Find("GameplayComposition");var top=Find("TopHUD");
            var health=Find("CrystalHealthText");var wave=Find("WaveText");var energy=Find("EnergyText");var pause=Find("PauseButton");var fill=Find("HealthFill").GetComponent<Image>();
            foreach(var keep in new[]{health,wave,energy,pause,fill.transform}){keep.SetParent(top,false);keep.gameObject.SetActive(true);}
            foreach(var child in top.Cast<Transform>().ToArray())if(!new[]{health,wave,energy,pause,fill.transform}.Contains(child))child.gameObject.SetActive(false);
            top.GetComponent<Image>().color=new Color(.015f,.035f,.07f,1);
            Img("HealthPod",top,S("Frames/Button"),8,8,278,188).type=Image.Type.Sliced;
            Img("StagePod",top,S("Frames/Button"),288,8,470,188).type=Image.Type.Sliced;
            Img("WavePodPremium",top,S("Frames/Button"),760,8,312,188).type=Image.Type.Sliced;
            Img("CrystalEmblem",top,S("Icons/Crystal"),10,20,80,125);
            T("CrystalName",top,"SOUL CRYSTAL",87,28,180,38,23,Gold);PlaceText(health.GetComponent<Text>(),90,70,175,50,32,Cyan);
            Img("HealthTrackPremium",top,S("Frames/SliderTrack"),80,135,185,30).type=Image.Type.Sliced;
            Place(fill.rectTransform,88,143,167,14);fill.sprite=S("Frames/Button");fill.type=Image.Type.Filled;fill.fillMethod=Image.FillMethod.Horizontal;fill.fillOrigin=0;fill.color=Cyan;fill.raycastTarget=false;
            T("World",top,"WORLD I / SOUL CRYSTAL DEFENSE",320,25,402,32,22,Gold);
            T("Stage",top,"THE FORGOTTEN GRAVEYARD",315,70,412,45,26,Cream);
            Img("StageDivider",top,S("Frames/SliderTrack"),345,140,350,24).type=Image.Type.Sliced;
            PlaceText(wave.GetComponent<Text>(),783,30,185,43,30,Cream);
            Img("SoulEnergyIcon",top,S("Icons/Crystal"),785,104,45,60);
            PlaceText(energy.GetComponent<Text>(),837,105,125,40,32,Cyan);T("SoulLabel",top,"SOUL ENERGY",827,147,146,25,17,Cyan);
            var pi=pause.GetComponent<Image>();pi.sprite=S("Icons/Pause");pi.color=Color.white;pi.type=Image.Type.Simple;pi.preserveAspect=true;Place(pi.rectTransform,976,40,88,105);
            foreach(var text in pause.GetComponentsInChildren<Text>())text.gameObject.SetActive(false);
            fill.transform.SetAsLastSibling();health.SetAsLastSibling();wave.SetAsLastSibling();energy.SetAsLastSibling();pause.SetAsLastSibling();
            var tray=Find("BottomTray");Place(tray.GetComponent<RectTransform>(),0,1445,1080,475);tray.GetComponent<Image>().color=new Color(.015f,.035f,.07f,1);
            var viewport=Find("BattlefieldViewport");Place(viewport.GetComponent<RectTransform>(),0,205,1080,1240);
            var arts=root.GetComponent<CrystalArts>();
            for(int i=0;i<arts.buttons.Length;i++)
            {
                var b=arts.buttons[i];b.GetComponent<Image>().sprite=S("Frames/Button");b.GetComponent<Image>().type=Image.Type.Sliced;b.GetComponent<Image>().color=Color.white;Place(b.GetComponent<RectTransform>(),230+i*280,12,270,90);
                Img("AbilityIcon",b.transform,S("Icons/"+new[]{"Pulse","Storm","Barrier"}[i]),5,10,65,65);
                var label=b.transform.Find("Name").GetComponent<Text>();PlaceText(label,74,18,188,30,22,Cream);PlaceText(arts.cooldownLabels[i],74,52,188,25,18,Cyan);
            }
            var artsTitle=tray.Find("ArtsTitle");if(artsTitle)artsTitle.gameObject.SetActive(false);var artsSubtitle=tray.Find("ArtsSubtitle");if(artsSubtitle)artsSubtitle.gameObject.SetActive(false);
            Img("ArtsEmblem",tray,S("Icons/Crystal"),20,8,65,90);T("ArtsTitlePremium",tray,"CRYSTAL\nARTS",87,18,135,65,24,Gold);
            StyleCards(tray);
            var hint=Find("TrayTitle").GetComponent<Text>();PlaceText(hint,30,437,1020,34,23,Gold);hint.text="CHOOSE A GUARDIAN / TAP TO SUMMON / SWIPE FOR MORE";
            var flow=root.GetComponent<GameScreenFlow>()??root.gameObject.AddComponent<GameScreenFlow>();
            foreach(string n in new[]{"PauseScreen","BattleSettingsScreen","BattleTutorialScreen"}){var old=root.Find(n);if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);}
            var pp=Screen("PauseScreen",root);Frame(pp,"PauseFrame",90,300,900,1290);Header(pp,"GAME PAUSED",155,350,770,160);
            T("PauseSubtitle",pp,"THE BATTLE AWAITS YOUR RETURN",165,540,750,70,28,Cyan);
            Button("Resume",pp,"RESUME GAME","Play",185,690,710,145,flow.Resume);Button("Settings",pp,"SETTINGS","Settings",185,870,710,145,flow.Settings);
            Button("Tutorial",pp,"HOW TO PLAY","Book",185,1050,710,145,flow.Tutorial);Button("Menu",pp,"BACK TO MENU","Exit",185,1230,710,145,flow.Menu);flow.pausePanel=pp.gameObject;
            flow.settingsPanel=Settings(root,flow.Back,out var audio);flow.settingsPanel.name="BattleSettingsScreen";
            flow.tutorialPanel=Tutorial(root,flow.Back);flow.tutorialPanel.name="BattleTutorialScreen";
            flow.pausePanel.SetActive(false);flow.settingsPanel.SetActive(false);flow.tutorialPanel.SetActive(false);
            UnityEngine.Object.FindFirstObjectByType<GameplayHUD>().screenFlow=flow;
            var result=UnityEngine.Object.FindObjectsByType<VictoryDefeatPanel>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault();
            if(result){foreach(var image in result.GetComponentsInChildren<Image>(true).Where(im=>im.name.Contains("Panel")||im.name.Contains("Card"))){image.sprite=S("Frames/Panel");image.type=Image.Type.Sliced;image.preserveAspect=false;image.color=Color.white;}result.transform.SetAsLastSibling();}
            Canvas.ForceUpdateCanvases();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("PREMIUM_UI|Gameplay HUD, portraits, ability icons, pause, settings and tutorial saved.");
        }
        static void StyleCards(Transform tray)
        {
            var content=Find("CardsLayout").GetComponent<RectTransform>();
            var view=tray.Find("GuardianScroll") as RectTransform;if(!view)view=R("GuardianScroll",tray);Place(view,20,205,1040,225);
            var maskImage=view.GetComponent<Image>()??view.gameObject.AddComponent<Image>();maskImage.color=new Color(0,0,0,.01f);var mask=view.GetComponent<Mask>()??view.gameObject.AddComponent<Mask>();mask.showMaskGraphic=false;
            content.SetParent(view,false);Place(content,0,0,1518,220);
            var scroll=view.GetComponent<ScrollRect>()??view.gameObject.AddComponent<ScrollRect>();scroll.viewport=view;scroll.content=content;scroll.horizontal=true;scroll.vertical=false;scroll.movementType=ScrollRect.MovementType.Clamped;
            var group=content.GetComponent<HorizontalLayoutGroup>();group.spacing=16;group.padding=new RectOffset(4,4,4,4);group.childControlWidth=group.childControlHeight=true;group.childForceExpandWidth=false;group.childForceExpandHeight=true;
            var cards=content.GetComponentsInChildren<DefenderCardUI>();
            foreach(var card in cards)
            {
                var data=card.Data;string name=data.defenderType switch{Data.DefenderType.DwarfGuardian=>"Dwarf",Data.DefenderType.BloodChef=>"Chef",Data.DefenderType.SoulArcher=>"Archer",Data.DefenderType.BoneMage=>"Mage",Data.DefenderType.IronPaladin=>"Paladin",_=>"Witch"};
                var layout=card.GetComponent<LayoutElement>()??card.gameObject.AddComponent<LayoutElement>();layout.minWidth=layout.preferredWidth=235;
                var image=card.GetComponent<Image>();image.sprite=S("Frames/CardFrame");image.type=Image.Type.Sliced;image.preserveAspect=false;image.color=Color.white;
                foreach(var outline in card.GetComponents<Outline>())UnityEngine.Object.DestroyImmediate(outline);
                var so=new SerializedObject(card);so.FindProperty("presentationPortrait").objectReferenceValue=S("Portraits/"+name);so.ApplyModifiedPropertiesWithoutUndo();
                var icon=card.transform.Find("Icon").GetComponent<Image>();icon.sprite=S("Portraits/"+name);icon.preserveAspect=true;Place(icon.rectTransform,18,10,199,135);
                var label=card.transform.Find("NameText").GetComponent<Text>();PlaceText(label,12,143,211,34,24,Cream);
                var badge=card.transform.Find("CostBadge").GetComponent<RectTransform>();Place(badge,65,179,105,32);
                var selection=card.transform.Find("SelectionBorder").GetComponent<Image>();selection.sprite=S("Frames/SelectionFrame");selection.type=Image.Type.Sliced;selection.preserveAspect=false;selection.color=Color.white;
                card.RefreshVisuals();
            }
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,cards.Length*251+8);scroll.horizontalNormalizedPosition=0;
        }
        static RectTransform Screen(string n,Transform parent){var r=R(n,parent);Place(r,0,0,1080,1920);var shade=r.gameObject.AddComponent<Image>();shade.color=new Color(0,.015f,.04f,.86f);return r;}
        static void Frame(Transform p,string n,float x,float y,float w,float h){Img(n,p,S("Frames/Panel"),x,y,w,h).type=Image.Type.Sliced;}
        static void Header(Transform p,string title,float x,float y,float w,float h){Img("HeaderArt",p,S("Frames/TitleBanner"),x,y,w,h).type=Image.Type.Sliced;T("Heading",p,title,x+55,y+25,w-110,h-40,50,Cream);}
        static GameObject Settings(Transform parent,UnityAction close,out AudioSettingsPanel controls)
        {
            var r=Screen("SettingsPanel",parent);Frame(r,"SettingsFrame",90,300,900,1290);Header(r,"SETTINGS",160,340,760,170);controls=r.gameObject.AddComponent<AudioSettingsPanel>();
            controls.masterValue=T("MasterValue",r,"100%",820,610,130,55,36,Cyan);T("MasterLabel",r,"MASTER VOLUME",210,610,570,55,36,Cream);
            controls.master=Slider(r,"MasterVolume",controls.Master,590+115);
            controls.musicValue=T("MusicValue",r,"70%",820,850,130,55,36,Cyan);T("MusicLabel",r,"MUSIC",210,850,570,55,36,Cream);controls.music=Slider(r,"MusicVolume",controls.Music,945);
            controls.effectsValue=T("EffectsValue",r,"100%",820,1090,130,55,36,Cyan);T("EffectsLabel",r,"SOUND EFFECTS",210,1090,570,55,36,Cream);controls.effects=Slider(r,"EffectsVolume",controls.Effects,1185);
            T("SavedHint",r,"Audio preferences save automatically.",180,1325,720,60,28,Cream);Button("Back",r,"BACK","Back",250,1425,580,110,close);r.gameObject.SetActive(false);return r.gameObject;
        }
        static Slider Slider(Transform parent,string name,UnityAction<float> action,float y)
        {
            var bg=Img(name,parent,S("Frames/SliderTrack"),190,y,700,58);bg.type=Image.Type.Sliced;bg.raycastTarget=true;
            var fillArea=R("FillArea",bg.transform);Place(fillArea,25,15,650,28);var fill=Img("Fill",fillArea,S("Frames/Button"),0,0,650,28,Cyan);fill.type=Image.Type.Sliced;
            var handleArea=R("HandleArea",bg.transform);Place(handleArea,25,0,650,58);var handle=Img("Handle",handleArea,S("Icons/Crystal"),0,-13,72,88);
            var slider=bg.gameObject.AddComponent<Slider>();slider.minValue=0;slider.maxValue=1;slider.value=1;slider.fillRect=fill.rectTransform;slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;
            UnityEventTools.AddPersistentListener(slider.onValueChanged,action);return slider;
        }
        static GameObject Tutorial(Transform parent,UnityAction close)
        {
            var r=Screen("HowToPlayPanel",parent);Frame(r,"TutorialFrame",60,180,960,1510);Header(r,"HOW TO PLAY",160,200,760,165);
            string[] steps={"Choose a guardian card.\nSwipe to see the full squad.","Tap an empty tile to deploy.\nPlacement costs Soul Energy.","Guardians attack automatically.\nEnemies advance from the top.","Protect the Soul Crystal.\nUse crystal arts and reinforcements."};
            string[] icons={"Portraits/Dwarf","Portraits/Archer","Portraits/Chef","Icons/Crystal"};
            for(int i=0;i<4;i++){float y=410+i*275;Frame(r,"Step_"+(i+1),105,y,870,245);T("Number",r,(i+1).ToString(),125,y+60,90,95,58,Gold);T("Instructions_"+i,r,steps[i],230,y+45,440,160,31,Cream);Img("Illustration_"+i,r,S(icons[i]),705,y+20,225,200);}
            T("SupportTip",r,"ELEPHANT: HOLD A TILE  /  TRAIN: SWEEP A LANE",130,1530,820,70,25,Cyan);Button("Back",r,"BACK","Back",230,1740,620,115,close);r.gameObject.SetActive(false);return r.gameObject;
        }
        static GameObject Collection(Transform parent,UnityAction close)
        {
            var r=Screen("ShopPanel",parent);Frame(r,"CollectionFrame",55,140,970,1590);Header(r,"SOUL COLLECTION",150,130,780,150);T("Count",r,"6 GUARDIANS  /  2 REINFORCEMENTS",150,295,780,50,29,Cyan);
            var details=r.gameObject.AddComponent<CollectionDetails>();
            string[] titles={"HAMMER DWARF","BATTLE CHEF","SOUL ARCHER","BONE MAGE","IRON PALADIN","FROST WITCH","ROYAL ELEPHANT","SOUL TRAIN"};
            string[] roles={"Front-line tank","Splash attacker","Piercing arrows","Chain lightning","Armored guardian","Slow and control","Temporary bulwark","Emergency lane sweep"};
            string[] portraits={"Portraits/Dwarf","Portraits/Chef","Portraits/Archer","Portraits/Mage","Portraits/Paladin","Portraits/Witch","Supports/ElephantPortrait","Supports/TrainPortrait"};
            details.entries=new CollectionDetails.Entry[8];details.highlights=new GameObject[8];
            for(int i=0;i<8;i++)
            {
                float x=100+(i%2)*460,y=370+(i/2)*265;var im=Img("CollectionCard_"+i,r,S("Frames/CardFrame"),x,y,420,250);im.type=Image.Type.Sliced;im.raycastTarget=true;
                var b=im.gameObject.AddComponent<Button>();b.targetGraphic=im;UnityEventTools.AddIntPersistentListener(b.onClick,details.Select,i);
                Img("Portrait",im.transform,S(portraits[i]),15,12,145,175);T("Name",im.transform,titles[i],165,30,235,65,27,Cream);T("Role",im.transform,roles[i],170,100,225,70,23,Cyan);T("Available",im.transform,i<6?"INCLUDED":"ONCE / BATTLE",165,180,235,40,22,Gold);
                var selected=Img("Selected",im.transform,S("Frames/SelectionFrame"),0,0,420,250);selected.type=Image.Type.Sliced;details.highlights[i]=selected.gameObject;selected.gameObject.SetActive(false);
                details.entries[i]=new CollectionDetails.Entry{title=titles[i],role=roles[i],portrait=S(portraits[i]),description=i<6?"Included in your guardian squad.":i==6?"200 Soul. Choose an empty tile. Defends for 20 seconds; once per battle.":"150 Soul. Choose a lane. Sweeps upward, damaging each enemy once; once per battle."};
            }
            Frame(r,"DetailsSurface",100,1460,880,225);details.portrait=Img("SelectedPortrait",r,S(portraits[0]),115,1470,180,190);details.title=T("SelectedName",r,titles[0],320,1480,630,50,34,Gold);details.role=T("SelectedRole",r,roles[0],320,1535,630,35,26,Cyan);details.description=T("SelectedDescription",r,"Included in your squad.",320,1580,630,85,25,Cream);
            Button("Back",r,"BACK TO MENU","Back",230,1760,620,115,close);r.gameObject.SetActive(false);return r.gameObject;
        }
        static RectTransform R(string n,Transform p){var go=new GameObject(n,typeof(RectTransform));go.transform.SetParent(p,false);return go.GetComponent<RectTransform>();}
        static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
        static void Place(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);r.localScale=Vector3.one;}
        static Image Img(string n,Transform p,Sprite sprite,float x,float y,float w,float h,Color? color=null){var r=R(n,p);Place(r,x,y,w,h);var im=r.gameObject.AddComponent<Image>();im.sprite=sprite;im.color=color??Color.white;im.preserveAspect=!n.Contains("Frame");im.raycastTarget=false;return im;}
        static Text T(string n,Transform p,string text,float x,float y,float w,float h,int size,Color color){var r=R(n,p);var t=r.gameObject.AddComponent<Text>();t.font=font;t.text=text;PlaceText(t,x,y,w,h,size,color);return t;}
        static void PlaceText(Text t,float x,float y,float w,float h,int size,Color color){Place(t.rectTransform,x,y,w,h);t.font=font;t.color=color;t.fontSize=size;t.resizeTextForBestFit=true;t.resizeTextMinSize=Mathf.Max(18,size-5);t.resizeTextMaxSize=size;t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;}
        static Button Button(string n,Transform p,string label,string icon,float x,float y,float w,float h,UnityAction action){var im=Img(n,p,S("Frames/Button"),x,y,w,h);im.preserveAspect=false;im.type=Image.Type.Sliced;im.raycastTarget=true;var b=im.gameObject.AddComponent<Button>();b.targetGraphic=im;UnityEventTools.AddPersistentListener(b.onClick,action);Img("Icon",im.transform,S("Icons/"+icon),25,15,h-30,h-30);T("Label",im.transform,label,h+5,12,w-h-35,h-24,Mathf.Min(47,(int)(h*.3f)),Cream);return b;}
    }
}

