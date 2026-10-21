using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using SoulCrystalDefense.UI;
using SoulCrystalDefense.Data;

namespace SoulCrystalDefense.EditorTools
{
    public static class TutorialIntegrationAuthoring
    {
        const string Root="Assets/UI/Totorial/";
        static Transform Find(string name)=>Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(t=>t.name==name);
        static T Child<T>(Transform root,string name) where T:Component=>root.GetComponentsInChildren<T>(true).First(t=>t.name==name);
        [MenuItem("SoulCrystalDefense/Tutorial/Install Manual Tutorial")]
        public static void Install()
        {
            if(EditorApplication.isPlaying)throw new System.Exception("Stop Play Mode before authoring");
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(scene.name!="SoulCrystalDefenseGameplay")throw new System.Exception("Open SoulCrystalDefenseGameplay first");
            var composition=Find("GameplayComposition");
            var existing=composition.Find("FirstWaveTutorialSystem");if(existing)Object.DestroyImmediate(existing.gameObject);
            var root=new GameObject("FirstWaveTutorialSystem",typeof(RectTransform),typeof(AudioSource),typeof(FirstWaveTutorial));
            root.transform.SetParent(composition,false);var rr=root.GetComponent<RectTransform>();rr.anchorMin=Vector2.zero;rr.anchorMax=Vector2.one;rr.offsetMin=rr.offsetMax=Vector2.zero;
            var panel=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/TutorialDialogue.prefab"),root.transform);
            panel.name="TutorialDialogue";var rt=panel.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=new Vector2(.5f,1);rt.pivot=new Vector2(.5f,1);rt.anchoredPosition=new Vector2(0,-280);rt.localScale=Vector3.one*.85f;
            var group=panel.GetComponent<CanvasGroup>();
            if(!group)group=panel.AddComponent<CanvasGroup>();
            EditorUtility.SetDirty(group);
            PrefabUtility.RecordPrefabInstancePropertyModifications(group);
            var t=root.GetComponent<FirstWaveTutorial>();t.composition=composition.GetComponent<RectTransform>();t.dialogue=rt;t.dialogueGroup=group;
            t.speakerText=Child<TextMeshProUGUI>(panel.transform,"Speaker_PAYBACK");t.instructionText=Child<TextMeshProUGUI>(panel.transform,"Instruction_PAYBACK");t.progressText=Child<TextMeshProUGUI>(panel.transform,"Progress_PAYBACK");
            t.portraitImage=Child<Image>(panel.transform,"GuidePortrait");t.continueButton=Child<Button>(panel.transform,"ContinueButton");t.skipButton=Child<Button>(panel.transform,"SkipButton");t.continueText=Child<TextMeshProUGUI>(panel.transform,"Continue_PAYBACK");
            t.hintText=NewLabel("ActionHint_PAYBACK",panel.transform,"FOLLOW THE GLOWING TARGET / SKIP ANYTIME",18,504,964,52,23);
            var compact=new GameObject("CombatTutorialGuide",typeof(RectTransform),typeof(Image),typeof(CanvasGroup));compact.transform.SetParent(root.transform,false);
            Place(compact.GetComponent<RectTransform>(),40,280,1000,112);
            var compactImage=compact.GetComponent<Image>();compactImage.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Art/TutorialFrame.png");compactImage.type=Image.Type.Sliced;compactImage.pixelsPerUnitMultiplier=4;compactImage.raycastTarget=true;
            t.combatGuide=compact.GetComponent<CanvasGroup>();
            NewLabel("CombatObjective_PAYBACK",compact.transform,"6 / 7  -  CLEAR WAVE ONE\nDEFEND ALL THREE LANES",28,14,730,84,27);
            var cs=new GameObject("CombatSkip",typeof(RectTransform),typeof(Image),typeof(Button));cs.transform.SetParent(compact.transform,false);Place(cs.GetComponent<RectTransform>(),798,22,175,68);
            var csi=cs.GetComponent<Image>();csi.sprite=compactImage.sprite;csi.type=Image.Type.Sliced;csi.pixelsPerUnitMultiplier=6;
            t.combatSkipButton=cs.GetComponent<Button>();t.combatSkipButton.targetGraphic=csi;
            NewLabel("CombatSkip_PAYBACK",cs.transform,"SKIP",8,5,159,58,25);
            var hud=Object.FindFirstObjectByType<GameplayHUD>();if(hud)t.countdownDisplay=(GameObject)new SerializedObject(hud).FindProperty("countdownContainer").objectReferenceValue;
            var highlight=new GameObject("TutorialTargetHighlight",typeof(RectTransform),typeof(Image));highlight.transform.SetParent(root.transform,false);
            var hr=highlight.GetComponent<RectTransform>();hr.anchorMin=hr.anchorMax=hr.pivot=new Vector2(.5f,.5f);
            var hi=highlight.GetComponent<Image>();hi.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Art/TutorialFrame.png");hi.type=Image.Type.Sliced;hi.fillCenter=false;hi.raycastTarget=false;hi.pixelsPerUnitMultiplier=3;
            t.targetHighlight=hr;t.highlightImage=hi;highlight.transform.SetAsFirstSibling();
            var cards=Find("BottomTray").GetComponentsInChildren<DefenderCardUI>(true);
            t.guardianCard=cards.First(c=>c.Data&&c.Data.defenderType==DefenderType.DwarfGuardian);
            t.archerCard=cards.First(c=>c.Data&&c.Data.defenderType==DefenderType.SoulArcher);
            t.guardedButtons=Find("BottomTray").GetComponentsInChildren<Button>(true);
            t.guardedControls=t.guardedButtons.Select(b=>
            {
                var control=b.GetComponent<CanvasGroup>();
                // Unity missing-component wrappers must use its bool operator, not C# ??.
                if(!control)control=b.gameObject.AddComponent<CanvasGroup>();
                EditorUtility.SetDirty(control);
                if(PrefabUtility.IsPartOfPrefabInstance(control))PrefabUtility.RecordPrefabInstancePropertyModifications(control);
                return control;
            }).ToArray();
            if(t.guardedControls.Any(c=>!c))throw new System.Exception("Tutorial control serialization failed");
            t.obstructingAnnouncements=Object.FindObjectsByType<SoulCrystalDefense.Presentation.BattlePresentation>(FindObjectsSortMode.None).Where(p=>p.announcement).Select(p=>p.announcement).ToArray();
            var audio=root.GetComponent<AudioSource>();audio.playOnAwake=false;audio.spatialBlend=0;t.voiceSource=audio;
            string[] who={"GUARDIAN","GUARDIAN","GUARDIAN","ARCHER","CHEF","GUARDIAN","MAGE"};
            string[] portraits={"Dwarf","Dwarf","Dwarf","Archer","Chef","Dwarf","Mage"};
            string[] instructions={
                "PROTECT OUR SOUL CRYSTAL!\nLET'S PREPARE FOR WAVE ONE.",
                "TAP THE GLOWING GUARDIAN CARD.\nI'LL HOLD THE FRONT LINE!",
                "TAP THE GLOWING CENTER PATCH\nTO PLACE YOUR GUARDIAN.",
                "SELECT THE ARCHER CARD.\nPLACE YOUR ARCHER ON THE GLOWING LEFT PATCH.",
                "SELECT THE CHEF CARD.\nDEFEND THE GLOWING RIGHT PATCH. CHECK YOUR SOULS!",
                "HOLD THE LINE!\nDEFEAT THE REMAINING ENEMIES TO CLEAR WAVE ONE.",
                "WAVE ONE CLEARED!\nKEEP DEFENDING. THE WAVES ARE ENDLESS!"};
            t.steps=Enumerable.Range(0,7).Select(i=>new FirstWaveTutorial.Step {speaker=who[i],instruction=instructions[i],portrait=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Portraits/"+portraits[i]+".png")}).ToArray();
            foreach(var button in root.GetComponentsInChildren<Button>(true)){var nav=button.navigation;nav.mode=Navigation.Mode.None;button.navigation=nav;}
            // A permanent replay button is authored in the existing How To Play screen.
            var howTo=Find("BattleTutorialScreen");var old=howTo.Find("ReplayTutorial");if(old)Object.DestroyImmediate(old.gameObject);
            var replay=new GameObject("ReplayTutorial",typeof(RectTransform),typeof(Image),typeof(Button));replay.transform.SetParent(howTo,false);
            Place(replay.GetComponent<RectTransform>(),260,1590,560,100);
            var image=replay.GetComponent<Image>();image.sprite=hi.sprite;image.type=Image.Type.Sliced;image.pixelsPerUnitMultiplier=4;
            var rb=replay.GetComponent<Button>();rb.targetGraphic=image;UnityEventTools.AddPersistentListener(rb.onClick,t.Replay);
            NewLabel("Replay_PAYBACK",replay.transform,"REPLAY TUTORIAL",20,12,520,76,34);
            // The scene already supports endless procedural waves; retain it explicitly.
            var manager=Object.FindFirstObjectByType<SoulCrystalDefense.Systems.GameManager>();var so=new SerializedObject(manager);so.FindProperty("useProceduralProgression").boolValue=true;so.FindProperty("unlimitedWaves").boolValue=true;so.ApplyModifiedPropertiesWithoutUndo();
            foreach(var component in panel.GetComponentsInChildren<Component>(true))if(component)PrefabUtility.RecordPrefabInstancePropertyModifications(component);
            var payback=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Final/PaybAck SDF.asset");
            foreach(var label in root.GetComponentsInChildren<TMP_Text>(true))
            {
                label.font=payback;
                if(PrefabUtility.IsPartOfPrefabInstance(label))PrefabUtility.RecordPrefabInstancePropertyModifications(label);
            }
            Child<Image>(panel.transform,"DialogueBorder").raycastTarget=true;
            Child<Image>(panel.transform,"PortraitHolder").raycastTarget=true;
            foreach(var imageComponent in panel.GetComponentsInChildren<Image>(true))PrefabUtility.RecordPrefabInstancePropertyModifications(imageComponent);
            EditorUtility.SetDirty(t);
            compact.SetActive(false);highlight.SetActive(false);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("TUTORIAL_INSTALL|Saved manual canvas, 7 editable steps, highlights, skip/replay, voice slots and endless mode");
        }
        static void Place(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
        static TextMeshProUGUI NewLabel(string name,Transform parent,string text,float x,float y,float w,float h,float size)
        {
            var existing=parent.Find(name);
            var go=existing?existing.gameObject:new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));
            if(!existing)go.transform.SetParent(parent,false);Place(go.GetComponent<RectTransform>(),x,y,w,h);
            var label=go.GetComponent<TextMeshProUGUI>();label.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Final/PaybAck SDF.asset");label.text=text;label.color=new Color(.015f,.1f,.3f);label.fontSize=label.fontSizeMax=size;label.fontSizeMin=size*.75f;label.enableAutoSizing=true;label.alignment=TextAlignmentOptions.Center;label.overflowMode=TextOverflowModes.Ellipsis;label.raycastTarget=false;return label;
        }
    }
}
