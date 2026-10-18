using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using SoulCrystalDefense.Data;
using SoulCrystalDefense.Units;
using SoulCrystalDefense.Combat;
using SoulCrystalDefense.Systems;
namespace SoulCrystalDefense.EditorTools
{
    public static class ReinforcementAuthoring
    {
        public static void Apply()
        {
            string dataPath="Assets/SHAMSA/GameplayData/RoyalBulwark_Data.asset";
            var data=AssetDatabase.LoadAssetAtPath<DefenderData>(dataPath);if(!data){data=ScriptableObject.CreateInstance<DefenderData>();AssetDatabase.CreateAsset(data,dataPath);}
            data.defenderType=DefenderType.ElephantPreview;data.displayName="Royal Bulwark";data.soulCost=200;data.maxHealth=500;data.damage=25;data.attackCooldown=1.6f;data.attackRange=1.1f;data.knockback=.3f;data.cardIcon=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Premium/Supports/ElephantPortrait.png");
            var go=new GameObject("RoyalBulwark");var visual=new GameObject("Visual",typeof(SpriteRenderer));visual.transform.SetParent(go.transform,false);var sr=visual.GetComponent<SpriteRenderer>();sr.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Premium/Supports/ElephantWorld.png");sr.sortingOrder=4;visual.transform.localScale=Vector3.one*(1.55f/sr.sprite.bounds.size.y);
            var hp=new GameObject("HealthBar",typeof(HealthBar));hp.transform.SetParent(go.transform,false);hp.transform.localPosition=Vector3.up*.85f;
            var unit=go.AddComponent<DefenderUnit>();var so=new SerializedObject(unit);so.FindProperty("defenderData").objectReferenceValue=data;so.FindProperty("spriteRenderer").objectReferenceValue=sr;so.FindProperty("healthBar").objectReferenceValue=hp.GetComponent<HealthBar>();so.ApplyModifiedPropertiesWithoutUndo();
            data.unitPrefab=PrefabUtility.SaveAsPrefabAsset(go,"Assets/SHAMSA/Prefabs/RoyalBulwark.prefab");Object.DestroyImmediate(go);EditorUtility.SetDirty(data);
            var train=new GameObject("SoulExpress",typeof(SpriteRenderer),typeof(SoulTrainSweep));var ts=train.GetComponent<SpriteRenderer>();ts.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Premium/Supports/TrainWorld.png");ts.sortingOrder=9;train.transform.localScale=Vector3.one*(1.7f/ts.sprite.bounds.size.y);
            var trainPrefab=PrefabUtility.SaveAsPrefabAsset(train,"Assets/SHAMSA/Prefabs/SoulExpress.prefab");Object.DestroyImmediate(train);
            var root=GameObject.Find("GameplayComposition");var support=root.GetComponent<BattleReinforcements>()??root.AddComponent<BattleReinforcements>();support.elephantData=data;support.trainPrefab=trainPrefab;
            var tray=GameObject.Find("BottomTray").transform;
            foreach(string n in new[]{"RoyalBulwarkButton","SoulExpressButton"}){var old=tray.Find(n);if(old)Object.DestroyImmediate(old.gameObject);}
            support.elephantButton=Control("RoyalBulwarkButton",tray,35,"ROYAL ELEPHANT","Elephant",out support.elephantState);UnityEventTools.AddPersistentListener(support.elephantButton.onClick,support.SelectElephant);
            support.trainButton=Control("SoulExpressButton",tray,560,"SOUL TRAIN","Train",out support.trainState);UnityEventTools.AddPersistentListener(support.trainButton.onClick,support.SelectTrain);
            var bossData=AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/SHAMSA/GameplayData/ArmoredElephant_Data.asset");bossData.displayName="Corrupted Colossus";EditorUtility.SetDirty(bossData);
            var scene=EditorSceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
        static Button Control(string name,Transform parent,float x,string title,string icon,out Text state)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(parent,false);Place(go.GetComponent<RectTransform>(),x,110,485,85);var im=go.GetComponent<Image>();im.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Premium/Frames/Button.png");im.type=Image.Type.Sliced;
            var ic=new GameObject("Icon",typeof(RectTransform),typeof(Image));ic.transform.SetParent(go.transform,false);Place(ic.GetComponent<RectTransform>(),15,5,85,75);ic.GetComponent<Image>().sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Premium/Icons/"+icon+".png");ic.GetComponent<Image>().preserveAspect=true;ic.GetComponent<Image>().raycastTarget=false;
            Label(go.transform,"Title",title,105,7,365,33,26);state=Label(go.transform,"State","ONCE PER BATTLE",105,45,365,26,20);state.color=new Color(.3f,.9f,1);
            var button=go.GetComponent<Button>();button.targetGraphic=im;return button;
        }
        static Text Label(Transform parent,string name,string text,float x,float y,float w,float h,int size){var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);Place(go.GetComponent<RectTransform>(),x,y,w,h);var t=go.GetComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=text;t.fontSize=size;t.alignment=TextAnchor.MiddleCenter;t.color=new Color(1,.93f,.76f);t.raycastTarget=false;return t;}
        static void Place(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
    }
}
