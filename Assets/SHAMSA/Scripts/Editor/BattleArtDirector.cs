using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using SoulCrystalDefense.UI;
using SoulCrystalDefense.Grid;
using SoulCrystalDefense.Systems;
using SoulCrystalDefense.Presentation;

namespace SoulCrystalDefense.EditorTools
{
    public static class BattleArtDirector
    {
        static readonly Color Ink = new Color(.025f,.038f,.065f,.98f);
        static readonly Color Cyan = new Color(.27f,.85f,.92f,1);
        static readonly Color Gold = new Color(.82f,.66f,.37f,1);
        static readonly Color Bone = new Color(.91f,.91f,.84f,1);
        const string Folder = "Assets/SHAMSA/Visuals/Polish/";
        static Font font;
        static Sprite rounded;
        static Transform Find(string name) => Resources.FindObjectsOfTypeAll<Transform>().FirstOrDefault(t=>t.gameObject.scene.IsValid() && t.name==name);

        [MenuItem("SoulCrystalDefense/Apply Moonlit Presentation")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Stop Play Mode before authoring presentation.");
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(scene.path!="Assets/Scenes/SoulCrystalDefenseGameplay.unity") scene=EditorSceneManager.OpenScene("Assets/Scenes/SoulCrystalDefenseGameplay.unity");
            font=Find("WaveText").GetComponent<Text>().font;
            CreateRoundedSprite();
            // Existing polisher preserves all live HUD and button bindings.
            SoulCrystalDefenseUIPolisher.ApplyAndSave();
            scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var top=Find("TopHUD"); var tray=Find("BottomTray");
            top.GetComponent<Image>().color=Ink;
            foreach(var n in new[]{"CrystalHealthPod","WavePod","EnergyPod"})
            {
                var image=Find(n).GetComponent<Image>(); image.sprite=rounded; image.type=Image.Type.Sliced;
                image.color=new Color(.045f,.072f,.105f,1);
                foreach(var o in image.GetComponents<Outline>()) Object.DestroyImmediate(o);
            }
            Find("CrystalLabel").GetComponent<Text>().color=Gold;
            Find("ArenaLabel").GetComponent<Text>().text="THE FORGOTTEN GATE";
            Find("ArenaLabel").GetComponent<Text>().fontSize=14;
            Find("HealthFill").GetComponent<Image>().color=Cyan;
            Find("CrystalHealthText").GetComponent<Text>().text="20/20";
            Find("EnergyText").GetComponent<Text>().text="250";
            Find("WaveText").GetComponent<Text>().text="WAVE 1/20";
            Rect(Find("EnergyPod").GetComponent<RectTransform>(),new Vector2(1,.5f),new Vector2(-200,0),new Vector2(200,88));
            Rect(Find("PauseButton").GetComponent<RectTransform>(),new Vector2(1,.5f),new Vector2(-63,0),new Vector2(78,78));
            Find("PauseButton").GetComponent<Image>().sprite=rounded;
            Find("PauseButton").GetComponent<Image>().type=Image.Type.Sliced;
            tray.GetComponent<RectTransform>().sizeDelta=new Vector2(0,286);
            tray.GetComponent<Image>().color=Ink;
            Find("TrayTitle").GetComponent<Text>().text="CHOOSE A DEFENDER  •  TAP A TILE TO SUMMON";
            Find("TrayTitle").GetComponent<RectTransform>().sizeDelta=new Vector2(1000,28);
            Find("TrayTitle").GetComponent<Text>().color=Gold;
            var layout=Find("CardsLayout").GetComponent<HorizontalLayoutGroup>();
            layout.childControlWidth=true; layout.childControlHeight=true; layout.childForceExpandWidth=true; layout.childForceExpandHeight=true;
            layout.spacing=18;
            var lr=layout.GetComponent<RectTransform>(); lr.anchorMin=Vector2.zero; lr.anchorMax=Vector2.one;
            lr.offsetMin=new Vector2(30,24); lr.offsetMax=new Vector2(-30,-48);
            foreach(var card in tray.GetComponentsInChildren<DefenderCardUI>(true)) Card(card);
            var countdown=Find("CountdownContainer");
            countdown.GetComponent<Image>().sprite=rounded; countdown.GetComponent<Image>().type=Image.Type.Sliced;
            Rect(countdown.GetComponent<RectTransform>(),new Vector2(.5f,.5f),new Vector2(0,430),new Vector2(240,100));
            countdown.GetComponentInChildren<Text>(true).fontSize=54;
            // Neutral tiles reveal the stone artwork; highlighted tiles remain clearly visible.
            foreach(var cell in Object.FindObjectsByType<PlacementCell>(FindObjectsSortMode.None))
            {
                var so=new SerializedObject(cell);
                so.FindProperty("cellRenderer").objectReferenceValue=cell.GetComponent<SpriteRenderer>();
                so.FindProperty("normalColor").colorValue=new Color(.35f,.65f,.72f,.045f);
                so.FindProperty("validColor").colorValue=new Color(.25f,.9f,1,.32f);
                so.FindProperty("invalidColor").colorValue=new Color(1,.28f,.2f,.38f);
                so.ApplyModifiedPropertiesWithoutUndo(); cell.SetVisualState(PlacementCell.CellVisualState.Normal);
            }
            var backdrop=Find("GraveyardBackdrop").GetComponent<SpriteRenderer>(); backdrop.color=new Color(.72f,.81f,.90f,1);
            Atmosphere(); HudEvents(top);
            var camera=Camera.main;
            camera.transform.position=new Vector3(0,-.4f,-10);
            camera.orthographicSize=7.15f;
            foreach(var text in Find("HUDCanvas").GetComponentsInChildren<Text>(true)) text.raycastTarget=false;
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("[Art Direction] Moonlit presentation authored and saved; gameplay data untouched.");
        }
        static void Card(DefenderCardUI card)
        {
            var root=card.transform; var image=root.GetComponent<Image>(); image.sprite=rounded; image.type=Image.Type.Sliced;
            image.color=new Color(.07f,.10f,.14f,1);
            foreach(var outline in root.GetComponents<Outline>()) Object.DestroyImmediate(outline);
            var inner=root.Find("InnerShade"); if(inner!=null) inner.gameObject.SetActive(false);
            var icon=root.Find("Icon").GetComponent<Image>(); icon.gameObject.SetActive(true); icon.enabled=true; icon.color=Color.white;
            icon.sprite=card.Data!=null ? card.Data.cardIcon : AssetDatabase.LoadAllAssetsAtPath(root.name=="Card_Elephant" ? "Assets/SHAMSA/Assets/Elephant/Elephant_Ideal.png" : "Assets/SHAMSA/Assets/Train/Train_Ideal.png").OfType<Sprite>().FirstOrDefault();
            if(!card.IsLocked)
            {
                string portraitPath=Folder+root.name+"_Portrait.asset";
                var portrait=AssetDatabase.LoadAssetAtPath<Sprite>(portraitPath);
                if(portrait==null)
                {
                    var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(root.name=="Card_Dwarf"?"Assets/Screenshots/Crop_Card1_Dwarf.png":"Assets/Screenshots/Crop_Card2_Chef.png");
                    portrait=Sprite.Create(texture,new Rect(13,48,165,159),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
                    portrait.name=root.name+"_Portrait";AssetDatabase.CreateAsset(portrait,portraitPath);
                }
                var data=new SerializedObject(card);data.FindProperty("presentationPortrait").objectReferenceValue=portrait;data.ApplyModifiedPropertiesWithoutUndo();icon.sprite=portrait;
            }
            icon.preserveAspect=true; Rect(icon.rectTransform,new Vector2(.5f,.5f),new Vector2(0,4),new Vector2(180,145));
            var name=root.Find("NameText").GetComponent<Text>(); Rect(name.rectTransform,new Vector2(.5f,1),new Vector2(0,-23),new Vector2(224,32));
            name.fontSize=21; name.color=Bone; name.alignment=TextAnchor.MiddleCenter;
            name.horizontalOverflow=HorizontalWrapMode.Wrap; name.resizeTextForBestFit=true; name.resizeTextMinSize=16; name.resizeTextMaxSize=21;
            var badge=root.Find("CostBadge"); Rect(badge.GetComponent<RectTransform>(),new Vector2(.5f,0),new Vector2(0,29),new Vector2(104,38));
            badge.GetComponent<Image>().sprite=rounded; badge.GetComponent<Image>().type=Image.Type.Sliced;
            var selection=root.Find("SelectionBorder"); selection.SetAsLastSibling();
            selection.GetComponent<Image>().sprite=rounded; selection.GetComponent<Image>().type=Image.Type.Sliced;
            selection.GetComponent<Image>().color=new Color(.2f,.85f,1,.12f);
            var overlay=root.Find("UnaffordableOverlay").GetComponent<Image>(); overlay.color=new Color(.015f,.025f,.045f,.54f); overlay.raycastTarget=false;
            var locked=root.Find("LockOverlay"); locked.GetComponent<Image>().color=new Color(.015f,.025f,.045f,.62f);
            var glyph=locked.Find("LockGlyph"); if(glyph!=null) glyph.gameObject.SetActive(false);
            var lockText=locked.GetComponentInChildren<Text>(true); Rect(lockText.rectTransform,new Vector2(.5f,0),new Vector2(0,49),new Vector2(210,38)); lockText.fontSize=18; lockText.color=Gold;
            if(card.IsLocked) {name.text=root.name=="Card_Elephant" ? "WAR ELEPHANT" : "WAR TRAIN"; badge.gameObject.SetActive(false);}
            locked.SetAsLastSibling();
        }
        static void HudEvents(Transform top)
        {
            var root=Find("SafeAreaRoot");
            var status=Label(top,"EncounterStatus","PREPARE YOUR DEFENSE",new Vector2(.5f,0),new Vector2(0,-32),new Vector2(600,28),17,Gold);
            var track=Panel(top,"EncounterTrack",new Vector2(.5f,0),new Vector2(0,-9),new Vector2(980,4),new Color(.1f,.17f,.22f,1));
            var fill=Panel(track.transform,"Progress",new Vector2(.5f,.5f),Vector2.zero,new Vector2(980,4),Cyan); fill.type=Image.Type.Filled; fill.fillMethod=Image.FillMethod.Horizontal; fill.fillAmount=0;
            var banner=Panel(root,"WaveAnnouncement",new Vector2(.5f,1),new Vector2(0,-260),new Vector2(660,138),new Color(.02f,.035f,.06f,.94f));
            var cg=banner.GetComponent<CanvasGroup>(); if(cg==null) cg=banner.gameObject.AddComponent<CanvasGroup>(); cg.alpha=0; cg.blocksRaycasts=false; cg.interactable=false;
            var title=Label(banner.transform,"Title","WAVE 01",new Vector2(.5f,.5f),new Vector2(0,20),new Vector2(620,60),42,Bone);
            var detail=Label(banner.transform,"Detail","DEFEND THE SOUL CRYSTAL",new Vector2(.5f,.5f),new Vector2(0,-33),new Vector2(620,30),18,Gold);
            var p=top.GetComponent<BattlePresentation>(); if(p==null) p=top.gameObject.AddComponent<BattlePresentation>();
            p.announcement=cg;p.announcementTitle=title;p.announcementDetail=detail;p.encounterStatus=status;p.waveProgress=fill;
        }
        static void Atmosphere()
        {
            var old=Find("MoonlitAtmosphere"); if(old!=null) Object.DestroyImmediate(old.gameObject);
            var root=new GameObject("MoonlitAtmosphere").transform;
            var shader=Shader.Find("SoulCrystal/AtmosphericEnergy");
            var ring=Material("PortalEnergy",shader,new Color(.55f,.28f,1,.8f),0);
            var soul=Material("CrystalEnergy",shader,new Color(.15f,.8f,1,.7f),0);
            var mist=Material("GroundMist",shader,new Color(.25f,.55f,.65f,.32f),1);
            var pixel=AssetDatabase.LoadAssetAtPath<Sprite>(Folder+"EnergyQuad.png");
            foreach(var portal in Object.FindObjectsByType<PortalVisual>(FindObjectsSortMode.None))
            {
                var portalRenderer=portal.GetComponent<SpriteRenderer>();
                portalRenderer.sprite=pixel;portalRenderer.sharedMaterial=ring;
                portal.transform.localScale=new Vector3(1.35f,.65f,1);
                var so=new SerializedObject(portal); so.FindProperty("telegraphColor").colorValue=new Color(1,.4f,.65f,1);so.ApplyModifiedPropertiesWithoutUndo();
            }
            var core=Find("CrystalCore");
            Quad(root,"SanctuarySigil",pixel,soul,core.position+Vector3.down*.2f,new Vector3(2,.7f,1),2);
            var platform=Find("PlatformBase"); if(platform!=null) platform.GetComponent<SpriteRenderer>().color=new Color(.07f,.15f,.2f,.22f);
            for(int i=0;i<6;i++) Quad(root,"DriftingMist_"+i,pixel,mist,new Vector3(i%2==0 ? -2.3f:2.3f,-3.6f+i*1.5f,0),new Vector3(3.5f,1.8f,1),-4);
            // A single bounded particle system gives soul motes without per-frame allocations.
            var go=new GameObject("SoulMotes");go.transform.SetParent(root);var ps=go.AddComponent<ParticleSystem>();
            var main=ps.main; main.startLifetime=7;main.startSpeed=.08f;main.startSize=new ParticleSystem.MinMaxCurve(.015f,.035f);main.maxParticles=36; main.startColor=new Color(.35f,.85f,1,.45f);main.simulationSpace=ParticleSystemSimulationSpace.World;
            var emission=ps.emission;emission.rateOverTime=4;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(5.8f,9,0);
            var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=soul;renderer.sortingOrder=5;
        }
        static Material Material(string name,Shader shader,Color color,float mode)
        {
            string path=Folder+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){m=new Material(shader);AssetDatabase.CreateAsset(m,path);}m.SetColor("_Color",color);m.SetFloat("_Mode",mode);return m;
        }
        static void Quad(Transform parent,string name,Sprite sprite,Material mat,Vector3 pos,Vector3 scale,int order)
        {
            var g=new GameObject(name);g.transform.SetParent(parent);g.transform.position=pos;g.transform.localScale=scale;var sr=g.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.sharedMaterial=mat;sr.sortingOrder=order;
        }
        static void CreateRoundedSprite()
        {
            const int size=64;var tex=new Texture2D(size,size,TextureFormat.RGBA32,false);
            for(int y=0;y<size;y++)for(int x=0;x<size;x++) {float dx=Mathf.Max(Mathf.Abs(x-31.5f)-20,0),dy=Mathf.Max(Mathf.Abs(y-31.5f)-20,0);tex.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(12-Mathf.Sqrt(dx*dx+dy*dy))));}
            System.IO.File.WriteAllBytes(Folder+"RoundedPanel.png",tex.EncodeToPNG());Object.DestroyImmediate(tex);
            tex=new Texture2D(2,2);tex.SetPixels(new[]{Color.white,Color.white,Color.white,Color.white});tex.Apply();System.IO.File.WriteAllBytes(Folder+"EnergyQuad.png",tex.EncodeToPNG());Object.DestroyImmediate(tex);
            AssetDatabase.Refresh();
            foreach(var name in new[]{"RoundedPanel","EnergyQuad"}) {var imp=(TextureImporter)AssetImporter.GetAtPath(Folder+name+".png");imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;imp.spritePixelsPerUnit=name=="EnergyQuad"?2:100;imp.spriteBorder=name=="RoundedPanel"?new Vector4(15,15,15,15):Vector4.zero;imp.alphaIsTransparency=true;imp.mipmapEnabled=false;imp.SaveAndReimport();}
            rounded=AssetDatabase.LoadAssetAtPath<Sprite>(Folder+"RoundedPanel.png");
        }
        static void Rect(RectTransform r,Vector2 anchor,Vector2 pos,Vector2 size){r.anchorMin=r.anchorMax=anchor;r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;}
        static Image Panel(Transform parent,string name,Vector2 anchor,Vector2 pos,Vector2 size,Color color)
        {
            var t=parent.Find(name);var g=t!=null?t.gameObject:new GameObject(name,typeof(RectTransform));g.transform.SetParent(parent,false);Rect(g.GetComponent<RectTransform>(),anchor,pos,size);var i=g.GetComponent<Image>();if(i==null)i=g.AddComponent<Image>();i.sprite=rounded;i.type=Image.Type.Sliced;i.color=color;i.raycastTarget=false;return i;
        }
        static Text Label(Transform parent,string name,string value,Vector2 anchor,Vector2 pos,Vector2 size,int fontSize,Color color)
        {
            var t=parent.Find(name);var g=t!=null?t.gameObject:new GameObject(name,typeof(RectTransform));g.transform.SetParent(parent,false);Rect(g.GetComponent<RectTransform>(),anchor,pos,size);var text=g.GetComponent<Text>();if(text==null)text=g.AddComponent<Text>();text.font=font;text.text=value;text.fontSize=fontSize;text.alignment=TextAnchor.MiddleCenter;text.color=color;text.raycastTarget=false;return text;
        }
    }
}
