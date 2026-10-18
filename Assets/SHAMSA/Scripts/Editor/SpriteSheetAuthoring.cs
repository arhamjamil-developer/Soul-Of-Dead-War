using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using SoulCrystalDefense.Data;
using SoulCrystalDefense.Units;
using SoulCrystalDefense.Presentation;
using SoulCrystalDefense.Systems;
using UnityEngine.UI;
namespace SoulCrystalDefense.EditorTools
{
    public static class SpriteSheetAuthoring
    {
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new Exception("Stop Play Mode first");
            foreach(string name in new[]{"SoulArcher","BoneMage","IronPaladin","FrostWitch","SkeletonRunner","ShieldBearer"})
            {
                string dir="Assets/UI/AnimationSheets/"+name;
                if(!System.IO.Directory.Exists(dir))continue;
                foreach(var file in System.IO.Directory.GetFiles(dir,"*.png").Where(p=>!p.Contains("Source")))Import(file.Replace('\\','/'));
                bool enemy=name=="SkeletonRunner"||name=="ShieldBearer";
                if(enemy)CreateEnemy(name);
                var root=PrefabUtility.LoadPrefabContents("Assets/SHAMSA/Prefabs/"+name+".prefab");
                try
                {
                    var sourceUnit=(UnityEngine.Object)root.GetComponent<DefenderUnit>()??root.GetComponent<EnemyUnit>(); var sourceSO=new SerializedObject(sourceUnit); var renderer=sourceSO.FindProperty("spriteRenderer").objectReferenceValue as SpriteRenderer; if(!renderer)renderer=root.GetComponentInChildren<SpriteRenderer>();root.transform.localScale=Vector3.one;
                    renderer.transform.localScale=Vector3.one; foreach(var extra in root.GetComponentsInChildren<SpriteRenderer>())if(extra!=renderer&&extra.name!="Shadow")extra.enabled=false;
                    var animator=root.GetComponent<UnitAnimationPlayer>()??root.AddComponent<UnitAnimationPlayer>();
                    var idle=Frames(dir,enemy?"Walk":"Idle");var attack=Frames(dir,"Attack");var death=Frames(dir,"Death");
                    var so=new SerializedObject(animator);so.FindProperty("targetRenderer").objectReferenceValue=renderer;so.ApplyModifiedPropertiesWithoutUndo();animator.SetFrames(idle,idle,attack,death);animator.SetFps(9,12,10,6);renderer.sprite=idle[0];
                    var unit=(UnityEngine.Object)root.GetComponent<DefenderUnit>()??root.GetComponent<EnemyUnit>();
                    so=new SerializedObject(unit);so.FindProperty("animPlayer").objectReferenceValue=animator;so.FindProperty("spriteRenderer").objectReferenceValue=renderer;so.ApplyModifiedPropertiesWithoutUndo();
                    var motion=root.GetComponent<GuardianMotion>();if(motion)UnityEngine.Object.DestroyImmediate(motion);
                    var hp=root.GetComponentInChildren<SoulCrystalDefense.Combat.HealthBar>();if(hp){hp.transform.localScale=Vector3.one;hp.transform.localPosition=new Vector3(0,.66f,0);}
                    PrefabUtility.SaveAsPrefabAsset(root,"Assets/SHAMSA/Prefabs/"+name+".prefab");
                }
                finally{PrefabUtility.UnloadPrefabContents(root);}
            }
            var director=UnityEngine.Object.FindFirstObjectByType<WaveDirector>();
            director.skeletonRunnerPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SHAMSA/Prefabs/SkeletonRunner.prefab");director.skeletonRunnerData=AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/SHAMSA/GameplayData/SkeletonRunner_Data.asset");
            director.shieldBearerPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SHAMSA/Prefabs/ShieldBearer.prefab");director.shieldBearerData=AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/SHAMSA/GameplayData/ShieldBearer_Data.asset");
            var atmosphere=director.GetComponent<BattleAtmosphere>()??director.gameObject.AddComponent<BattleAtmosphere>();
            atmosphere.backdrop=GameObject.Find("GraveyardBackdrop").GetComponent<SpriteRenderer>();atmosphere.realms=new Sprite[4];atmosphere.realms[0]=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Gameplay/Battlefield.png");
            string[] paths={"03-FrozenTombs-CleanBoard","02-EmberWasteland-CleanBoard","04-RoyalSoulCitadel-CleanBoard"};
            for(int i=0;i<3;i++){string p="Assets/SHAMSA/Concept/Portrait/"+paths[i]+".png";Import(p);atmosphere.realms[i+1]=AssetDatabase.LoadAssetAtPath<Sprite>(p);}
            atmosphere.stageLabel=GameObject.Find("Stage").GetComponent<Text>();atmosphere.worldLabel=GameObject.Find("World").GetComponent<Text>();
            var fx=UnityEngine.Object.FindFirstObjectByType<GameplayEffects>();
            fx.impactFrames=Effects("Impact");fx.summonFrames=Effects("Summon");fx.arcaneFrames=Effects("Arcane");fx.iceFrames=Effects("Ice");
            var scene=EditorSceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("PREMIUM_ANIMATION|Saved ally and enemy frames, new enemy types, and four atmosphere bands.");
        }
        static Sprite[] Frames(string dir,string action)=>Enumerable.Range(0,6).Select(i=>AssetDatabase.LoadAssetAtPath<Sprite>(dir+"/"+action+"_"+i.ToString("00")+".png")).ToArray();
        static Sprite[] Effects(string name)=>Enumerable.Range(0,6).Select(i=>AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Premium/VFX/"+name+"_"+i.ToString("00")+".png")).ToArray();
        static void Import(string path){AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var i=(TextureImporter)AssetImporter.GetAtPath(path);i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;i.spritePixelsPerUnit=220;i.alphaIsTransparency=true;i.mipmapEnabled=false;i.textureCompression=TextureImporterCompression.Uncompressed;i.SaveAndReimport();}
        static void CreateEnemy(string name)
        {
            string path="Assets/SHAMSA/GameplayData/"+name+"_Data.asset";var data=AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if(!data){data=ScriptableObject.CreateInstance<EnemyData>();AssetDatabase.CreateAsset(data,path);}
            bool runner=name=="SkeletonRunner";data.enemyType=runner?EnemyType.SkeletonRunner:EnemyType.ShieldBearer;data.displayName=runner?"Skeleton Runner":"Shield Bearer";data.maxHealth=runner?42:150;data.moveSpeed=runner ? .85f : .32f;data.damage=runner?7:12;data.attackCooldown=runner ? .9f : 1.8f;data.attackRange=.8f;data.crystalBreachDamage=runner?1:3;data.soulReward=runner?12:30;
            var prefab=PrefabUtility.LoadPrefabContents("Assets/SHAMSA/Prefabs/BoneRaider.prefab");
            try{prefab.name=name;var so=new SerializedObject(prefab.GetComponent<EnemyUnit>());so.FindProperty("enemyData").objectReferenceValue=data;so.ApplyModifiedPropertiesWithoutUndo();data.unitPrefab=PrefabUtility.SaveAsPrefabAsset(prefab,"Assets/SHAMSA/Prefabs/"+name+".prefab");EditorUtility.SetDirty(data);}
            finally{PrefabUtility.UnloadPrefabContents(prefab);}
        }
    }
}


