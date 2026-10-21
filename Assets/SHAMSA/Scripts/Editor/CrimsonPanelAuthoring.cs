using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using SoulCrystalDefense.UI;

namespace SoulCrystalDefense.EditorTools
{
    /// <summary>Writes native uGUI objects into scenes. No runtime layout component is installed.</summary>
    public static class CrimsonPanelAuthoring
    {
        const string Kit="Assets/Art Production From Cards/Panels/Production/";
        const string Buttons="Assets/Art Production From Cards/MainMenu/Buttons/";
        static TMP_FontAsset font;static Material textMaterial;static Sprite frame,plate;
        static readonly Color Ivory=new Color(1,.91f,.73f),White=Color.white;
        static T Ensure<T>(GameObject go)where T:Component{var c=go.GetComponent<T>();return c?c:go.AddComponent<T>();}
        static Sprite S(string path)=>AssetDatabase.LoadAssetAtPath<Sprite>(path);
        static void Import(string path,Vector4 border)
        {
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var i=(TextureImporter)AssetImporter.GetAtPath(path);
            i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;i.spriteBorder=border;i.spritePixelsPerUnit=100;i.alphaIsTransparency=true;i.mipmapEnabled=false;i.maxTextureSize=4096;i.npotScale=TextureImporterNPOTScale.None;i.textureCompression=TextureImporterCompression.Uncompressed;
            i.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="iPhone",overridden=true,maxTextureSize=4096,format=TextureImporterFormat.ASTC_4x4,compressionQuality=100});i.SaveAndReimport();
        }
        static void Prepare()
        {
            foreach(var p in Directory.GetFiles(Kit,"*.png"))Import(p.Replace('\\','/'),p.Contains("Stone-Panel")?new Vector4(105,105,105,105):p.Contains("Slider-Track")||p.Contains("Slider-Fill")?new Vector4(70,28,70,28):Vector4.zero);
            foreach(var p in Directory.GetFiles(Kit+"Steps","*.png"))Import(p.Replace('\\','/'),Vector4.zero);
            foreach(var p in new[]{"01-Dwarf-Chef-Crimson-Battle","02-Archer-Paladin-Crimson-Battle","03-Mage-Witch-Crimson-Battle"})Import("Assets/Art Production From Cards/"+p+".png",Vector4.zero);
            font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Final/PaybAck SDF.asset");textMaterial=AssetDatabase.LoadAssetAtPath<Material>(Buttons+"PAYBACK-White-Outline.mat");
            frame=S(Kit+"Stone-Panel.png");plate=S(Buttons+"Crimson-Button-Plate.png");
        }
        [MenuItem("SoulCrystalDefense/Presentation/Install Approved Crimson Panels")]
        public static void InstallAll()
        {
            if(EditorApplication.isPlaying)throw new System.Exception("Stop Play Mode first");Prepare();
            var menu=Object.FindFirstObjectByType<MainMenuController>();
            if(!menu)throw new System.Exception("Open MainMenu first");
            EditorSceneManager.SaveScene(menu.gameObject.scene,"ArtReview/CardCharacterPanels/MainMenu-live-before-panels.unity",true);
            InstallMenu(menu);EditorSceneManager.SaveScene(menu.gameObject.scene);
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/SoulCrystalDefenseGameplay.unity");
            EditorSceneManager.SaveScene(scene,"ArtReview/CardCharacterPanels/Gameplay-live-before-panels.unity",true);
            InstallGameplay();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");Debug.Log("CRIMSON_PANELS|Saved menu and gameplay panel hierarchies with editable art, TMP and controls");
        }
        static void InstallMenu(MainMenuController menu)
        {
            var settings=Shell(menu.settingsPanel,"SETTINGS",0);var values=new TextMeshProUGUI[3];var sliders=new Slider[3];
            UnityAction<float>[] changes={menu.SetVolume,menu.SetMusicVolume,menu.SetEffectsVolume};
            AudioRows(settings,changes,sliders,values);
            menu.volumeSlider=sliders[0];menu.musicSlider=sliders[1];menu.sfxSlider=sliders[2];
            menu.volumeLabel=null;menu.musicLabel=null;menu.sfxLabel=null;menu.volumeLabelTMP=values[0];menu.musicLabelTMP=values[1];menu.sfxLabelTMP=values[2];
            Footer(settings,menu.ClosePanels);
            Help(menu.howToPlayPanel,menu.ClosePanels,null);
            Shop(menu.shopPanel,menu.ClosePanels);
            var exit=Shell(menu.exitPanel,"LEAVE THE GAME?",0);
            Text("Subtitle",exit,"YOUR GUARDIANS WILL BE HERE WHEN YOU RETURN",.08f,.55f,.92f,.69f,37,Ivory);
            menu.exitMessage=null;menu.exitMessageTMP=Text("ExitMessage",exit,"READY TO LEAVE THE BATTLE?",.10f,.40f,.90f,.53f,34,White);
            Action("ConfirmExit",exit,"EXIT", "Exit",.12f,.23f,.88f,.35f,menu.ConfirmExit);
            Action("Back",exit,"BACK TO MENU","Play",.12f,.08f,.88f,.20f,menu.ClosePanels);
            foreach(var go in new[]{menu.settingsPanel,menu.howToPlayPanel,menu.shopPanel,menu.exitPanel})go.SetActive(false);
            EditorUtility.SetDirty(menu);EditorSceneManager.MarkSceneDirty(menu.gameObject.scene);
        }
        static void InstallGameplay()
        {
            var flow=Object.FindFirstObjectByType<GameScreenFlow>();if(!flow)throw new System.Exception("Gameplay screen flow missing");
            var pause=Shell(flow.pausePanel,"GAME PAUSED",0);
            Text("Subtitle",pause,"THE BATTLE AWAITS YOUR RETURN",.08f,.65f,.92f,.72f,30,Ivory);
            Action("Resume",pause,"RESUME GAME","Play",.10f,.49f,.90f,.61f,flow.Resume);
            Action("Settings",pause,"SETTINGS","Settings",.10f,.35f,.90f,.47f,flow.Settings);
            Action("HowToPlay",pause,"HOW TO PLAY","HowToPlay",.10f,.21f,.90f,.33f,flow.Tutorial);
            Action("Menu",pause,"MAIN MENU","Exit",.10f,.07f,.90f,.19f,flow.Menu);
            var audio=Ensure<AudioSettingsPanel>(flow.settingsPanel);
            var settings=Shell(flow.settingsPanel,"SETTINGS",0);var values=new TextMeshProUGUI[3];var sliders=new Slider[3];
            AudioRows(settings,new UnityAction<float>[]{audio.Master,audio.Music,audio.Effects},sliders,values);
            audio.master=sliders[0];audio.music=sliders[1];audio.effects=sliders[2];audio.masterValue=null;audio.musicValue=null;audio.effectsValue=null;
            audio.masterValueTMP=values[0];audio.musicValueTMP=values[1];audio.effectsValueTMP=values[2];EditorUtility.SetDirty(audio);Footer(settings,flow.Back);
            var tutorial=Object.FindFirstObjectByType<FirstWaveTutorial>(FindObjectsInactive.Include);
            Help(flow.tutorialPanel,flow.Back,tutorial?new UnityAction(tutorial.Replay):null);
            ResultPanels(flow);
            foreach(var go in new[]{flow.pausePanel,flow.settingsPanel,flow.tutorialPanel})go.SetActive(false);
        }
        static RectTransform Shell(GameObject root,string title,int header)
        {
            foreach(Transform child in root.transform.Cast<Transform>().ToArray())Object.DestroyImmediate(child.gameObject);
            var r=root.GetComponent<RectTransform>();r.localScale=Vector3.one;Anchor(r,0,0,1,1);
            var shade=Ensure<Image>(root);shade.sprite=null;shade.color=new Color(.018f,.003f,.008f,.9f);shade.raycastTarget=true;
            var content=R("AuthoredPanelContent",r,.05f,.045f,.95f,.955f);
            var bg=Picture("StoneFrame",content,frame,0,0,1,1);bg.type=Image.Type.Sliced;bg.pixelsPerUnitMultiplier=3.5f;
            Header(content,title,header);return content;
        }
        static void Header(RectTransform content,string title,int which)
        {
            string[] files={"01-Dwarf-Chef-Crimson-Battle","02-Archer-Paladin-Crimson-Battle","03-Mage-Witch-Crimson-Battle"};
            var art=Picture("CharacterHeader",content,S("Assets/Art Production From Cards/"+files[which]+".png"),.025f,.805f,.975f,.978f);art.preserveAspect=true;
            var strip=Picture("TitleStrip",content,plate,.055f,.724f,.945f,.817f);strip.type=Image.Type.Sliced;strip.pixelsPerUnitMultiplier=4;
            var titleText=Text("Heading",content,title,.08f,.732f,.92f,.809f,59,Ivory);titleText.fontSizeMin=32;titleText.textWrappingMode=TextWrappingModes.NoWrap;
        }
        static void AudioRows(RectTransform content,UnityAction<float>[] callbacks,Slider[] sliders,TextMeshProUGUI[] values)
        {
            string[] labels={"MASTER VOLUME","MUSIC","SOUND EFFECTS"};string[] icons={"Speaker","Music","Effects"};
            for(int i=0;i<3;i++)
            {
                float y=.527f-i*.195f;var row=Row("AudioRow_"+i,content,.055f,y,.945f,y+.176f);
                var icon=Picture("AudioIcon",row,S(Kit+"Audio-"+icons[i]+".png"),.035f,.26f,.16f,.77f);icon.preserveAspect=true;
                Text("AudioLabel",row,labels[i],.20f,.59f,.77f,.92f,36,White).alignment=TextAlignmentOptions.MidlineLeft;
                values[i]=Text("Percentage",row,i==1?"70%":"100%",.78f,.59f,.955f,.92f,36,Ivory);
                sliders[i]=Slider(row,callbacks[i]);sliders[i].SetValueWithoutNotify(i==1?.7f:1);
            }
            Text("SavedHint",content,"AUDIO PREFERENCES SAVE AUTOMATICALLY",.08f,.108f,.92f,.139f,23,Ivory);
        }
        static Slider Slider(RectTransform row,UnityAction<float> change)
        {
            var root=R("VolumeSlider",row,.20f,.12f,.95f,.52f);var hit=root.gameObject.AddComponent<Image>();hit.color=new Color(1,1,1,.001f);
            var track=Picture("Track",root,S(Kit+"Slider-Track.png"),0,.24f,1,.76f);track.type=Image.Type.Sliced;track.pixelsPerUnitMultiplier=4;
            var area=R("FillArea",root,.025f,.29f,.975f,.71f);var fill=Picture("Fill",area,S(Kit+"Slider-Fill.png"),0,0,1,1);fill.type=Image.Type.Sliced;fill.pixelsPerUnitMultiplier=4;
            var handleArea=R("HandleSlideArea",root,.025f,0,.975f,1);var handle=R("Handle",handleArea,.5f,.5f,.5f,.5f);handle.sizeDelta=new Vector2(75,75);
            var graphic=handle.gameObject.AddComponent<Image>();graphic.sprite=S(Kit+"Slider-Knob.png");graphic.preserveAspect=true;graphic.raycastTarget=false;
            var slider=root.gameObject.AddComponent<Slider>();slider.minValue=0;slider.maxValue=1;slider.fillRect=fill.rectTransform;slider.handleRect=handle;slider.targetGraphic=graphic;slider.direction=UnityEngine.UI.Slider.Direction.LeftToRight;
            UnityEventTools.AddPersistentListener(slider.onValueChanged,change);return slider;
        }
        static void Help(GameObject root,UnityAction back,UnityAction replay)
        {
            var content=Shell(root,"HOW TO PLAY",1);
            string[] titles={"CHOOSE A GUARDIAN","DEPLOY ON A TILE","AUTO ATTACK","DEFEND THE CRYSTAL"};
            string[] copy={"SELECT A GUARDIAN CARD.","TAP AN EMPTY TILE.\nSPEND SOUL ENERGY.","GUARDIANS ATTACK ENEMIES IN THEIR LANE.","STOP ENEMIES BEFORE THEY REACH YOUR CRYSTAL."};
            string[] steps={"Choose-Guardian","Deploy-Tile","Auto-Attack","Defend-Crystal"};
            for(int i=0;i<4;i++)
            {
                float y=.565f-i*.151f;var row=Row("InstructionStep_"+(i+1),content,.045f,y,.955f,y+.142f);
                var art=Picture("GameplayIllustration",row,S(Kit+"Steps/"+steps[i]+".png"),.60f,.05f,.98f,.95f);art.preserveAspect=true;
                var number=Text("StepNumber",row,(i+1).ToString(),.02f,.20f,.115f,.80f,54,Ivory);number.fontSizeMin=30;
                var heading=Text("StepHeading",row,titles[i],.13f,.61f,.60f,.93f,31,Ivory);heading.alignment=TextAlignmentOptions.MidlineLeft;heading.textWrappingMode=TextWrappingModes.NoWrap;
                var body=Text("Instruction",row,copy[i],.13f,.08f,.58f,.59f,27,White);body.alignment=TextAlignmentOptions.MidlineLeft;
            }
            Text("EnergyHint",content,"SOUL ENERGY REGENERATES OVER TIME",.08f,.083f,.92f,.112f,22,Ivory);
            if(replay!=null){Action("Back",content,"BACK","Exit",.07f,.018f,.43f,.081f,back);Action("ReplayTutorial",content,"REPLAY TUTORIAL","Play",.46f,.018f,.93f,.081f,replay);}
            else Action("Back",content,"BACK","Exit",.19f,.018f,.81f,.081f,back);
        }
        static void Shop(GameObject root,UnityAction back)
        {
            var content=Shell(root,"SOUL SHOP",2);Text("CollectionLabel",content,"YOUR DEFENDER COLLECTION",.10f,.684f,.90f,.721f,26,Ivory);
            string[] names={"DWARF","CHEF","ARCHER","MAGE","PALADIN","FROST WITCH"};string[] files={"Dwarf","Chef","Archer","Mage","Paladin","Witch"};
            for(int i=0;i<6;i++)
            {
                int column=i%2,row=i/2;float x=.045f+column*.465f,y=.517f-row*.170f;
                var card=Row("GuardianCard_"+files[i],content,x,y,x+.445f,y+.16f);
                var artRoot=R("PortraitViewport",card,.025f,.255f,.975f,.975f);artRoot.gameObject.AddComponent<RectMask2D>();
                var image=Picture("Portrait",artRoot,S("Assets/UI/Final/Production/Portraits/"+files[i]+".png"),0,0,1,1);image.preserveAspect=true;
                Text("GuardianName",card,names[i],.03f,.115f,.97f,.27f,31,Ivory).textWrappingMode=TextWrappingModes.NoWrap;
                Text("Status",card,"IN SQUAD",.15f,.015f,.85f,.12f,22,White);
            }
            var support=Row("Reinforcements",content,.10f,.121f,.90f,.171f);
            Text("SupportNames",support,"ROYAL ELEPHANT / SOUL TRAIN",.04f,.47f,.96f,.94f,25,Ivory);
            Text("SupportStatus",support,"REINFORCEMENTS INCLUDED",.04f,.05f,.96f,.49f,20,White);
            Text("PurchaseStatus",content,"NO PURCHASES AVAILABLE YET",.10f,.087f,.90f,.119f,22,White);
            Action("Back",content,"BACK","Exit",.19f,.018f,.81f,.082f,back);
        }
        static void ResultPanels(GameScreenFlow flow)
        {
            var result=Object.FindFirstObjectByType<VictoryDefeatPanel>(FindObjectsInactive.Include);if(!result)return;
            var serialized=new SerializedObject(result);var root=serialized.FindProperty("panelRoot").objectReferenceValue as GameObject;if(!root)root=result.gameObject;
            if(root!=result.gameObject&&result.transform.IsChildOf(root.transform))throw new System.Exception("Result panel script must be retained outside rebuilt children");
            var victory=Shell(root,"VICTORY",1);victory.name="VictoryContent";
            Text("VictoryCopy",victory,"YOUR GUARDIANS HELD THE LINE",.08f,.64f,.92f,.70f,32,Ivory);
            var stars=new Image[3];for(int i=0;i<3;i++){float x=.30f+i*.14f;stars[i]=Picture("Star_"+i,victory,S(Kit+"Victory-Star.png"),x,.555f,x+.12f,.63f);stars[i].preserveAspect=true;}
            var reward=Text("Reward",victory,"CRYSTAL FRAGMENTS",.10f,.45f,.90f,.55f,36,White);
            var total=Text("TotalFragments",victory,"TOTAL FRAGMENTS",.10f,.34f,.90f,.44f,30,White);
            var replay=Action("Replay",victory,"PLAY AGAIN","Play",.12f,.18f,.88f,.29f,null);Action("Menu",victory,"MAIN MENU","Exit",.12f,.05f,.88f,.16f,flow.Menu);
            // Build the second content without clearing the already authored victory objects.
            var defeat=R("DefeatContent",root.transform,.05f,.045f,.95f,.955f);var bg=Picture("StoneFrame",defeat,frame,0,0,1,1);bg.type=Image.Type.Sliced;bg.pixelsPerUnitMultiplier=3.5f;Header(defeat,"DEFEAT",0);
            Text("DefeatCopy",defeat,"THE CRYSTAL NEEDS YOU AGAIN",.08f,.57f,.92f,.69f,37,Ivory);
            var hint=Text("DefeatHint",defeat,"REBUILD YOUR DEFENSE AND TRY AGAIN",.10f,.35f,.90f,.55f,34,White);
            var retry=Action("Retry",defeat,"TRY AGAIN","Play",.12f,.18f,.88f,.29f,null);Action("Menu",defeat,"MAIN MENU","Exit",.12f,.05f,.88f,.16f,flow.Menu);
            void Set(string name,Object value)=>serialized.FindProperty(name).objectReferenceValue=value;
            Set("panelRoot",root);Set("victoryContainer",victory.gameObject);Set("defeatContainer",defeat.gameObject);
            Set("rewardFragmentText",null);Set("totalFragmentsText",null);Set("defeatHintText",null);Set("victoryTitleText",null);Set("defeatTitleText",null);
            Set("rewardFragmentTMP",reward);Set("totalFragmentsTMP",total);Set("defeatHintTMP",hint);Set("victoryReplayButton",replay);Set("victoryNextButton",null);Set("defeatRetryButton",retry);
            serialized.FindProperty("starImages").arraySize=3;for(int i=0;i<3;i++)serialized.FindProperty("starImages").GetArrayElementAtIndex(i).objectReferenceValue=stars[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(result);victory.gameObject.SetActive(false);defeat.gameObject.SetActive(false);root.SetActive(false);root.transform.SetAsLastSibling();
        }
        static void Footer(RectTransform parent,UnityAction close)=>Action("Back",parent,"BACK","Exit",.19f,.02f,.81f,.095f,close);
        static RectTransform Row(string name,Transform parent,float x,float y,float xx,float yy){var r=R(name,parent,x,y,xx,yy);var image=r.gameObject.AddComponent<Image>();image.sprite=frame;image.type=Image.Type.Sliced;image.pixelsPerUnitMultiplier=7;image.raycastTarget=false;return r;}
        static Button Action(string name,Transform parent,string caption,string icon,float x,float y,float xx,float yy,UnityAction call)
        {
            var root=R(name,parent,x,y,xx,yy);var hit=root.gameObject.AddComponent<Image>();hit.color=new Color(1,1,1,.001f);var b=root.gameObject.AddComponent<Button>();
            var visual=R("CrimsonVisual",root,0,0,1,1);var surface=visual.gameObject.AddComponent<Image>();surface.sprite=plate;surface.type=Image.Type.Sliced;surface.pixelsPerUnitMultiplier=4;surface.raycastTarget=false;
            var graphic=Picture("Icon",visual,S(Buttons+"Icons/Icon-"+icon+".png"),.045f,.17f,.17f,.83f);graphic.preserveAspect=true;
            var text=Text("Label",visual,caption,.19f,.10f,.95f,.90f,46,White);text.textWrappingMode=TextWrappingModes.NoWrap;text.fontSizeMin=21;
            b.targetGraphic=surface;b.transition=Selectable.Transition.ColorTint;var colors=b.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1,.95f,.85f);colors.pressedColor=new Color(.7f,.55f,.55f);colors.selectedColor=Color.white;b.colors=colors;
            if(call!=null)UnityEventTools.AddPersistentListener(b.onClick,call);return b;
        }
        static TextMeshProUGUI Text(string name,Transform parent,string caption,float x,float y,float xx,float yy,float size,Color color)
        {
            var t=R(name,parent,x,y,xx,yy).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.fontSharedMaterial=textMaterial;t.text=caption;t.fontSize=t.fontSizeMax=size;t.fontSizeMin=Mathf.Max(16,size*.68f);t.enableAutoSizing=true;t.alignment=TextAlignmentOptions.Center;t.textWrappingMode=TextWrappingModes.Normal;t.overflowMode=TextOverflowModes.Truncate;t.color=color;t.raycastTarget=false;t.margin=new Vector4(3,1,3,1);return t;
        }
        static Image Picture(string name,Transform parent,Sprite sprite,float x,float y,float xx,float yy){var i=R(name,parent,x,y,xx,yy).gameObject.AddComponent<Image>();i.sprite=sprite;i.raycastTarget=false;return i;}
        static RectTransform R(string name,Transform parent,float x,float y,float xx,float yy){var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var r=go.GetComponent<RectTransform>();Anchor(r,x,y,xx,yy);return r;}
        static void Anchor(RectTransform r,float x,float y,float xx,float yy){r.anchorMin=new Vector2(x,y);r.anchorMax=new Vector2(xx,yy);r.pivot=new Vector2(.5f,.5f);r.offsetMin=r.offsetMax=Vector2.zero;}
    }
}
