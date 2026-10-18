using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using SoulCrystalDefense.Data;
using SoulCrystalDefense.Units;
using SoulCrystalDefense.Systems;
using SoulCrystalDefense.UI;
using SoulCrystalDefense.Grid;
namespace SoulCrystalDefense.EditorTools {
public static class PremiumVisualQA {
static List<string> checks=new List<string>(); static double until; static GameObject dead;
static void Check(bool value,string name){checks.Add((value?"PASS ":"FAIL ")+name);}
public static void Repair(){
PremiumVisualAuthoring.ImportKit();
foreach(var im in UnityEngine.Object.FindObjectsByType<Image>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(im.type==Image.Type.Sliced){im.preserveAspect=false;im.pixelsPerUnitMultiplier=2;}
var top=GameObject.Find("TopHUD").transform;foreach(var n in new[]{"CrystalHealthText","WaveText","EnergyText","PauseButton"})top.Find(n).SetAsLastSibling();
EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();
}
public static void Run(){
checks.Clear();Time.timeScale=1;
GameManager.Instance.StartLevel(GameManager.Instance.CurrentLevelData);WaveDirector.Instance.StopAllWaveActivity();WaveDirector.Instance.enabled=false;
foreach(var e in UnityEngine.Object.FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(e.gameObject);
foreach(var d in UnityEngine.Object.FindObjectsByType<DefenderUnit>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(d.gameObject);
SoulEnergyWallet.Instance.Initialize(5000,25,4);SoulEnergyWallet.Instance.SetAutoGenerate(false);
foreach(var name in new[]{"SoulArcher","BoneMage","IronPaladin","FrostWitch","SkeletonRunner","ShieldBearer"}){
var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SHAMSA/Prefabs/"+name+".prefab");var a=prefab.GetComponent<UnitAnimationPlayer>();Check(a&&a.HasIdleFrames&&a.HasAttackFrames&&a.HasDeathFrames&&a.TargetRenderer.sprite!=null,name+" has bound idle/walk, attack and death frames");}
var rein=UnityEngine.Object.FindFirstObjectByType<BattleReinforcements>();int energy=SoulEnergyWallet.Instance.CurrentEnergy;
Check(rein.SummonElephant(0,2)&&SoulEnergyWallet.Instance.CurrentEnergy==energy-200,"Royal Bulwark spawns and spends 200");Check(!rein.SummonElephant(1,2),"Royal Bulwark is limited to once per battle");
var elephant=UnityEngine.Object.FindObjectsByType<DefenderUnit>(FindObjectsSortMode.None).First(x=>x.Data.defenderType==DefenderType.ElephantPreview);Check(!elephant.CanUpgrade&&elephant.GetComponent<TemporaryGuardian>(),"Elephant has timed lifetime and rejects upgrades");
energy=SoulEnergyWallet.Instance.CurrentEnergy;Check(rein.RunTrain(4)&&SoulEnergyWallet.Instance.CurrentEnergy==energy-150,"Soul Express spawns and spends 150");Check(!rein.RunTrain(3),"Soul Express is limited to once per battle");
var shield=Spawn("ShieldBearer",2,0);shield.enabled=false;float hp=shield.CurrentHealth;shield.TakeDamage(100);Check(Mathf.Approximately(hp-shield.CurrentHealth,65),"Shield Bearer reduces actual damage by 35 percent");UnityEngine.Object.DestroyImmediate(shield.gameObject);
var runner=Spawn("SkeletonRunner",1,0);dead=runner.gameObject;runner.TakeDamage(9999);
var flow=UnityEngine.Object.FindFirstObjectByType<GameScreenFlow>();flow.TogglePause();Check(GameManager.Instance.IsPaused&&flow.pausePanel.activeSelf,"Pause opens panel and suspends battle");flow.Settings();Check(flow.settingsPanel.activeSelf&&!flow.pausePanel.activeSelf,"Settings opens from pause");flow.Back();flow.Tutorial();Check(flow.tutorialPanel.activeSelf,"Tutorial opens from pause");flow.Resume();Check(!GameManager.Instance.IsPaused&&!flow.tutorialPanel.activeSelf,"Resume closes panels and restores battle");
var scroll=GameObject.Find("GuardianScroll").GetComponent<ScrollRect>();scroll.horizontalNormalizedPosition=1;Canvas.ForceUpdateCanvases();var last=GameObject.Find("Card_FrostWitch").GetComponent<RectTransform>();Check(RectTransformUtility.RectangleContainsScreenPoint(scroll.viewport,RectTransformUtility.WorldToScreenPoint(null,last.TransformPoint(last.rect.center))),"Horizontal tray scroll reveals sixth guardian");scroll.horizontalNormalizedPosition=0;
until=EditorApplication.timeSinceStartup+1.2;EditorApplication.update+=Finish;
}
static void Finish(){if(EditorApplication.timeSinceStartup<until)return;EditorApplication.update-=Finish;Check(dead==null,"Death animation completes and removes defeated enemy");File.WriteAllLines("ArtReview/PremiumVisuals/Integration-Acceptance.txt",checks);Debug.Log("PREMIUM_QA|"+checks.Count+" checks / "+checks.Count(x=>x.StartsWith("FAIL"))+" failures");}
static EnemyUnit Spawn(string name,int lane,float y){var go=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SHAMSA/Prefabs/"+name+".prefab"),new Vector3(BoardGrid.Instance.GetLaneX(lane),y,0),Quaternion.identity);var unit=go.GetComponent<EnemyUnit>();unit.Initialize(AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/SHAMSA/GameplayData/"+name+"_Data.asset"),lane);return unit;}
public static void Stage(){
Time.timeScale=1;GameManager.Instance.StartLevel(GameManager.Instance.CurrentLevelData);WaveDirector.Instance.StopAllWaveActivity();WaveDirector.Instance.enabled=false;
foreach(var e in UnityEngine.Object.FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(e.gameObject);
foreach(var d in UnityEngine.Object.FindObjectsByType<DefenderUnit>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(d.gameObject);
SoulEnergyWallet.Instance.Initialize(2400,25,4);SoulEnergyWallet.Instance.SetAutoGenerate(false);
string[] names={"DwarfGuardian","BloodChef","SoulArcher","BoneMage","IronPaladin","FrostWitch"};
for(int i=0;i<6;i++){var data=AssetDatabase.LoadAssetAtPath<DefenderData>("Assets/SHAMSA/GameplayData/"+names[i]+"_Data.asset");PlacementController.Instance.DeployDefender(data,i%5,i<5?2:3);}
for(int i=0;i<5;i++){var e=Spawn(i%2==0?"ShieldBearer":"SkeletonRunner",i,2.5f+(i%2)*.7f);e.enabled=false;}
Time.timeScale=.15f;
}
}}

