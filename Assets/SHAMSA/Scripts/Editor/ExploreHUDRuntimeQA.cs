using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using SoulCrystalDefense.UI;
using SoulCrystalDefense.Systems;
using SoulCrystalDefense.Grid;

namespace SoulCrystalDefense.EditorTools
{
    public static class ExploreHUDRuntimeQA
    {
        [Serializable] public class Check{public string name;public bool passed;}
        [Serializable] public class Report{public List<Check> checks=new List<Check>();public string status="running";}
        static Report report;
        static CollapsibleBattleHUD hud;
        static double until;
        static int phase;
        static readonly List<InputDevice> suspendedDevices=new List<InputDevice>();
        static void RestoreInput(){foreach(var d in suspendedDevices)if(d.added)InputSystem.EnableDevice(d);suspendedDevices.Clear();}
        static void CheckResult(string name,bool passed){report.checks.Add(new Check{name=name,passed=passed});Save();}
        static void Save(){Directory.CreateDirectory("ArtReview/CollapsibleHUD");File.WriteAllText("ArtReview/CollapsibleHUD/Runtime-QA.json",JsonUtility.ToJson(report,true));}
        static void Capture(string name)=>ScreenCapture.CaptureScreenshot(Path.GetFullPath("ArtReview/CollapsibleHUD/"+name+".png"));
        public static void Start()
        {
            if(!EditorApplication.isPlaying)throw new Exception("Enter Play Mode first");
            EditorApplication.update-=Tick;report=new Report();hud=UnityEngine.Object.FindFirstObjectByType<CollapsibleBattleHUD>();
            RestoreInput();foreach(var d in InputSystem.devices)if(d.enabled){suspendedDevices.Add(d);InputSystem.DisableDevice(d);}
            var tutorial=FirstWaveTutorial.Instance;
            bool had=PlayerPrefs.HasKey("SoulCrystal.FirstWaveTutorial.v1");int seen=PlayerPrefs.GetInt("SoulCrystal.FirstWaveTutorial.v1",0);
            if(tutorial&&tutorial.Running)tutorial.Skip();
            if(had)PlayerPrefs.SetInt("SoulCrystal.FirstWaveTutorial.v1",seen);else PlayerPrefs.DeleteKey("SoulCrystal.FirstWaveTutorial.v1");PlayerPrefs.Save();
            CheckResult("All authored HUD references assigned",hud&&hud.topGroup&&hud.bottomGroup&&hud.floatingPauseGroup&&hud.returnBattleGroup);
            CheckResult("Measured panels: top >=232, bottom 580",hud.TopTravel>=256&&hud.BottomTravel>=604);
            hud.screenFlow.Resume();hud.SetTopHidden(false);hud.SetBottomHidden(false);phase=-1;until=EditorApplication.timeSinceStartup+1;EditorApplication.update+=Tick;
        }
        static void Next(int next,float seconds){phase=next;until=EditorApplication.timeSinceStartup+seconds;}
        static void Tick()
        {
            if(!EditorApplication.isPlaying||!hud){if(report!=null){report.status="interrupted";Save();}RestoreInput();EditorApplication.update-=Tick;return;}
            if(EditorApplication.timeSinceStartup<until)return;
            try
            {
                var pan=CinemachineCameraPanController.Instance;
                switch(phase)
                {
                    case -1:
                        hud.topButton.onClick.Invoke();hud.bottomButton.onClick.Invoke();Next(0,1);break;
                    case 0:
                        CheckResult("Both panels hide after animated button clicks",hud.TopHidden&&hud.BottomHidden&&hud.topGroup.alpha<.01f&&hud.bottomGroup.alpha<.01f);
                        CheckResult("Hidden panels release raycasts",!hud.topGroup.blocksRaycasts&&!hud.bottomGroup.blocksRaycasts);
                        CheckResult("Floating pause and return remain available",hud.floatingPauseGroup.interactable&&hud.returnBattleGroup.interactable);
                        var top=RectTransformUtility.CalculateRelativeRectTransformBounds(hud.layoutRoot,hud.topPanel);
                        var bottom=RectTransformUtility.CalculateRelativeRectTransformBounds(hud.layoutRoot,hud.bottomPanel);
                        CheckResult("Complete panel bounds moved outside safe canvas",top.min.y>=hud.layoutRoot.rect.yMax&&bottom.max.y<=hud.layoutRoot.rect.yMin);
                        CheckResult("Up/down artwork swaps correctly",hud.topArtwork.sprite==hud.down&&hud.bottomArtwork.sprite==hud.up);
                        Capture("01-HUD-Hidden");Next(10,.4f);break;
                    case 10:
                        hud.Pause();hud.SetTopHidden(false);hud.SetBottomHidden(false);Next(1,1);break;
                    case 1:
                        CheckResult("Unscaled slide animation works during pause",GameManager.Instance.IsPaused&&hud.topGroup.alpha>.99f&&hud.bottomGroup.alpha>.99f);
                        CheckResult("Collapse handles do not overlap pause menu",!hud.topHandle.gameObject.activeSelf&&!hud.bottomHandle.gameObject.activeSelf);
                        hud.screenFlow.Resume();hud.SetTopHidden(true);hud.SetBottomHidden(true);Next(2,1);break;
                    case 2:
                        pan.VisitNeighbor(-1);Next(3,2.5f);break;
                    case 3:
                        CheckResult("Left neighbor camera target and rendering",pan.TargetPosition.x< -5&&Camera.main.transform.position.x< -2);
                        Capture("02-Guardian-Neighbor");Next(11,.4f);break;
                    case 11:
                        pan.VisitNeighbor(1);Next(4,3);break;
                    case 4:
                        CheckResult("Right neighbor camera target and rendering",pan.TargetPosition.x>5&&Camera.main.transform.position.x>2);
                        Capture("03-Chef-Neighbor");Next(12,.4f);break;
                    case 12:
                        var grid=BoardGrid.Instance;
                        CheckResult("Neighbor art does not create deployment cells",grid.GetCellAtWorldPosition(new Vector3(-9.5f,0))==null&&grid.GetCellAtWorldPosition(new Vector3(9.5f,0))==null&&grid.GetComponentsInChildren<PlacementCell>().Length==40);
                        var card=UnityEngine.Object.FindObjectsByType<DefenderCardUI>(FindObjectsSortMode.None).First(c=>c.Data!=null);
                        PlacementController.Instance.SelectCard(card);
                        CheckResult("Selecting a card returns to battle",Mathf.Abs(pan.TargetPosition.x)<.01f);
                        PlacementController.Instance.CancelPlacement();
                        for(int i=0;i<10;i++){hud.ToggleTop();hud.ToggleBottom();}
                        hud.SetTopHidden(false);hud.SetBottomHidden(false);Next(5,1);break;
                    case 5:
                        CheckResult("Rapid toggles settle cleanly",!hud.TopHidden&&!hud.BottomHidden&&hud.topGroup.alpha>.99f&&hud.bottomGroup.alpha>.99f);
                        CheckResult("Restored panels accept input",hud.topGroup.interactable&&hud.bottomGroup.interactable&&hud.topGroup.blocksRaycasts&&hud.bottomGroup.blocksRaycasts);
                        CheckResult("Return is centered",Mathf.Abs(pan.TargetPosition.x)<.01f);
                        Capture("04-HUD-Restored");report.status=report.checks.All(c=>c.passed)?"passed":"failed";Save();RestoreInput();EditorApplication.update-=Tick;Debug.Log("EXPLORE_HUD_QA|"+report.status+"|"+report.checks.Count+" checks");break;
                }
            }
            catch(Exception ex){CheckResult(ex.Message,false);report.status="failed";Save();RestoreInput();EditorApplication.update-=Tick;Debug.LogException(ex);}
        }
    }
}
