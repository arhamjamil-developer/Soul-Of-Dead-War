using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using SoulCrystalDefense.UI;

namespace SoulCrystalDefense.EditorTools
{
    [InitializeOnLoad]
    public static class CrimsonMenuRuntimeQA
    {
        [Serializable] public class Check{public string name;public bool passed;public int width,height;}
        [Serializable] public class Report{public string status="running";public List<Check> checks=new List<Check>();}
        const string Launch="CrimsonMenuQA.Run";
        static Report report;static MainMenuController menu;static Button[] buttons;static int phase;static double until;
        static readonly List<InputDevice> devices=new List<InputDevice>();
        static CrimsonMenuRuntimeQA(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Launch,false)){SessionState.SetBool(Launch,false);EditorApplication.delayCall+=Start;}};}
        public static void Run(){SessionState.SetBool(Launch,true);EditorApplication.isPlaying=true;}
        static void Size(int i){var v=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));v.GetType().GetProperty("selectedSizeIndex").SetValue(v,i);v.Focus();}
        static void Save(){Directory.CreateDirectory("ArtReview/CrimsonMenu");File.WriteAllText("ArtReview/CrimsonMenu/Runtime-QA.json",JsonUtility.ToJson(report,true));}
        static void CheckResult(string name,bool pass){report.checks.Add(new Check{name=name,passed=pass,width=Screen.width,height=Screen.height});Save();}
        static void Restore(){foreach(var d in devices)if(d.added)InputSystem.EnableDevice(d);devices.Clear();EditorApplication.update-=Tick;}
        static void Next(int n,float seconds){phase=n;until=EditorApplication.timeSinceStartup+seconds;}
        static void Capture(string name)=>ScreenCapture.CaptureScreenshot(Path.GetFullPath("ArtReview/CrimsonMenu/"+name+".png"));
        static void Start()
        {
            report=new Report();menu=UnityEngine.Object.FindFirstObjectByType<MainMenuController>();
            buttons=UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Where(b=>b.name.StartsWith("Button_")&&b.transform.Find("CrimsonVisual")).ToArray();
            foreach(var d in InputSystem.devices)if(d.enabled){devices.Add(d);InputSystem.DisableDevice(d);}
            menu.ClosePanels(false);Size(41);Next(0,1.2f);EditorApplication.update+=Tick;
        }
        static void Layout(string name)
        {
            bool fits=true,white=true,hit=true,textFits=true;
            foreach(var button in buttons)
            {
                var text=button.GetComponentInChildren<TextMeshProUGUI>();text.ForceMeshUpdate();white&=text.color==Color.white;textFits&=!text.isTextOverflowing;
                var canvas=button.GetComponentInParent<Canvas>();var camera=canvas.worldCamera;var rect=button.GetComponent<RectTransform>();var corners=new Vector3[4];rect.GetWorldCorners(corners);
                foreach(var c in corners){var p=RectTransformUtility.WorldToScreenPoint(camera,c);fits&=p.x>=-1&&p.x<=Screen.width+1&&p.y>=-1&&p.y<=Screen.height+1;}
                var results=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(camera,rect.position)},results);hit&=results.Count>0&&results[0].gameObject==button.gameObject;
            }
            var badge=menu.highestWaveTMP;badge.ForceMeshUpdate();textFits&=!badge.isTextOverflowing;white&=badge.color==Color.white;
            var bcanvas=badge.GetComponentInParent<Canvas>();var bc=new Vector3[4];badge.transform.parent.GetComponent<RectTransform>().GetWorldCorners(bc);
            foreach(var c in bc){var p=RectTransformUtility.WorldToScreenPoint(bcanvas.worldCamera,c);fits&=p.x>=-1&&p.x<=Screen.width+1&&p.y>=-1&&p.y<=Screen.height+1;}
            CheckResult(name+": all controls on screen",fits);CheckResult(name+": white text fits",white&&textFits);CheckResult(name+": all five buttons receive raycasts",hit&&buttons.Length==5);
        }
        static void Scores()
        {
            bool had=PlayerPrefs.HasKey("HighestWaveReached");int old=PlayerPrefs.GetInt("HighestWaveReached");bool pass=true;
            try{foreach(int score in new[]{0,9,20,40,int.MaxValue}){PlayerPrefs.SetInt("HighestWaveReached",score);menu.UpdateHighestWaveDisplay();menu.highestWaveTMP.ForceMeshUpdate();pass&=menu.highestWaveTMP.text.Contains(score.ToString())&&menu.highestWaveTMP.color==Color.white&&!menu.highestWaveTMP.isTextOverflowing;}}
            finally{if(had)PlayerPrefs.SetInt("HighestWaveReached",old);else PlayerPrefs.DeleteKey("HighestWaveReached");menu.UpdateHighestWaveDisplay();PlayerPrefs.Save();}
            CheckResult("Dynamic highest wave and ranks fit, including large values",pass);
        }
        static void Pointer(Button b,ExecuteEvents.EventFunction<IPointerDownHandler> handler)=>ExecuteEvents.Execute(b.gameObject,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},handler);
        static void Tick()
        {
            if(!EditorApplication.isPlaying||!menu){report.status="interrupted";Save();Restore();return;}
            if(EditorApplication.timeSinceStartup<until)return;
            try
            {
                var play=buttons.First(b=>b.name=="Button_Play");
                switch(phase)
                {
                    case 0: Layout("Phone");Scores();Capture("MainMenu-Phone");Next(1,.4f);break;
                    case 1: Pointer(play,ExecuteEvents.pointerDownHandler);Next(2,.25f);break;
                    case 2:
                        CheckResult("Native press animation contracts visual to 97 percent",Mathf.Abs(play.transform.Find("CrimsonVisual").localScale.x-.97f)<.005f);Capture("MainMenu-Pressed");
                        ExecuteEvents.Execute(play.gameObject,new PointerEventData(EventSystem.current),ExecuteEvents.pointerUpHandler);Next(3,.3f);break;
                    case 3: ExecuteEvents.Execute(play.gameObject,new PointerEventData(EventSystem.current),ExecuteEvents.pointerEnterHandler);Next(4,.25f);break;
                    case 4:
                        CheckResult("Native hover animation expands visual to 102 percent",Mathf.Abs(play.transform.Find("CrimsonVisual").localScale.x-1.02f)<.005f);Capture("MainMenu-Highlighted");
                        ExecuteEvents.Execute(play.gameObject,new PointerEventData(EventSystem.current),ExecuteEvents.pointerExitHandler);Next(5,.4f);break;
                    case 5: Size(19);Next(6,1.2f);break;
                    case 6: Layout("iPad");Capture("MainMenu-iPad");Next(7,.4f);break;
                    case 7:
                        foreach(var pair in new[]{new[]{"Settings","settings"},new[]{"Shop","shop"},new[]{"HowToPlay","help"},new[]{"Exit","exit"}})
                        {
                            menu.ClosePanels(false);buttons.First(b=>b.name=="Button_"+pair[0]).onClick.Invoke();
                            var target=pair[1]=="settings"?menu.settingsPanel:pair[1]=="shop"?menu.shopPanel:pair[1]=="help"?menu.howToPlayPanel:menu.exitPanel;
                            CheckResult(pair[0]+" retained navigation",target.activeSelf);
                        }
                        menu.ClosePanels(false);CheckResult("Play retained persistent scene-loading callback",play.onClick.GetPersistentEventCount()>0&&play.onClick.GetPersistentMethodName(0)=="Play");
                        report.status=report.checks.All(c=>c.passed)?"passed":"failed";Save();Restore();Size(41);Debug.Log("CRIMSON_MENU_QA|"+report.status+"|"+report.checks.Count);EditorApplication.isPlaying=false;break;
                }
            }
            catch(Exception e){CheckResult(e.Message,false);report.status="failed";Save();Restore();Debug.LogException(e);}
        }
    }
}
