using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SoulCrystalDefense.Data;
using SoulCrystalDefense.Grid;
using SoulCrystalDefense.Systems;
using SoulCrystalDefense.Units;

namespace SoulCrystalDefense.UI
{
    /// <summary>Controls saved canvas objects only. Completion follows real gameplay events.</summary>
    public sealed class FirstWaveTutorial : MonoBehaviour
    {
        public const string SeenKey = "SoulCrystal.FirstWaveTutorial.v1";
        public const string ReplayKey = "SoulCrystal.ReplayTutorial";
        public static FirstWaveTutorial Instance { get; private set; }
        [Serializable] public class Step
        {
            public string speaker;
            [TextArea(2,5)] public string instruction;
            public Sprite portrait;
            public AudioClip voice;
        }
        [Header("Editable dialogue and optional recordings")]
        public Step[] steps;
        public AudioSource voiceSource;
        [Header("Authored canvas references")]
        public RectTransform composition,dialogue,targetHighlight;
        public CanvasGroup dialogueGroup;
        public TextMeshProUGUI speakerText,instructionText,progressText,continueText,hintText;
        public Image portraitImage,highlightImage;
        public Button continueButton,skipButton;
        public CanvasGroup combatGuide;
        public Button combatSkipButton;
        public GameObject countdownDisplay;
        public DefenderCardUI guardianCard,archerCard,chefCard;
        public CanvasGroup[] guardedControls;
        public Button[] guardedButtons;
        public CanvasGroup[] obstructingAnnouncements;
        public bool Running { get; private set; }
        public int CurrentStep { get; private set; }=-1;
        public bool HoldsClock => Running && CurrentStep != 5;
        bool clockOwned;
        float previousTimeScale=1,shownAt;
        bool[] originalControlStates;
        bool[] originalAnnouncementStates;
        PlacementController placement;
        WaveDirector director;
        readonly Vector3[] cardCorners=new Vector3[4];

        void Awake()
        {
            Instance=this;
            dialogue.gameObject.SetActive(false);targetHighlight.gameObject.SetActive(false);
            combatGuide.gameObject.SetActive(false);
            continueButton.onClick.AddListener(Continue);skipButton.onClick.AddListener(Skip);
            combatSkipButton.onClick.AddListener(Skip);
            originalControlStates=new bool[guardedControls.Length];
            for(int i=0;i<guardedControls.Length;i++)originalControlStates[i]=guardedControls[i]&&guardedControls[i].interactable;
            originalAnnouncementStates=new bool[obstructingAnnouncements.Length];
            for(int i=0;i<obstructingAnnouncements.Length;i++)originalAnnouncementStates[i]=obstructingAnnouncements[i]&&obstructingAnnouncements[i].gameObject.activeSelf;
            if(chefCard==null||guardianCard==null||archerCard==null)
            {
                var allCards=FindObjectsByType<DefenderCardUI>(FindObjectsInactive.Include,FindObjectsSortMode.None);
                foreach(var card in allCards)
                {
                    if(card==null||card.Data==null)continue;
                    if(guardianCard==null&&card.Data.defenderType==DefenderType.DwarfGuardian)guardianCard=card;
                    if(archerCard==null&&card.Data.defenderType==DefenderType.SoulArcher)archerCard=card;
                    if(chefCard==null&&card.Data.defenderType==DefenderType.BloodChef)chefCard=card;
                }
            }
        }
        IEnumerator Start()
        {
            // Wait until GameManager is active and ready before evaluating tutorial launch.
            while (GameManager.Instance == null || !GameManager.Instance.IsGameActive)
            {
                yield return null;
            }

            placement = PlacementController.Instance;
            director = WaveDirector.Instance;
            if (placement) { placement.OnCardSelected += Selected; placement.OnDefenderPlaced += Placed; }
            if (director) director.OnWaveCompleted += WaveCompleted;

            bool replay = PlayerPrefs.GetInt(ReplayKey, 0) == 1;
            PlayerPrefs.DeleteKey(ReplayKey);
            bool seen = PlayerPrefs.GetInt(SeenKey, 0) == 1;

            if (replay || !seen)
            {
                Begin();
            }
        }
        void Begin()
        {
            if (steps == null || steps.Length == 0) return;
            if (!GameManager.Instance || !GameManager.Instance.IsGameActive) return;
            previousTimeScale = Time.timeScale > 0 ? Time.timeScale : 1;
            Running = true;
            placement?.CancelPlacement();
            foreach (var group in obstructingAnnouncements)
            {
                if (group) group.gameObject.SetActive(false);
            }
            Enter(0);
        }
        void Enter(int step)
        {
            CurrentStep=step;shownAt=Time.unscaledTime;
            var entry=steps[step];speakerText.text=entry.speaker;instructionText.text=entry.instruction;
            progressText.text=$"{step+1} / 7";portraitImage.sprite=entry.portrait;
            dialogue.gameObject.SetActive(step!=5);combatGuide.gameObject.SetActive(step==5);dialogueGroup.alpha=0;
            continueButton.gameObject.SetActive(step==0||step==6);
            continueText.text=step==6?"KEEP PLAYING":"LET'S GO";
            hintText.text=step==5?"DEFEND ALL THREE LANES  /  SKIP ANYTIME":"FOLLOW THE GLOWING TARGET  /  SKIP ANYTIME";
            // Guided placements must remain affordable while the battle clock is held.
            var required=ExpectedCard();
            if(required&&SoulEnergyWallet.Instance)
            {
                int grant=Mathf.Max(0,required.Data.soulCost-SoulEnergyWallet.Instance.CurrentEnergy);
                if(grant>0){SoulEnergyWallet.Instance.AddEnergy(grant);instructionText.text+=$"\nTRAINING BONUS: {grant} SOULS.";}
            }
            if(voiceSource){voiceSource.Stop();if(entry.voice)voiceSource.PlayOneShot(entry.voice);}
            ApplyControls();ApplyClock();
        }
        DefenderCardUI ExpectedCard()
        {
            if (CurrentStep == 1 || CurrentStep == 2) return guardianCard;
            if (CurrentStep == 3) return archerCard;
            if (CurrentStep == 4) return chefCard != null ? chefCard : guardianCard;
            return null;
        }
        public bool AllowsCard(DefenderCardUI card)=>!HoldsClock||(ExpectedCard()!=null&&card==ExpectedCard());
        public bool AllowsPlacement(DefenderData data,int lane,int row)
        {
            if(!HoldsClock)return true;
            int targetLane=CurrentStep==2?2:CurrentStep==3?1:CurrentStep==4?3:-1;
            int targetRow=CurrentStep==3?2:3;
            var expected=ExpectedCard();
            return targetLane>=0&&lane==targetLane&&row==targetRow&&expected&&data==expected.Data;
        }
        void Selected(DefenderCardUI card){if(Running&&CurrentStep==1&&card==guardianCard)Enter(2);}
        void Placed(DefenderUnit unit)
        {
            if(!Running)return;
            if(CurrentStep>=2&&CurrentStep<=4&&AllowsPlacement(unit.Data,unit.LaneIndex,unit.RowIndex))Enter(CurrentStep+1);
        }
        void WaveCompleted(WaveData wave){if(Running&&CurrentStep==5&&wave&&wave.waveNumber==1)Enter(6);}
        public void Continue()
        {
            if(!Running)return;
            if(CurrentStep==0)Enter(1);
            else if(CurrentStep==6)Finish(true);
        }
        public void Skip(){if(Running)Finish(true);}
        public void Replay()
        {
            PlayerPrefs.SetInt(ReplayKey,1);PlayerPrefs.Save();
            if(Running)Finish(false);
            Time.timeScale=1;SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        void Finish(bool remember)
        {
            Running=false;
            if(remember){PlayerPrefs.SetInt(SeenKey,1);PlayerPrefs.Save();}
            placement?.CancelPlacement();
            dialogue.gameObject.SetActive(false);targetHighlight.gameObject.SetActive(false);
            combatGuide.gameObject.SetActive(false);
            if(countdownDisplay)countdownDisplay.SetActive(director&&director.CountdownRemaining>0);
            if(voiceSource)voiceSource.Stop();
            RestoreControls();ReleaseClock();
            for(int i=0;i<obstructingAnnouncements.Length;i++)if(obstructingAnnouncements[i])obstructingAnnouncements[i].gameObject.SetActive(originalAnnouncementStates[i]);
        }
        void ApplyControls()
        {
            var expected=ExpectedCard();
            for(int i=0;i<guardedControls.Length;i++)
                if(guardedControls[i])guardedControls[i].interactable=originalControlStates[i]&&(!HoldsClock||(expected&&guardedButtons[i].GetComponent<DefenderCardUI>()==expected));
        }
        void RestoreControls(){for(int i=0;i<guardedControls.Length;i++)if(guardedControls[i])guardedControls[i].interactable=originalControlStates[i];}
        void ApplyClock()
        {
            if(HoldsClock){clockOwned=true;Time.timeScale=0;}
            else ReleaseClock();
        }
        void ReleaseClock(){if(clockOwned){clockOwned=false;if(!GameManager.Instance||!GameManager.Instance.IsPaused)Time.timeScale=previousTimeScale;}}
        void LateUpdate()
        {
            if(!Running)return;
            if(!GameManager.Instance||!GameManager.Instance.IsGameActive){Finish(false);return;}
            bool paused=GameManager.Instance.IsPaused;
            combatGuide.alpha=paused?0:1;combatGuide.interactable=!paused;combatGuide.blocksRaycasts=!paused;
            if(HoldsClock&&countdownDisplay)countdownDisplay.SetActive(false);
            dialogueGroup.alpha=paused?0:Mathf.MoveTowards(dialogueGroup.alpha,1,Time.unscaledDeltaTime*6);
            dialogueGroup.blocksRaycasts=!paused;dialogueGroup.interactable=!paused;
            ApplyClock();
            if(paused){targetHighlight.gameObject.SetActive(false);return;}
            RectTransform cardTarget=null;PlacementCell cellTarget=null;
            var expected=ExpectedCard();
            if(expected)
            {
                if(CurrentStep==1||!placement||placement.SelectedCard!=expected)cardTarget=expected.GetComponent<RectTransform>();
                else if(CurrentStep>=2&&CurrentStep<=4)cellTarget=BoardGrid.Instance.GetCell(CurrentStep==2?2:CurrentStep==3?1:3,CurrentStep==3?2:3);
            }
            targetHighlight.gameObject.SetActive(cardTarget||cellTarget);
            var space=(RectTransform)targetHighlight.parent;
            var canvas=space.GetComponentInParent<Canvas>().rootCanvas;
            var uiCamera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:(canvas.worldCamera?canvas.worldCamera:Camera.main);
            if(cardTarget)
            {
                cardTarget.GetWorldCorners(cardCorners);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(space,RectTransformUtility.WorldToScreenPoint(uiCamera,cardCorners[0]),uiCamera,out var lo);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(space,RectTransformUtility.WorldToScreenPoint(uiCamera,cardCorners[2]),uiCamera,out var hi);
                targetHighlight.anchoredPosition=(lo+hi)*.5f-space.rect.center;targetHighlight.sizeDelta=hi-lo+new Vector2(12,12);
            }
            else if(cellTarget&&Camera.main)
            {
                Vector2 screen=Camera.main.WorldToScreenPoint(cellTarget.transform.position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(space,screen,uiCamera,out var local);
                targetHighlight.anchoredPosition=local-space.rect.center;
                Vector2 half=Camera.main.WorldToScreenPoint(cellTarget.transform.position+new Vector3(.62f,.45f,0));
                RectTransformUtility.ScreenPointToLocalPointInRectangle(space,half,uiCamera,out var corner);
                targetHighlight.sizeDelta=new Vector2(Mathf.Abs(corner.x-local.x),Mathf.Abs(corner.y-local.y))*2;
            }
            highlightImage.color=Color.Lerp(new Color(.1f,.8f,1,.45f),Color.white,(Mathf.Sin(Time.unscaledTime*5)+1)*.5f);
        }
        void OnDisable(){if(Running)Finish(false);}
        void OnDestroy()
        {
            if(placement){placement.OnCardSelected-=Selected;placement.OnDefenderPlaced-=Placed;}
            if(director)director.OnWaveCompleted-=WaveCompleted;
            if(Instance==this)Instance=null;
        }
    }
}
