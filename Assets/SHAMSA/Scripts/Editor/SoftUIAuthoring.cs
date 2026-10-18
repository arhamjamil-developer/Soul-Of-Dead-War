using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using SoulCrystalDefense.UI;
namespace SoulCrystalDefense.EditorTools {
public static class SoftUIAuthoring {
public static void ConvertText(){
var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
foreach(var text in UnityEngine.Object.FindObjectsByType<Text>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
 if(!text.GetComponentInParent<Canvas>())continue;
 var binding=text.GetComponentInChildren<TMPTextBinding>(true);
 if(!binding){var go=new GameObject("Label_TMP",typeof(RectTransform),typeof(TextMeshProUGUI),typeof(TMPTextBinding));go.transform.SetParent(text.transform,false);binding=go.GetComponent<TMPTextBinding>();binding.source=text;binding.target=go.GetComponent<TextMeshProUGUI>();}
 var t=binding.target;var r=t.rectTransform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=new Vector2(5,2);r.offsetMax=new Vector2(-5,-2);t.font=font;t.text=text.text;t.color=text.color;t.fontStyle=text.fontStyle==FontStyle.Bold?FontStyles.Bold:FontStyles.Normal;t.fontSize=text.fontSize;t.enableAutoSizing=true;t.fontSizeMax=text.fontSize;t.fontSizeMin=Mathf.Max(10,text.fontSize*.5f);t.overflowMode=TextOverflowModes.Ellipsis;t.textWrappingMode=TextWrappingModes.Normal;t.raycastTarget=false;t.margin=new Vector4(3,1,3,1);
 t.alignment=text.alignment switch {TextAnchor.UpperLeft=>TextAlignmentOptions.TopLeft,TextAnchor.UpperCenter=>TextAlignmentOptions.Top,TextAnchor.UpperRight=>TextAlignmentOptions.TopRight,TextAnchor.MiddleLeft=>TextAlignmentOptions.MidlineLeft,TextAnchor.MiddleRight=>TextAlignmentOptions.MidlineRight,TextAnchor.LowerLeft=>TextAlignmentOptions.BottomLeft,TextAnchor.LowerCenter=>TextAlignmentOptions.Bottom,TextAnchor.LowerRight=>TextAlignmentOptions.BottomRight,_=>TextAlignmentOptions.Midline};text.enabled=false;text.raycastTarget=false;EditorUtility.SetDirty(binding);
}
Save();
}
public static void Gameplay(){
var round=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
foreach(var im in UnityEngine.Object.FindObjectsByType<Image>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
 if(im.sprite==null)continue;string path=AssetDatabase.GetAssetPath(im.sprite);if(!path.Contains("Premium/Frames"))continue;
 if(im.name.Contains("Selection")||im.name=="Selected"){im.sprite=round;im.type=Image.Type.Sliced;im.preserveAspect=false;im.pixelsPerUnitMultiplier=1;im.color=new Color(.3f,.88f,1,.22f);continue;}
 im.sprite=round;im.type=Image.Type.Sliced;im.preserveAspect=false;im.pixelsPerUnitMultiplier=1;im.color=new Color(.07f,.15f,.23f,.97f);
 var old=im.GetComponent<Outline>();if(old)UnityEngine.Object.DestroyImmediate(old);
 var outline=im.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.35f,.65f,.75f,.22f);outline.effectDistance=new Vector2(1,-1);outline.useGraphicAlpha=true;
}
var fill=GameObject.Find("HealthFill").GetComponent<Image>();fill.sprite=round;fill.type=Image.Type.Filled;fill.fillMethod=Image.FillMethod.Horizontal;fill.fillOrigin=0;fill.color=new Color(.18f,.86f,1,1); var divider=GameObject.Find("StageDivider").GetComponent<RectTransform>();divider.sizeDelta=new Vector2(350,5); var top=GameObject.Find("TopHUD").transform;
SetText(top,"CrystalName",92,25,175,33,22);SetText(top,"CrystalHealthText",90,72,175,45,34);SetText(top,"World",325,24,398,30,19);SetText(top,"Stage",325,68,398,60,27);SetText(top,"WaveText",790,28,180,42,28);SetText(top,"EnergyText",840,105,120,40,32);
foreach(var im in top.GetComponentsInChildren<Image>())if(im.name.Contains("Track")||im.name.Contains("Divider"))im.color=new Color(.3f,.75f,.85f,.4f);
foreach(var name in new[]{"RoyalBulwarkButton","SoulExpressButton"}){var button=GameObject.Find(name);SetText(button.transform,"Title",110,8,355,32,25);SetText(button.transform,"State",110,43,355,28,21);}
ConvertText();
}
static void SetText(Transform root,string name,float x,float y,float w,float h,int size){var t=root.Find(name);if(!t)return;var r=t.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);t.GetComponent<Text>().fontSize=size;}
static void Save(){EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());}
public static object Audit(){Canvas.ForceUpdateCanvases();var texts=UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Exclude,FindObjectsSortMode.None);foreach(var t in texts)t.ForceMeshUpdate();return new {resolution=Screen.width+"x"+Screen.height,visibleTMP=texts.Length,overflow=texts.Where(t=>t.isTextOverflowing).Select(t=>t.transform.parent.name).ToArray(),legacyVisible=UnityEngine.Object.FindObjectsByType<Text>(FindObjectsSortMode.None).Count(t=>t.enabled)};}
}}

