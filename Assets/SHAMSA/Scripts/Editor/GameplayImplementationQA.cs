using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
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
    public static class GameplayImplementationQA
    {
        static readonly List<string> results=new List<string>();
        static readonly List<EnemyUnit> targets=new List<EnemyUnit>();
        static CrystalArts arts;
        static DefenderUnit archer,mage;
        static int phase;
        static float until;
        static void Check(bool condition,string name)
        {
            results.Add((condition?"PASS ":"FAIL ")+name);
            if(!condition)Debug.LogError("GAMEPLAY_ACCEPTANCE|FAIL|"+name);
        }
        public static void Run()
        {
            if(!EditorApplication.isPlaying)throw new Exception("Play Mode required");
            EditorApplication.update-=Tick;results.Clear();targets.Clear();Time.timeScale=1;
            var manager=GameManager.Instance;
            manager.StartLevel(manager.CurrentLevelData);
            WaveDirector.Instance.StopAllWaveActivity();WaveDirector.Instance.enabled=false;
            foreach(var existingEnemy in UnityEngine.Object.FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(existingEnemy.gameObject);
            foreach(var unit in UnityEngine.Object.FindObjectsByType<DefenderUnit>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(unit.gameObject);
            var wallet=SoulEnergyWallet.Instance;wallet.Initialize(2000,25,4);wallet.SetAutoGenerate(false);
            var placement=UnityEngine.Object.FindFirstObjectByType<PlacementController>();
            string[] dataNames={"DwarfGuardian","BloodChef","SoulArcher","BoneMage"};
            for(int i=0;i<4;i++)
            {
                var data=AssetDatabase.LoadAssetAtPath<DefenderData>("Assets/SHAMSA/GameplayData/"+dataNames[i]+"_Data.asset");
                Check(placement.DeployDefender(data,i,0),dataNames[i]+" deploys");
            }
            Check(wallet.CurrentEnergy==1450,"four deployments spend exactly 550 energy");
            var dwarfData=AssetDatabase.LoadAssetAtPath<DefenderData>("Assets/SHAMSA/GameplayData/DwarfGuardian_Data.asset");
            Check(!placement.DeployDefender(dwarfData,0,0)&&wallet.CurrentEnergy==1450,"occupied tile rejects placement without spending");
            foreach(var unit in UnityEngine.Object.FindObjectsByType<DefenderUnit>(FindObjectsSortMode.None))
            {unit.enabled=false;if(unit.Data.defenderType==DefenderType.SoulArcher)archer=unit;if(unit.Data.defenderType==DefenderType.BoneMage)mage=unit;}
            arts=UnityEngine.Object.FindFirstObjectByType<CrystalArts>();arts.enabled=false;
            var ready=(float[])typeof(CrystalArts).GetField("readyAt",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(arts);Array.Clear(ready,0,ready.Length);
            arts.CorruptTiles();Check(arts.CorruptedCount==2,"two tactical cells become corrupted");
            Check(UnityEngine.Object.FindObjectsByType<PlacementCell>(FindObjectsSortMode.None).Where(c=>c.IsCorrupted).All(c=>Mathf.Approximately(c.AttackSpeedMultiplier,.65f)),"corruption applies a 35 percent attack-speed penalty");
            var enemy=Spawn(2,new Vector3(0,0,0));float y=enemy.transform.position.y;
            arts.SoulPulse();Check(Mathf.Approximately(enemy.transform.position.y,y+1.1f)&&arts.CorruptedCount==0,"Soul Pulse knocks back and purifies");
            y=enemy.transform.position.y;arts.SoulPulse();Check(Mathf.Approximately(enemy.transform.position.y,y)&&arts.CooldownRemaining(0)>17,"Soul Pulse cannot bypass its cooldown");
            float hp=enemy.CurrentHealth;arts.CrystalStorm();Check(Mathf.Approximately(enemy.CurrentHealth,hp-36),"Crystal Storm deals 36 actual damage");
            hp=enemy.CurrentHealth;arts.CrystalStorm();Check(Mathf.Approximately(enemy.CurrentHealth,hp),"Crystal Storm cannot bypass its cooldown");
            GameManager.Instance.TogglePause();arts.DivineBarrier();Check(!SoulCrystal.Instance.IsBarrierActive,"paused battle rejects ability casts");
            Check(!placement.DeployDefender(dwarfData,4,0),"paused battle rejects deployment");GameManager.Instance.TogglePause();
            arts.DivineBarrier();int crystalHp=SoulCrystal.Instance.CurrentHealth;SoulCrystal.Instance.TakeBreachDamage(3);
            Check(SoulCrystal.Instance.IsBarrierActive&&SoulCrystal.Instance.CurrentHealth==crystalHp,"Divine Barrier blocks real breach damage");
            ClearTargets();
            for(int i=0;i<3;i++)Spawn(2,new Vector3(0,-.2f+i*.5f,0));
            archer.enabled=true;phase=1;until=Time.time+2.8f;EditorApplication.update+=Tick;
        }
        static EnemyUnit Spawn(int lane,Vector3 pos)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SHAMSA/Prefabs/BoneRaider.prefab");
            var go=UnityEngine.Object.Instantiate(prefab,pos,Quaternion.identity);var enemy=go.GetComponent<EnemyUnit>();
            enemy.Initialize(AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/SHAMSA/GameplayData/BoneRaider_Data.asset"),lane);
            enemy.enabled=false;targets.Add(enemy);return enemy;
        }
        static void ClearTargets(){foreach(var target in targets)if(target)UnityEngine.Object.DestroyImmediate(target.gameObject);targets.Clear();}
        static void Tick()
        {
            if(!EditorApplication.isPlaying){EditorApplication.update-=Tick;return;}
            if(Time.time<until)return;
            if(phase==1)
            {
                Check(targets.Count==3&&targets.All(e=>e!=null&&e.CurrentHealth<e.MaxHealth),"Soul Archer pierces three distinct enemies");
                archer.enabled=false;ClearTargets();
                foreach(var projectile in UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(projectile.gameObject);
                Spawn(3,new Vector3(1.35f,-.2f,0));Spawn(2,new Vector3(.4f,-.2f,0));Spawn(4,new Vector3(2.3f,-.2f,0));
                mage.enabled=true;phase=2;until=Time.time+2.2f;
            }
            else if(phase==2)
            {
                Check(targets.All(e=>e!=null&&e.CurrentHealth<e.MaxHealth),"Bone Mage chains damage to neighboring lanes");
                mage.enabled=false;phase=3;until=Time.time+1.3f;
            }
            else
            {
                int hp=SoulCrystal.Instance.CurrentHealth;SoulCrystal.Instance.TakeBreachDamage(1);
                Check(!SoulCrystal.Instance.IsBarrierActive&&SoulCrystal.Instance.CurrentHealth==hp-1,"barrier expires and normal breach damage resumes");
                var canvas=GameObject.Find("HUDCanvas").GetComponent<Canvas>();
                var raycaster=canvas.GetComponent<GraphicRaycaster>();
                foreach(var button in canvas.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Art_")||b.GetComponent<DefenderCardUI>()!=null))
                {
                    var r=button.GetComponent<RectTransform>();var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center))};
                    var hits=new List<RaycastResult>();raycaster.Raycast(pointer,hits);
                    Check(hits.Count>0&&hits[0].gameObject==button.gameObject,button.name+" receives pointer raycasts");
                }
                File.WriteAllLines("ArtReview/GameplayImplementation/Acceptance.txt",results);
                EditorApplication.update-=Tick;Debug.Log("GAMEPLAY_ACCEPTANCE|COMPLETE|"+results.Count+" checks; "+results.Count(s=>s.StartsWith("FAIL"))+" failures");
            }
        }
    }
}

