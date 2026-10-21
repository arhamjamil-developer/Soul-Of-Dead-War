using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using SoulCrystalDefense.Combat;
using SoulCrystalDefense.Data;
using SoulCrystalDefense.Grid;
using SoulCrystalDefense.Presentation;
using SoulCrystalDefense.Systems;
using SoulCrystalDefense.UI;
using SoulCrystalDefense.Units;

namespace SoulCrystalDefense.EditorTools
{
    public static class GameplayMockupAuthoring
    {
        const string Art="Assets/UI/Gameplay/";
        static readonly Color Ink=new Color(.04f,.085f,.13f,1),Gold=new Color(.85f,.72f,.47f),Cyan=new Color(.44f,.9f,.92f),Cream=new Color(.96f,.92f,.82f);
        static Font font;
        static Sprite panel;
        static Transform Find(string name)=>UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(t=>t.name==name);
        [MenuItem("SoulCrystalDefense/Implement Animated Gameplay Mockup")]
        public static void Build()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            var scene=EditorSceneManager.GetActiveScene();
            if(scene.path!="Assets/Scenes/SoulCrystalDefenseGameplay.unity")throw new InvalidOperationException("Open the gameplay scene first.");
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            panel=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SHAMSA/Visuals/Polish/RoundedPanel.png");
            var boardSprite=Import("Battlefield",100,false);
            var archer=Import("SoulArcher",800,true);var mage=Import("BoneMage",800,true);
            var archerData=Guardian("SoulArcher",DefenderType.SoulArcher,archer,150,95,22,1.1f);
            var mageData=Guardian("BoneMage",DefenderType.BoneMage,mage,175,80,26,1.65f);
            var board=UnityEngine.Object.FindFirstObjectByType<BoardGrid>();
            var bs=new SerializedObject(board);bs.FindProperty("laneSpacing").floatValue=1.35f;bs.ApplyModifiedPropertiesWithoutUndo();
            foreach(var cell in board.GetComponentsInChildren<PlacementCell>())
            {
                cell.transform.position=board.CalculateCellPosition(cell.Lane,cell.Row);
                var box=cell.GetComponent<BoxCollider2D>();if(box)box.size=new Vector2(1.2f,.9f);
            }
            foreach(var portal in UnityEngine.Object.FindObjectsByType<PortalVisual>(FindObjectsSortMode.None))portal.transform.position=board.GetPortalPosition(portal.LaneIndex);
            var bg=Find("GraveyardBackdrop").GetComponent<SpriteRenderer>();bg.sprite=boardSprite;bg.color=Color.white;
            bg.transform.localScale=new Vector3(8.3f/boardSprite.bounds.size.x,11.9f/boardSprite.bounds.size.y,1);bg.transform.position=Vector3.zero;
            var canvas=Find("HUDCanvas").GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;
            canvas.GetComponent<CanvasScaler>().referenceResolution=new Vector2(1080,1920);
            var safe=Find("SafeAreaRoot").GetComponent<RectTransform>();
            var composition=Find("GameplayComposition") as RectTransform;
            if(!composition)
            {
                composition=Rect("GameplayComposition",safe);composition.anchorMin=composition.anchorMax=new Vector2(.5f,.5f);composition.sizeDelta=new Vector2(1080,1920);
                foreach(var child in safe.Cast<Transform>().Where(c=>c!=composition).ToArray())child.SetParent(composition,false);
            }
            var viewport=GetRect("BattlefieldViewport",composition);Place(viewport,0,205,1080,1350);
            var camera=Camera.main;camera.transform.position=new Vector3(0,.05f,-10);camera.orthographicSize=6.05f;
            var backdropCamera=Find("GameplayBackdropCamera");
            if(!backdropCamera){var go=new GameObject("GameplayBackdropCamera",typeof(Camera));var c=go.GetComponent<Camera>();c.cullingMask=0;c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=Ink;c.depth=-100;}
            camera.depth=0;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Ink;
            var top=Find("TopHUD").GetComponent<RectTransform>();top.SetParent(composition,false);Place(top,0,0,1080,205);top.GetComponent<Image>().color=Ink;
            var health=Find("CrystalHealthText");var energy=Find("EnergyText");var wave=Find("WaveText");var pause=Find("PauseButton");var fill=Find("HealthFill").GetComponent<Image>();
            foreach(var keep in new[]{health,energy,wave,pause,fill.transform})keep.SetParent(top,false);
            foreach(var child in top.Cast<Transform>().ToArray())if(!new[]{health,energy,wave,pause,fill.transform}.Contains(child))child.gameObject.SetActive(false);
            Text("WorldLabel",top,"WORLD I  /  SOUL CRYSTAL DEFENSE",35,20,660,35,24,Gold);
            Text("StageTitle",top,"THE FORGOTTEN GRAVEYARD",35,63,710,48,31,Cream);
            ConfigureText(wave.GetComponent<Text>(),"WAVE 1/20",790,30,190,65,28,Cream);
            Place(pause.GetComponent<RectTransform>(),995,30,60,65);
            Text("HealthLabel",top,"SOUL CRYSTAL",75,119,225,27,20,Gold);
            ConfigureText(health.GetComponent<Text>(),"20 / 20",75,146,220,40,28,Cream);
            var crystalSprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SHAMSA/Visuals/SoulCrystal_Sprite.png");
            Image("CrystalHealthIcon",top,crystalSprite,30,125,35,56,Color.white);
            Image("HealthTrackV2",top,panel,300,143,350,20,new Color(.02f,.04f,.07f));Place(fill.rectTransform,300,143,350,20);fill.color=Cyan;fill.transform.SetAsLastSibling();
            Image("EnergyIconV2",top,crystalSprite,717,131,30,45,Color.white);
            ConfigureText(energy.GetComponent<Text>(),"250",760,132,115,55,36,Cyan);
            Text("EnergyLabelV2",top,"SOUL ENERGY",886,139,163,40,20,new Color(.65f,.75f,.8f));
            Image("TopRule",top,null,0,203,1080,2,Gold);
            var tray=Find("BottomTray").GetComponent<RectTransform>();tray.SetParent(composition,false);Place(tray,0,1554,1080,366);tray.GetComponent<Image>().color=Ink;
            var cardsLayout=Find("CardsLayout").GetComponent<RectTransform>();Place(cardsLayout,26,120,1028,198); // Tray-local coordinates.
            var group=cardsLayout.GetComponent<HorizontalLayoutGroup>();group.spacing=22;group.padding=new RectOffset();group.childControlWidth=group.childControlHeight=true;group.childForceExpandHeight=group.childForceExpandWidth=true;
            var title=Find("TrayTitle").GetComponent<Text>();ConfigureText(title,"SELECT A GUARDIAN  /  TAP A TILE TO SUMMON",20,330,1040,32,23,new Color(.67f,.77f,.82f));
            var cards=cardsLayout.GetComponentsInChildren<DefenderCardUI>(true);
            for(int i=0;i<cards.Length;i++)
            {
                var card=cards[i];if(i==2)card.Setup(archerData,false);if(i==3)card.Setup(mageData,false);
                var so=new SerializedObject(card);so.FindProperty("presentationPortrait").objectReferenceValue=card.Data.cardIcon;so.ApplyModifiedPropertiesWithoutUndo();
                var root=card.transform;card.GetComponent<Image>().color=new Color(.08f,.14f,.2f);
                var outline=card.GetComponent<Outline>()??card.gameObject.AddComponent<Outline>();outline.effectColor=Gold;outline.effectDistance=new Vector2(1,-1);
                var icon=root.Find("Icon").GetComponent<Image>();icon.sprite=card.Data.cardIcon;icon.preserveAspect=true;Local(icon.rectTransform,0,-5,205,135,new Vector2(.5f,.7f));
                var label=root.Find("NameText").GetComponent<Text>();Local(label.rectTransform,0,44,228,35,new Vector2(.5f,0));label.fontSize=24;label.resizeTextMaxSize=24;label.resizeTextMinSize=18;
                var cost=root.Find("CostBadge").GetComponent<RectTransform>();cost.gameObject.SetActive(true);Local(cost,0,20,100,32,new Vector2(.5f,0));
                var lockObj=root.Find("LockOverlay");lockObj.gameObject.SetActive(false);
                var old=root.Find("InnerShade");if(old)old.gameObject.SetActive(false);
                card.RefreshVisuals();
            }
            var arts=composition.GetComponent<CrystalArts>()??composition.gameObject.AddComponent<CrystalArts>();
            arts.presentation=top.GetComponent<BattlePresentation>();arts.buttons=new Button[3];arts.cooldownLabels=new Text[3];arts.cooldownFills=new Image[3];
            Text("ArtsTitle",tray,"CRYSTAL",30,20,125,30,20,Gold);Text("ArtsSubtitle",tray,"ARTS",30,52,125,26,18,Cream);
            string[] names={"SOUL PULSE","CRYSTAL STORM","DIVINE BARRIER"};UnityAction[] actions={arts.SoulPulse,arts.CrystalStorm,arts.DivineBarrier};
            for(int i=0;i<3;i++)
            {
                var button=Image("Art_"+i,tray,panel,163+i*284,20,255,85,new Color(.09f,.17f,.23f));button.raycastTarget=true;
                var b=button.GetComponent<Button>()??button.gameObject.AddComponent<Button>();b.targetGraphic=button;while(b.onClick.GetPersistentEventCount()>0)UnityEventTools.RemovePersistentListener(b.onClick,0);UnityEventTools.AddPersistentListener(b.onClick,actions[i]);arts.buttons[i]=b;
                var shade=Image("Cooldown",button.transform,panel,0,0,255,85,new Color(.02f,.04f,.07f,.5f));shade.type=UnityEngine.UI.Image.Type.Filled;shade.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;shade.fillAmount=0;arts.cooldownFills[i]=shade;
                Text("Name",button.transform,names[i],4,9,247,30,22,Cream);
                arts.cooldownLabels[i]=Text("State",button.transform,"READY",4,47,247,25,19,Cyan);
            }
            var bossPanel=Image("BossPanel",composition,panel,225,221,630,80,new Color(.12f,.06f,.1f,.95f));
            arts.bossPanel=bossPanel.gameObject;arts.bossLabel=Text("BossLabel",bossPanel.transform,"BOSS  /  ROYAL COLOSSUS",0,5,630,34,26,Gold);
            var bossFill=Image("Health",bossPanel.transform,panel,25,53,580,10,new Color(.8f,.35f,.4f));bossFill.type=UnityEngine.UI.Image.Type.Filled;bossFill.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;arts.bossHealth=bossFill;bossPanel.gameObject.SetActive(false);
            var announcer=arts.presentation;
            if(announcer)
            {
                var announcement=announcer.announcement;announcement.transform.SetParent(composition,false);Place(announcement.GetComponent<RectTransform>(),200,1330,680,105);announcement.transform.SetAsLastSibling();
                ConfigureText(announcer.announcementTitle,"WAVE 01",10,10,660,45,36,Gold);ConfigureText(announcer.announcementDetail,"DEFEND THE SOUL CRYSTAL",10,60,660,30,22,Cream);
                if(announcer.encounterStatus)announcer.encounterStatus.gameObject.SetActive(false);if(announcer.waveProgress)announcer.waveProgress.gameObject.SetActive(false);
            }
            var modal=Find("VictoryDefeatModal");if(modal){modal.SetParent(composition,false);var mr=modal.GetComponent<RectTransform>();mr.anchorMin=Vector2.zero;mr.anchorMax=Vector2.one;mr.offsetMin=mr.offsetMax=Vector2.zero;modal.SetAsLastSibling();}
            var countdown=Find("CountdownContainer");if(countdown)Local(countdown.GetComponent<RectTransform>(),0,-250,240,100,new Vector2(.5f,1));
            var barrier=Find("DivineBarrierVisual");
            if(!barrier){barrier=new GameObject("DivineBarrierVisual",typeof(SpriteRenderer)).transform;barrier.position=new Vector3(0,-4.5f,0);barrier.localScale=new Vector3(3,2.1f,1);}
            var br=barrier.GetComponent<SpriteRenderer>();br.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SHAMSA/Visuals/Polish/EnergyQuad.png");br.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/SHAMSA/Visuals/Polish/CrystalEnergy.mat");br.sortingOrder=7;br.enabled=false;arts.barrierVisual=br;
            var fx=UnityEngine.Object.FindFirstObjectByType<GameplayEffects>();if(!fx)fx=new GameObject("GameplayEffects").AddComponent<GameplayEffects>();fx.lineMaterial=LineMaterial();fx.damageFont=font;
            Canvas.ForceUpdateCanvases();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("GAMEPLAY_MOCKUP|AUTHORED|4 defenders, 3 crystal arts, corruption, responsive canvas and separate artwork.");
        }
        static Sprite Import(string name,float ppu,bool alpha)
        {
            string path=Art+name+".png";AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var i=(TextureImporter)AssetImporter.GetAtPath(path);i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;i.spritePixelsPerUnit=ppu;i.alphaIsTransparency=alpha;i.mipmapEnabled=false;i.textureCompression=TextureImporterCompression.Uncompressed;i.maxTextureSize=2048;i.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        static Material LineMaterial()
        {
            string path=Art+"BattleFeedback.mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Sprites/Default"));AssetDatabase.CreateAsset(m,path);}return m;
        }
        static DefenderData Guardian(string name,DefenderType kind,Sprite sprite,int cost,float health,float damage,float cooldown)
        {
            string dataPath="Assets/SHAMSA/GameplayData/"+name+"_Data.asset";
            var data=AssetDatabase.LoadAssetAtPath<DefenderData>(dataPath);if(!data){data=ScriptableObject.CreateInstance<DefenderData>();AssetDatabase.CreateAsset(data,dataPath);}
            data.defenderType=kind;data.displayName=kind==DefenderType.SoulArcher?"SOUL ARCHER":"BONE MAGE";data.soulCost=cost;data.maxHealth=health;data.damage=damage;data.attackCooldown=cooldown;data.attackRange=5.5f;data.isRanged=true;data.splashRadius=0;data.cardIcon=sprite;
            var go=new GameObject(name);var visual=new GameObject("Visual",typeof(SpriteRenderer));visual.transform.SetParent(go.transform,false);visual.GetComponent<SpriteRenderer>().sprite=sprite;visual.GetComponent<SpriteRenderer>().sortingOrder=4;
            var hp=new GameObject("HealthBar",typeof(HealthBar));hp.transform.SetParent(go.transform,false);hp.transform.localPosition=new Vector3(0,.65f,0);
            var unit=go.AddComponent<DefenderUnit>();var so=new SerializedObject(unit);so.FindProperty("defenderData").objectReferenceValue=data;so.FindProperty("spriteRenderer").objectReferenceValue=visual.GetComponent<SpriteRenderer>();so.FindProperty("healthBar").objectReferenceValue=hp.GetComponent<HealthBar>();so.ApplyModifiedPropertiesWithoutUndo();
            var motion=go.AddComponent<GuardianMotion>();motion.visual=visual.transform;
            string prefabPath="Assets/SHAMSA/Prefabs/"+name+".prefab";data.unitPrefab=PrefabUtility.SaveAsPrefabAsset(go,prefabPath);UnityEngine.Object.DestroyImmediate(go);
            if(kind==DefenderType.SoulArcher)
            {
                var projectile=new GameObject("SoulArrow",typeof(SpriteRenderer),typeof(Projectile));projectile.GetComponent<SpriteRenderer>().sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SHAMSA/Visuals/SoulFragment_Sprite.png");projectile.GetComponent<SpriteRenderer>().color=Cyan;projectile.GetComponent<SpriteRenderer>().sortingOrder=8;projectile.transform.localScale=new Vector3(.07f,.2f,1);
                data.projectilePrefab=PrefabUtility.SaveAsPrefabAsset(projectile,"Assets/SHAMSA/Prefabs/SoulArrow.prefab");UnityEngine.Object.DestroyImmediate(projectile);
            }
            EditorUtility.SetDirty(data);return data;
        }
        static RectTransform Rect(string name,Transform parent){var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);return go.GetComponent<RectTransform>();}
        static RectTransform GetRect(string name,Transform parent){var t=parent.Find(name);return t?t.GetComponent<RectTransform>():Rect(name,parent);}
        // Pixel coordinates relative to each authored parent's own top-left.
        static void Place(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);r.localScale=Vector3.one;}
        static void Local(RectTransform r,float x,float y,float w,float h,Vector2 anchor){r.anchorMin=r.anchorMax=anchor;r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);}
        static Image Image(string name,Transform parent,Sprite sprite,float x,float y,float w,float h,Color color){var r=GetRect(name,parent);r.gameObject.SetActive(true);Place(r,x,y,w,h);var im=r.GetComponent<Image>()??r.gameObject.AddComponent<Image>();im.sprite=sprite;im.type=sprite==panel?UnityEngine.UI.Image.Type.Sliced:UnityEngine.UI.Image.Type.Simple;im.color=color;im.raycastTarget=false;return im;}
        static Text Text(string name,Transform parent,string value,float x,float y,float w,float h,int size,Color color){var r=GetRect(name,parent);r.gameObject.SetActive(true);var text=r.GetComponent<Text>()??r.gameObject.AddComponent<Text>();text.font=font;ConfigureText(text,value,x,y,w,h,size,color);return text;}
        static void ConfigureText(Text t,string value,float x,float y,float w,float h,int size,Color color){Place(t.rectTransform,x,y,w,h);t.font=font;t.text=value;t.fontSize=size;t.color=color;t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;t.resizeTextForBestFit=true;t.resizeTextMinSize=Mathf.Max(14,size-5);t.resizeTextMaxSize=size;}
    }
}

