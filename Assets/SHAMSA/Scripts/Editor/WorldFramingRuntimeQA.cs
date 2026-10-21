using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using SoulCrystalDefense.Presentation;
using SoulCrystalDefense.Systems;
using SoulCrystalDefense.UI;

namespace SoulCrystalDefense.EditorTools
{
    public static class WorldFramingRuntimeQA
    {
        [Serializable] public class Result{public string name;public bool passed;public int width,height;public float cameraSize;}
        [Serializable] public class Report{public string status="running";public List<Result> checks=new List<Result>();}
        static readonly List<InputDevice> devices=new List<InputDevice>();
        static Report report;
        static BattlefieldCameraFraming fit;
        static CollapsibleBattleHUD hud;
        static CinemachineCameraPanController pan;
        static int phase;static double until;
        static void Save(){Directory.CreateDirectory("ArtReview/WorldExtensions");File.WriteAllText("ArtReview/WorldExtensions/Runtime-QA.json",JsonUtility.ToJson(report,true));}
        static void Restore(){foreach(var d in devices)if(d.added)InputSystem.EnableDevice(d);devices.Clear();EditorApplication.update-=Tick;}
        static void Check(string name,bool pass){report.checks.Add(new Result{name=name,passed=pass,width=Screen.width,height=Screen.height,cameraSize=Camera.main.orthographicSize});Save();}
        static void Next(int step,float delay){phase=step;until=EditorApplication.timeSinceStartup+delay;}
        static void Size(int index){var view=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));view.GetType().GetProperty("selectedSizeIndex").SetValue(view,index);view.Focus();}
        static void Capture(string name)=>ScreenCapture.CaptureScreenshot(Path.GetFullPath("ArtReview/WorldExtensions/"+name+".png"));
        static void FitCheck(string name)
        {
            var c=Camera.main;var min=c.WorldToViewportPoint(fit.BattleBounds.min);var max=c.WorldToViewportPoint(fit.BattleBounds.max);var v=fit.AvailableViewport;
            Check(name,min.x>=v.xMin-.003f&&max.x<=v.xMax+.003f&&min.y>=v.yMin-.003f&&max.y<=v.yMax+.003f);Capture(name);
        }
        public static void Start()
        {
            if(!EditorApplication.isPlaying)throw new Exception("Enter Play Mode first");
            Restore();report=new Report();fit=UnityEngine.Object.FindFirstObjectByType<BattlefieldCameraFraming>();hud=fit.hud;pan=fit.pan;
            foreach(var d in InputSystem.devices)if(d.enabled){devices.Add(d);InputSystem.DisableDevice(d);}
            bool had=PlayerPrefs.HasKey("SoulCrystal.FirstWaveTutorial.v1");int seen=PlayerPrefs.GetInt("SoulCrystal.FirstWaveTutorial.v1");
            var t=FirstWaveTutorial.Instance;if(t&&t.Running)t.Skip();
            if(had)PlayerPrefs.SetInt("SoulCrystal.FirstWaveTutorial.v1",seen);else PlayerPrefs.DeleteKey("SoulCrystal.FirstWaveTutorial.v1");PlayerPrefs.Save();
            PlacementController.Instance.CancelPlacement();hud.screenFlow.Resume();hud.SetTopHidden(false);hud.SetBottomHidden(false);pan.RecenterCamera();Size(19);
            Next(0,2);EditorApplication.update+=Tick;
        }
        static void Tick()
        {
            if(!EditorApplication.isPlaying||!fit){report.status="interrupted";Save();Restore();return;}
            if(EditorApplication.timeSinceStartup<until)return;
            try
            {
                switch(phase)
                {
                    case 0: FitCheck("iPad-Grid-Open");Next(1,.4f);break;
                    case 1: hud.SetTopHidden(true);hud.SetBottomHidden(true);Next(2,2);break;
                    case 2: FitCheck("iPad-Grid-Hidden");Next(3,.4f);break;
                    case 3: Size(41);hud.SetTopHidden(false);hud.SetBottomHidden(false);Next(4,3);break;
                    case 4: FitCheck("iPhone-Grid-Open");Next(5,.4f);break;
                    case 5: hud.SetTopHidden(true);hud.SetBottomHidden(true);Next(6,2);break;
                    case 6: FitCheck("iPhone-Grid-Hidden");Next(7,.4f);break;
                    case 7: pan.SetTargetPosition(new Vector2(0,13.6f));pan.SetZoom(4.2f);Next(8,2.5f);break;
                    case 8: Check("North area camera reachable",Camera.main.transform.position.y>10);Capture("North-Royal-Garden-Ingame");Next(9,.4f);break;
                    case 9: pan.SetTargetPosition(new Vector2(0,-13.6f));Next(10,3);break;
                    case 10: Check("South area camera reachable",Camera.main.transform.position.y< -10);Capture("South-Village-Ingame");Next(11,.4f);break;
                    case 11: hud.SetTopHidden(false);hud.SetBottomHidden(false);hud.ReturnToBattle();Next(12,3);break;
                    case 12:
                        FitCheck("iPhone-Returned-To-Battle");Check("All forty deployment cells retained",fit.board.GetComponentsInChildren<SoulCrystalDefense.Grid.PlacementCell>().Length==40);
                        report.status=report.checks.All(r=>r.passed)?"passed":"failed";Save();Restore();Debug.Log("WORLD_FRAMING_QA|"+report.status);break;
                }
            }
            catch(Exception e){Check(e.Message,false);report.status="failed";Save();Restore();Debug.LogException(e);}
        }
    }
}
