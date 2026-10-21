using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SoulCrystalDefense.UI;
using SoulCrystalDefense.Systems;

namespace SoulCrystalDefense.EditorTools
{
    [InitializeOnLoad]
    public static class CrimsonPanelsRuntimeQA
    {
        [Serializable] public class Check{public string name;public bool passed;public int width,height;}
        [Serializable] public class Report{public string status="running";public List<Check> checks=new List<Check>();}
        const string Launch="CrimsonPanelsQA.Launch";
        static Report report;static MainMenuController menu;static GameScreenFlow flow;static int phase,index;static double until;static bool tablet;
        static readonly List<InputDevice> devices=new List<InputDevice>();
        static readonly string[] audioKeys={"MainMenu.MasterVolume","Audio.Music","Audio.Effects"};static bool[] had;static float[] audio;static float listener;
        static CrimsonPanelsRuntimeQA(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Launch,false)){SessionState.SetBool(Launch,false);EditorApplication.delayCall+=Start;}};}
        public static void Run(){SessionState.SetBool(Launch,true);EditorApplication.isPlaying=true;}
        static void Size(int i){var v=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));v.GetType().GetProperty("selectedSizeIndex").SetValue(v,i);v.Focus();}
        static void Save(){Directory.CreateDirectory("ArtReview/ApprovedPanels");File.WriteAllText("ArtReview/ApprovedPanels/Runtime-QA.json",JsonUtility.ToJson(report,true));}
        static void CheckResult(string name,bool pass){report.checks.Add(new Check{name=name,passed=pass,width=Screen.width,height=Screen.height});Save();}
        static void Capture(string name)=>ScreenCapture.CaptureScreenshot(Path.GetFullPath("ArtReview/ApprovedPanels/"+(tablet?"iPad-":"iPhone-")+name+".png"));
        static void Next(int p,float seconds){phase=p;until=EditorApplication.timeSinceStartup+seconds;}
        static void RestoreAudio()
        {
            if(had==null)return;
            for(int i=0;i<3;i++){if(had[i])PlayerPrefs.SetFloat(audioKeys[i],audio[i]);else PlayerPrefs.DeleteKey(audioKeys[i]);}
            PlayerPrefs.Save();AudioListener.volume=listener;if(SoundManager.Instance)SoundManager.Instance.ApplySavedVolumes();
        }
        static void Restore(){RestoreAudio();foreach(var d in devices)if(d.added)InputSystem.EnableDevice(d);devices.Clear();EditorApplication.update-=Tick;}
        static void Start()
        {
            report=new Report();menu=UnityEngine.Object.FindFirstObjectByType<MainMenuController>();
            had=audioKeys.Select(PlayerPrefs.HasKey).ToArray();audio=audioKeys.Select(k=>PlayerPrefs.GetFloat(k,k=="Audio.Music"?.7f:1)).ToArray();listener=AudioListener.volume;
            foreach(var d in InputSystem.devices)if(d.enabled){devices.Add(d);InputSystem.DisableDevice(d);}
            menu.ClosePanels(false);tablet=false;index=0;Size(41);Next(0,1);EditorApplication.update+=Tick;
        }
        static void Validate(GameObject root,string name)
        {
            Canvas.ForceUpdateCanvases();bool textFits=true,inside=true,buttonsHit=true;
            foreach(var text in root.GetComponentsInChildren<TextMeshProUGUI>()){text.ForceMeshUpdate();textFits&=!text.isTextOverflowing;}
            var canvas=root.GetComponentInParent<Canvas>();var content=root.transform.Find("AuthoredPanelContent") as RectTransform;
            if(!content)content=root.transform.Find("VictoryContent") as RectTransform;
            if(content){var corners=new Vector3[4];content.GetWorldCorners(corners);foreach(var c in corners){var p=RectTransformUtility.WorldToScreenPoint(canvas.worldCamera,c);inside&=p.x>=0&&p.x<=Screen.width&&p.y>=0&&p.y<=Screen.height;}}
            foreach(var button in root.GetComponentsInChildren<Button>())
            {
                var hits=new List<RaycastResult>();var pos=RectTransformUtility.WorldToScreenPoint(canvas.worldCamera,button.transform.position);EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=pos},hits);
                buttonsHit&=hits.Count>0&&(hits[0].gameObject==button.gameObject||hits[0].gameObject.transform.IsChildOf(button.transform));
            }
            CheckResult((tablet?"iPad ":"iPhone ")+name+" text and panel fit",textFits&&inside);
            CheckResult((tablet?"iPad ":"iPhone ")+name+" buttons receive input",buttonsHit);
            Capture(name);
        }
        static GameObject MenuPanel(int i)=>new[]{menu.settingsPanel,menu.howToPlayPanel,menu.shopPanel,menu.exitPanel}[i];
        static void MenuOpen(int i){menu.ClosePanels(false);new UnityEngine.Events.UnityAction[]{menu.OpenSettings,menu.OpenHowToPlay,menu.OpenShop,menu.OpenExit}[i]();}
        static void AudioMenu()
        {
            menu.volumeSlider.value=.31f;menu.musicSlider.value=.42f;menu.sfxSlider.value=.53f;
            CheckResult("Menu audio sliders persist values and update TMP",Mathf.Abs(PlayerPrefs.GetFloat(audioKeys[0])-.31f)<.001f&&Mathf.Abs(PlayerPrefs.GetFloat(audioKeys[1])-.42f)<.001f&&Mathf.Abs(PlayerPrefs.GetFloat(audioKeys[2])-.53f)<.001f&&menu.volumeLabelTMP.text=="31%"&&menu.musicLabelTMP.text=="42%"&&menu.sfxLabelTMP.text=="53%");
            menu.SetVolume(audio[0]);menu.SetMusicVolume(audio[1]);menu.SetEffectsVolume(audio[2]);menu.volumeSlider.SetValueWithoutNotify(audio[0]);menu.musicSlider.SetValueWithoutNotify(audio[1]);menu.sfxSlider.SetValueWithoutNotify(audio[2]);RestoreAudio();
        }
        static void Animations()
        {
            var button=menu.playButton;var animator=button.GetComponent<Animator>();
            var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left};
            ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerDownHandler);animator.Update(0);animator.Update(.2f);animator.Update(.2f);
            CheckResult("Native menu press feedback",Mathf.Abs(button.transform.Find("CrimsonVisual").localScale.x-.97f)<.005f);
            ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerEnterHandler);animator.Update(0);animator.Update(.2f);animator.Update(.2f);
            CheckResult("Native menu hover feedback",Mathf.Abs(button.transform.Find("CrimsonVisual").localScale.x-1.02f)<.005f);
            ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerExitHandler);
        }
        static void GameOpen(int i)
        {
            if(i<3){flow.Resume();flow.TogglePause();if(i==1)flow.Settings();if(i==2)flow.Tutorial();return;}
            flow.Resume();var result=UnityEngine.Object.FindFirstObjectByType<VictoryDefeatPanel>(FindObjectsInactive.Include);
            if(i==3)result.ShowVictory(2,4,10);else result.ShowDefeat("DEFEATED ON WAVE 12. HIGHEST WAVE: 20. TRY A STRONGER FRONT LINE.");
        }
        static GameObject GamePanel(int i)
        {
            if(i<3)return new[]{flow.pausePanel,flow.settingsPanel,flow.tutorialPanel}[i];
            var result=UnityEngine.Object.FindFirstObjectByType<VictoryDefeatPanel>(FindObjectsInactive.Include);return (GameObject)new SerializedObject(result).FindProperty("panelRoot").objectReferenceValue;
        }
        static void Tick()
        {
            if(!EditorApplication.isPlaying){report.status="interrupted";Save();Restore();return;}
            if(EditorApplication.timeSinceStartup<until)return;
            try
            {
                switch(phase)
                {
                    case 0: Animations();MenuOpen(0);Next(1,.5f);break;
                    case 1:
                        Validate(MenuPanel(index),new[]{"Settings","HowToPlay","Shop","Exit"}[index]);
                        if(index==0&&!tablet)AudioMenu();Next(2,.25f);break;
                    case 2:
                        if(index<3){index++;MenuOpen(index);Next(1,.5f);break;}
                        if(!tablet){tablet=true;index=0;Size(19);MenuOpen(0);Next(1,1);break;}
                        menu.ClosePanels(false);RestoreAudio();SceneManager.LoadScene("SoulCrystalDefenseGameplay");Next(3,1.4f);break;
                    case 3:
                        flow=UnityEngine.Object.FindFirstObjectByType<GameScreenFlow>();var tutorial=FirstWaveTutorial.Instance;
                        bool seen=PlayerPrefs.HasKey("SoulCrystal.FirstWaveTutorial.v1");int old=PlayerPrefs.GetInt("SoulCrystal.FirstWaveTutorial.v1");if(tutorial&&tutorial.Running)tutorial.Skip();if(seen)PlayerPrefs.SetInt("SoulCrystal.FirstWaveTutorial.v1",old);else PlayerPrefs.DeleteKey("SoulCrystal.FirstWaveTutorial.v1");PlayerPrefs.Save();
                        tablet=false;index=0;Size(41);GameOpen(0);Next(4,1);break;
                    case 4:
                        Validate(GamePanel(index),new[]{"Pause","GameSettings","GameHowToPlay","Victory","Defeat"}[index]);
                        if(index==1&&!tablet)
                        {
                            var audioPanel=flow.settingsPanel.GetComponent<AudioSettingsPanel>();audioPanel.master.value=.26f;audioPanel.music.value=.37f;audioPanel.effects.value=.48f;
                            CheckResult("Paused gameplay audio values and TMP update",GameManager.Instance.IsPaused&&audioPanel.masterValueTMP.text=="26%"&&audioPanel.musicValueTMP.text=="37%"&&audioPanel.effectsValueTMP.text=="48%");RestoreAudio();
                        }
                        if(index==2&&!tablet){var replay=flow.tutorialPanel.GetComponentsInChildren<Button>().First(b=>b.name=="ReplayTutorial");CheckResult("Guided tutorial replay retained",replay.onClick.GetPersistentMethodName(0)=="Replay");}
                        Next(5,.25f);break;
                    case 5:
                        if(index>=3)GamePanel(index).SetActive(false);
                        if(index<4){index++;GameOpen(index);Next(4,.5f);break;}
                        if(!tablet){tablet=true;index=0;Size(19);GameOpen(0);Next(4,1);break;}
                        report.status=report.checks.All(c=>c.passed)?"passed":"failed";Save();Restore();Size(41);Debug.Log("CRIMSON_PANELS_QA|"+report.status+"|"+report.checks.Count);EditorApplication.isPlaying=false;break;
                }
            }
            catch(Exception e){CheckResult(e.Message,false);report.status="failed";Save();Restore();Debug.LogException(e);}
        }
    }
}
