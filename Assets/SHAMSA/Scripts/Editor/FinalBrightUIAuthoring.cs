using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SoulCrystalDefense.UI;
using SoulCrystalDefense.Data;
using SoulCrystalDefense.Grid;
using SoulCrystalDefense.Systems;
using SoulCrystalDefense.Presentation;
namespace SoulCrystalDefense.EditorTools {
public static class FinalBrightUIAuthoring {
const string A="Assets/UI/Final/Production/";
static readonly Color White=Color.white,Blue=new Color(.025f,.12f,.34f),Cyan=new Color(.12f,1,1),Gold=new Color(1,.83f,.13f);
static TMP_FontAsset paybackFont;
static TMP_FontAsset magicFont;
static Sprite S(string p)=>AssetDatabase.LoadAssetAtPath<Sprite>(A+p+".png");
static Transform Find(string n)=>UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(t=>t.name==n);
public static void Import(){
    paybackFont=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Final/PaybAck SDF.asset");
    magicFont=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Final/MagicBaloonDemoRegular SDF.asset");
    if(!paybackFont)paybackFont=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset");
    if(!magicFont)magicFont=paybackFont;
}
[MenuItem("SoulCrystalDefense/Final Bright UI/Apply Reference Art")]
public static void Apply(){
if(EditorApplication.isPlaying)throw new Exception("Stop Play Mode before authoring");
Import();var scene=EditorSceneManager.OpenScene("Assets/Scenes/SoulCrystalDefenseGameplay.unity");
var root=Find("GameplayComposition");var top=Find("TopHUD");var tray=Find("BottomTray");
var health=Find("CrystalHealthText");var wave=Find("WaveText");var energy=Find("EnergyText");var fill=Find("HealthFill");var pause=Find("PauseButton");
var keep=new[]{health,wave,energy,fill,pause};foreach(var t in keep)t.SetParent(top,false);
foreach(var t in top.Cast<Transform>().ToArray())if(!keep.Contains(t))UnityEngine.Object.DestroyImmediate(t.gameObject);
Place(top.GetComponent<RectTransform>(),0,0,1080,185);top.GetComponent<Image>().color=new Color(.15f,.7f,1);
Image("HealthPod",top,S("Frames/GoldPanel"),7,9,309,168);Image("StagePod",top,S("Frames/GoldPanel"),320,9,432,168);Image("EnergyPod",top,S("Frames/GoldPanel"),756,9,317,168);
Image("CrystalEmblem",top,S("Icons/Crystal"),-4,5,112,151,false);
Label("CrystalName",top,"SOUL CRYSTAL",103,24,195,32,22,White,true);
MoveLabel(health,104,65,195,57,48,White,false);
var track=Image("HealthTrack",top,AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),92,130,201,28);track.color=new Color(.09f,.35f,.12f);var f=fill.GetComponent<Image>();Place(f.rectTransform,96,134,193,20);f.sprite=track.sprite;f.type=UnityEngine.UI.Image.Type.Filled;f.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;f.fillOrigin=0;f.color=new Color(.3f,1,.02f);f.raycastTarget=false;
var world=Label("World",top,"REALM 1 / SOUL CRYSTAL DEFENSE",338,25,396,30,20,Gold,false);var stage=Label("Stage",top,"THE FORGOTTEN GRAVEYARD",335,69,402,52,27,White,false);
var bar=Image("StageDivider",top,track.sprite,354,140,365,4);bar.color=new Color(.4f,.95f,1);for(int i=0;i<3;i++)Image("RealmGem_"+i,top,S("Icons/Gem"),510+i*20,128,18,i==1?28:20,false);
MoveLabel(wave,774,27,193,44,30,White,false);Image("WaveInset",top,S("Frames/Inset"),774,28,193,46);wave.SetAsLastSibling();
Image("SoulEnergyIcon",top,S("Icons/Storm"),775,87,67,82,false);Image("EnergyInset",top,S("Frames/Inset"),844,91,119,43);MoveLabel(energy,845,89,117,47,36,Cyan,false);Label("SoulLabel",top,"SOUL ENERGY",837,135,151,28,20,Cyan,true);
var pi=pause.GetComponent<Image>();pi.sprite=S("Icons/Pause");pi.type=UnityEngine.UI.Image.Type.Simple;pi.color=White;pi.preserveAspect=true;Place(pi.rectTransform,973,29,91,94);foreach(var t in pause.GetComponentsInChildren<Text>(true))t.gameObject.SetActive(false);
foreach(var t in keep)t.SetAsLastSibling();
Place(Find("BattlefieldViewport").GetComponent<RectTransform>(),0,185,1080,1155);
Place(tray.GetComponent<RectTransform>(),0,1340,1080,580);Skin(tray.GetComponent<Image>(),S("Frames/Tray"));
var arts=root.GetComponent<CrystalArts>();var support=root.GetComponent<BattleReinforcements>();
foreach(var t in tray.Cast<Transform>().ToArray())if(!arts.buttons.Any(b=>b.transform==t)&&t!=support.elephantButton.transform&&t!=support.trainButton.transform&&t.name!="GuardianScroll")t.gameObject.SetActive(false);
var artTitle=Image("CrystalArtsHolder",tray,S("Frames/GoldPanel"),14,12,241,112);Image("CrystalIcon",artTitle.transform,S("Icons/Crystal"),7,5,85,98,false);Label("Title",artTitle.transform,"CRYSTAL\nARTS",91,20,140,68,24,White,false);
string[] abilityNames={"SOUL PULSE","CRYSTAL STORM","DIVINE BARRIER"};string[] abilityIcons={"Pulse","Storm","Barrier"};
for(int i=0;i<3;i++){
var b=arts.buttons[i];Skin(b.GetComponent<Image>(),S("Frames/GoldPanel"));Place(b.GetComponent<RectTransform>(),265+i*267,12,258,112);
foreach(var child in b.transform.Cast<Transform>().ToArray())if(child!=arts.cooldownLabels[i].transform&&child.GetComponent<Text>()==null)child.gameObject.SetActive(false);
Image("ReferenceIcon",b.transform,S("Icons/"+abilityIcons[i]),5,10,80,90,false);
var title=b.transform.Find("Name");MoveLabel(title,86,20,163,37,20,White,false);title.GetComponent<Text>().text=abilityNames[i];
MoveLabel(arts.cooldownLabels[i].transform,87,61,161,32,20,new Color(.1f,1,.67f),true);arts.cooldownFills[i].color=new Color(.03f,.06f,.25f,.42f);}
StyleSupport(support.elephantButton,"ROYAL ELEPHANT","Elephant",14,136,support.elephantState);StyleSupport(support.trainButton,"SOUL TRAIN","Train",546,136,support.trainState);
BuildCards(tray);
var hint=Image("GuardianHint",tray,S("Frames/HintBanner"),51,519,978,55);Label("Instructions",hint.transform,"CHOOSE A GUARDIAN / TAP TO SUMMON / SWIPE FOR MORE",38,9,902,36,22,White,true);
StyleOverlays(root);
var presentation=UnityEngine.Object.FindFirstObjectByType<BattlePresentation>();if(presentation){presentation.waveProgress=bar;var announcement=presentation.announcement;if(announcement){Skin(announcement.GetComponent<Image>(),S("Frames/GoldPanel"));Place(announcement.GetComponent<RectTransform>(),145,1160,790,146);MoveLabel(presentation.announcementTitle.transform,30,18,730,52,36,Gold,false);MoveLabel(presentation.announcementDetail.transform,30,75,730,44,24,White,true);}}
var atmosphere=UnityEngine.Object.FindFirstObjectByType<BattleAtmosphere>();if(atmosphere){atmosphere.stageLabel=stage;atmosphere.worldLabel=world;atmosphere.realms=new[]{S("World/Graveyard-Modular"),S("World/Graveyard-Modular"),S("World/Graveyard-Modular"),S("World/Graveyard-Modular")};atmosphere.enabled=false;}
WorldArt();
EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("FINAL_BRIGHT_UI|Saved reference art with PaybAck & MagicBaloon SDF fonts, short names");
}
static RectTransform CreatePill(RectTransform parent,string name,Vector2 anchor,Vector2 pos,Vector2 size,string label,TMP_FontAsset f){
    var existing=parent.Find(name);if(existing)UnityEngine.Object.DestroyImmediate(existing.gameObject);
    var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image),typeof(Button));
    go.transform.SetParent(parent,false);
    var rt=go.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=rt.pivot=anchor;rt.anchoredPosition=pos;rt.sizeDelta=size;
    var img=go.GetComponent<Image>();img.color=new Color(.05f,.12f,.32f,.92f);
    var outline=go.AddComponent<Outline>();outline.effectColor=new Color(1,.83f,.13f,.9f);outline.effectDistance=new Vector2(1,-1);
    var lGO=new GameObject("Label",typeof(RectTransform),typeof(CanvasRenderer),typeof(TextMeshProUGUI));
    lGO.transform.SetParent(go.transform,false);
    var lRT=lGO.GetComponent<RectTransform>();lRT.anchorMin=Vector2.zero;lRT.anchorMax=Vector2.one;lRT.offsetMin=new Vector2(2,1);lRT.offsetMax=new Vector2(-2,-1);
    var tmp=lGO.GetComponent<TextMeshProUGUI>();tmp.font=f;tmp.text=label;tmp.fontSize=12;tmp.alignment=TextAlignmentOptions.Center;tmp.color=White;tmp.raycastTarget=false;
    return rt;
}
static void StyleSupport(Button b,string title,string icon,float x,float y,Text state){Skin(b.GetComponent<Image>(),S("Frames/GoldPanel"));Place(b.GetComponent<RectTransform>(),x,y,518,112);foreach(var im in b.GetComponentsInChildren<Image>())if(im.name=="Icon"){im.sprite=S("Icons/"+icon);im.color=White;im.preserveAspect=true;Place(im.rectTransform,8,-4,180,115);}MoveLabel(b.transform.Find("Title"),190,19,310,39,24,White,false);b.transform.Find("Title").GetComponent<Text>().text=title;state.text=icon=="Elephant"?"200 SOUL / ONCE":"150 SOUL / ONCE";MoveLabel(state.transform,192,60,305,35,22,Cyan,true);}
static void BuildCards(Transform tray){
var scroll=tray.Find("GuardianScroll").GetComponent<ScrollRect>();Place(scroll.GetComponent<RectTransform>(),13,260,1054,250);var content=scroll.content;foreach(var t in content.Cast<Transform>().ToArray())UnityEngine.Object.DestroyImmediate(t.gameObject);content.gameObject.SetActive(true);scroll.gameObject.SetActive(true);Place(content,0,0,1576,250);
var group=content.GetComponent<HorizontalLayoutGroup>();group.spacing=18;group.padding=new RectOffset(4,4,0,0);group.childControlWidth=group.childControlHeight=true;group.childForceExpandWidth=group.childForceExpandHeight=false;
var holder=Image("GuardianCard",null,S("Frames/CardHolder"),0,0,244,250);holder.raycastTarget=true;var go=holder.gameObject;var b=go.AddComponent<Button>();b.targetGraphic=holder;var nav=b.navigation;nav.mode=Navigation.Mode.None;b.navigation=nav;var layout=go.AddComponent<LayoutElement>();layout.minWidth=layout.preferredWidth=244;layout.minHeight=layout.preferredHeight=250;
var portrait=Image("Portrait",go.transform,null,11,5,222,166,false);var name=Label("Name",go.transform,"GUARDIAN",12,170,220,29,22,Blue,false);var cost=Image("CostBadge",go.transform,S("Frames/CostPill"),22,203,200,40);Image("Currency",cost.transform,S("Icons/Gem"),28,5,28,30,false);var price=Label("Cost",cost.transform,"0",65,3,105,33,26,Blue,false);
var selected=Image("SelectedBorder",go.transform,S("Frames/CardHolder"),0,0,244,250);selected.fillCenter=false;selected.color=new Color(1,.88f,.18f);selected.gameObject.SetActive(false);
var overlay=Image("Unaffordable",go.transform,AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),0,0,244,250);overlay.color=new Color(.07f,.15f,.3f,.24f);overlay.gameObject.SetActive(false);
var card=go.AddComponent<DefenderCardUI>();var so=new SerializedObject(card);Set(so,"iconImage",portrait);Set(so,"nameText",name);Set(so,"costText",price);Set(so,"selectionBorder",selected);Set(so,"unaffordableOverlay",overlay.gameObject);Set(so,"cardButton",b);so.FindProperty("affordableTextColor").colorValue=Blue;so.FindProperty("unaffordableTextColor").colorValue=new Color(.55f,.15f,.08f);so.ApplyModifiedPropertiesWithoutUndo();
string dir=A+"Prefabs";System.IO.Directory.CreateDirectory(dir);var prefab=PrefabUtility.SaveAsPrefabAsset(go,dir+"/GuardianCard.prefab");UnityEngine.Object.DestroyImmediate(go);
string[] dataNames={"DwarfGuardian","BloodChef","SoulArcher","BoneMage","IronPaladin","FrostWitch"};
string[] shortNames={"Dwarf","Chef","Archer","Mage","Paladin","Witch"};
string[] pictures={"Dwarf","Chef","Archer","Mage","Paladin","Witch"};var cards=new UnityEngine.Object[6];
for(int i=0;i<6;i++){var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,content);instance.name="Card_"+dataNames[i];var c=instance.GetComponent<DefenderCardUI>();var data=AssetDatabase.LoadAssetAtPath<DefenderData>("Assets/SHAMSA/GameplayData/"+dataNames[i]+"_Data.asset");if(data)data.displayName=shortNames[i];var cs=new SerializedObject(c);Set(cs,"defenderData",data);Set(cs,"presentationPortrait",S("Portraits/"+pictures[i]));cs.ApplyModifiedPropertiesWithoutUndo();c.RefreshVisuals();foreach(var o in instance.GetComponentsInChildren<Component>(true))if(o)PrefabUtility.RecordPrefabInstancePropertyModifications(o);cards[i]=c;}
var placement=UnityEngine.Object.FindFirstObjectByType<PlacementController>();var ps=new SerializedObject(placement);var array=ps.FindProperty("cards");array.arraySize=6;for(int i=0;i<6;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=cards[i];ps.ApplyModifiedPropertiesWithoutUndo();scroll.horizontalNormalizedPosition=0;
}
static void WorldArt(){
var backdrop=Find("GraveyardBackdrop").GetComponent<SpriteRenderer>();backdrop.sprite=S("World/Graveyard-Modular");backdrop.color=White;var material=AssetDatabase.LoadAssetAtPath<Material>(A+"World/DaylightSprites.mat");if(!material){material=new Material(Shader.Find("Sprites/Default"));AssetDatabase.CreateAsset(material,A+"World/DaylightSprites.mat");}backdrop.sharedMaterial=material;backdrop.transform.position=new Vector3(0,.3f,0);backdrop.transform.localScale=new Vector3(9.4501f/backdrop.sprite.bounds.size.x,12.917f/backdrop.sprite.bounds.size.y,1);
foreach(string n in new[]{"MoonlitAtmosphere","Global_PostProcessing"}){var go=GameObject.Find(n);if(go)go.SetActive(false);}
foreach(var sr in Find("Portals").GetComponentsInChildren<SpriteRenderer>())sr.enabled=false;
var platform=Find("SoulCrystalPlatform");foreach(var sr in platform.GetComponentsInChildren<SpriteRenderer>())sr.enabled=false;
var crystal=platform.Find("CrystalCore").GetComponent<SpriteRenderer>();crystal.enabled=true;crystal.sprite=S("Icons/Gem");crystal.color=White;crystal.sharedMaterial=backdrop.sharedMaterial;crystal.transform.localScale=Vector3.one*(1.48f/crystal.sprite.bounds.size.y);
var oldRoot=GameObject.Find("BrightBattlefieldPatches");if(oldRoot)UnityEngine.Object.DestroyImmediate(oldRoot);var patches=new GameObject("BrightBattlefieldPatches");
foreach(var cell in UnityEngine.Object.FindObjectsByType<PlacementCell>(FindObjectsSortMode.None)){
var old=cell.transform.Find("StonePatchArt");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
var child=new GameObject("StonePatch_L"+cell.Lane+"_R"+cell.Row,typeof(SpriteRenderer));child.transform.SetParent(patches.transform,false);child.transform.position=cell.transform.position;
var tile=child.GetComponent<SpriteRenderer>();tile.sprite=S("World/StonePatch_"+((cell.Lane==2)?0:1+(cell.Lane%3)));tile.sortingOrder=-13;tile.sharedMaterial=backdrop.sharedMaterial;tile.color=White;child.transform.localScale=new Vector3(1.29f/tile.sprite.bounds.size.x,.94f/tile.sprite.bounds.size.y,1);
var sr=cell.GetComponent<SpriteRenderer>();sr.enabled=true;sr.sharedMaterial=backdrop.sharedMaterial;sr.sortingOrder=-12;sr.sprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");sr.drawMode=SpriteDrawMode.Sliced;sr.size=new Vector2(1.29f/cell.transform.lossyScale.x,.94f/cell.transform.lossyScale.y);var cs=new SerializedObject(cell);cs.FindProperty("normalColor").colorValue=new Color(1,1,1,0);cs.FindProperty("validColor").colorValue=new Color(.3f,1,.8f,.3f);cs.FindProperty("invalidColor").colorValue=new Color(1,.15f,.2f,.35f);cs.ApplyModifiedPropertiesWithoutUndo();sr.color=new Color(1,1,1,0);
}
var sky=GameObject.Find("GameplayBackdropCamera");if(sky)sky.GetComponent<Camera>().backgroundColor=new Color(.18f,.72f,1);var camera=Camera.main;if(camera)camera.backgroundColor=new Color(.25f,.75f,.93f);
}
static void StyleOverlays(Transform root){foreach(var im in root.GetComponentsInChildren<Image>(true))if(im.transform.parent&&im.transform.parent.name!="TopHUD"&&new[]{"PauseFrame","SettingsFrame","TutorialFrame","HeaderArt"}.Contains(im.name)){Skin(im,S(im.name=="HeaderArt"?"Frames/HintBanner":"Frames/GoldPanel"));}foreach(var im in root.GetComponentsInChildren<Image>(true))if(im.name.StartsWith("Step_"))Skin(im,S("Frames/GoldPanel"));}
static void Set(SerializedObject so,string n,UnityEngine.Object v){so.FindProperty(n).objectReferenceValue=v;}
static RectTransform Rect(string n,Transform parent){var go=new GameObject(n,typeof(RectTransform));if(parent)go.transform.SetParent(parent,false);return go.GetComponent<RectTransform>();}
static Image Image(string n,Transform parent,Sprite sprite,float x,float y,float w,float h,bool sliced=true){var r=Rect(n,parent);Place(r,x,y,w,h);var im=r.gameObject.AddComponent<Image>();im.sprite=sprite;im.type=sliced?UnityEngine.UI.Image.Type.Sliced:UnityEngine.UI.Image.Type.Simple;im.preserveAspect=!sliced;im.color=White;im.raycastTarget=false;if(sliced)im.pixelsPerUnitMultiplier=1.6f;return im;}
static void Skin(Image im,Sprite sprite){if(!im)return;im.sprite=sprite;im.type=UnityEngine.UI.Image.Type.Sliced;im.preserveAspect=false;im.color=White;im.pixelsPerUnitMultiplier=1.6f;foreach(var outline in im.GetComponents<Outline>())UnityEngine.Object.DestroyImmediate(outline);}
static Text Label(string n,Transform p,string value,float x,float y,float w,float h,int size,Color color,bool isCartoon=false){var r=Rect(n,p);var t=r.gameObject.AddComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=value;MoveLabel(r,x,y,w,h,size,color,isCartoon);return t;}
static void MoveLabel(Transform node,float x,float y,float w,float h,int size,Color color,bool isCartoon=false){var t=node.GetComponent<Text>();Place(node.GetComponent<RectTransform>(),x,y,w,h);t.fontSize=size;t.color=color;t.fontStyle=FontStyle.Bold;t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;ConfigureTMP(t,isCartoon);}
static void ConfigureTMP(Text source,bool isCartoon=false){var binding=source.GetComponentInChildren<TMPTextBinding>(true);if(!binding){var go=new GameObject("Label_TMP",typeof(RectTransform),typeof(TextMeshProUGUI),typeof(TMPTextBinding));go.transform.SetParent(source.transform,false);binding=go.GetComponent<TMPTextBinding>();binding.source=source;binding.target=go.GetComponent<TextMeshProUGUI>();}var t=binding.target;var rt=t.rectTransform;rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=new Vector2(2,1);rt.offsetMax=new Vector2(-2,-1);t.font=isCartoon?(magicFont?magicFont:paybackFont):(paybackFont?paybackFont:magicFont);t.fontStyle=FontStyles.Normal;t.text=source.text;t.color=source.color;t.fontSize=source.fontSize;t.fontSizeMax=source.fontSize;t.fontSizeMin=Mathf.Max(10,source.fontSize*.65f);t.enableAutoSizing=true;t.textWrappingMode=TextWrappingModes.Normal;t.overflowMode=TextOverflowModes.Ellipsis;t.alignment=source.alignment==TextAnchor.MiddleLeft?TextAlignmentOptions.MidlineLeft:TextAlignmentOptions.Midline;t.raycastTarget=false;t.margin=new Vector4(2,0,2,0);source.enabled=false;source.raycastTarget=false;}
static void Place(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
}}



