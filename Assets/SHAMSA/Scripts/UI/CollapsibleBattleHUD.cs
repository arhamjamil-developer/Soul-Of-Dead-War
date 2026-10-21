using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SoulCrystalDefense.Systems;

namespace SoulCrystalDefense.UI
{
    /// <summary>Animates saved panel/handle objects; never builds UI at runtime.</summary>
    public sealed class CollapsibleBattleHUD : MonoBehaviour
    {
        public RectTransform layoutRoot,topPanel,bottomPanel,topHandle,bottomHandle;
        public CanvasGroup topGroup,bottomGroup,floatingPauseGroup,returnBattleGroup;
        public Button topButton,bottomButton;
        public Image topArtwork,bottomArtwork;
        public Sprite up,down,upPressed,downPressed;
        public TextMeshProUGUI exploreHint;
        public GameScreenFlow screenFlow;
        [Range(.15f,.8f)] public float slideSeconds=.38f;
        public bool TopHidden {get;private set;}
        public bool BottomHidden {get;private set;}
        public float TopTravel {get;private set;}
        public float BottomTravel {get;private set;}
        Vector2 topShown,bottomShown;
        float topProgress,bottomProgress,topHandleShown,bottomHandleShown,hintUntil;
        Vector2 measuredSize;
        void Awake()
        {
            PreviewAuthoredLayout();
            floatingPauseGroup.gameObject.SetActive(false);
            returnBattleGroup.gameObject.SetActive(false);
        }
        public void PreviewAuthoredLayout()
        {
            topShown=topPanel.anchoredPosition;bottomShown=bottomPanel.anchoredPosition;
            Canvas.ForceUpdateCanvases();Measure();SetArrows();
            topHandle.anchoredPosition=new Vector2(0,topHandleShown);
            bottomHandle.anchoredPosition=new Vector2(0,bottomHandleShown);
        }
        public void Measure()
        {
            // Measure at authored locations, including pod children which extend past the parent rect.
            var tp=topPanel.anchoredPosition;var bp=bottomPanel.anchoredPosition;
            topPanel.anchoredPosition=topShown;bottomPanel.anchoredPosition=bottomShown;
            var t=RectTransformUtility.CalculateRelativeRectTransformBounds(layoutRoot,topPanel);
            var b=RectTransformUtility.CalculateRelativeRectTransformBounds(layoutRoot,bottomPanel);
            TopTravel=Mathf.Max(topPanel.rect.height,layoutRoot.rect.yMax-t.min.y)+24;
            BottomTravel=Mathf.Max(bottomPanel.rect.height,b.max.y-layoutRoot.rect.yMin)+24;
            topHandleShown=t.min.y-layoutRoot.rect.yMax-78;
            bottomHandleShown=b.max.y-layoutRoot.rect.yMin+78;
            measuredSize=layoutRoot.rect.size;
            topPanel.anchoredPosition=tp;bottomPanel.anchoredPosition=bp;
        }
        bool GuidanceActive=>FirstWaveTutorial.Instance&&FirstWaveTutorial.Instance.Running;
        public void ToggleTop(){SetTopHidden(!TopHidden);}
        public void ToggleBottom(){SetBottomHidden(!BottomHidden);}
        public void SetTopHidden(bool hide)
        {
            if(GuidanceActive)return;
            TopHidden=hide;SetArrows();ClickSound(hide);
        }
        public void SetBottomHidden(bool hide)
        {
            if(GuidanceActive)return;
            if(hide)PlacementController.Instance?.CancelPlacement();
            BottomHidden=hide;hintUntil=Time.unscaledTime+5;SetArrows();ClickSound(hide);
        }
        void ClickSound(bool hide){if(hide)SoundManager.Instance?.PlayUIPanelClose();else SoundManager.Instance?.PlayUIPanelOpen();}
        public void Pause(){if(screenFlow)screenFlow.TogglePause();}
        public void ReturnToBattle(){CinemachineCameraPanController.Instance?.RecenterCamera();}
        void SetArrows()
        {
            SetButton(topButton,topArtwork,TopHidden?down:up,TopHidden?downPressed:upPressed);
            SetButton(bottomButton,bottomArtwork,BottomHidden?up:down,BottomHidden?upPressed:downPressed);
        }
        static void SetButton(Button button,Image image,Sprite normal,Sprite pressed)
        {
            image.sprite=normal;button.transition=Selectable.Transition.SpriteSwap;
            button.spriteState=new SpriteState{highlightedSprite=normal,pressedSprite=pressed,selectedSprite=normal,disabledSprite=normal};
        }
        void LateUpdate()
        {
            if((measuredSize-layoutRoot.rect.size).sqrMagnitude>.01f)Measure();
            float dt=Time.unscaledDeltaTime/Mathf.Max(.1f,slideSeconds);
            topProgress=Mathf.MoveTowards(topProgress,TopHidden?1:0,dt);
            bottomProgress=Mathf.MoveTowards(bottomProgress,BottomHidden?1:0,dt);
            float t=Mathf.SmoothStep(0,1,topProgress),b=Mathf.SmoothStep(0,1,bottomProgress);
            topPanel.anchoredPosition=topShown+Vector2.up*TopTravel*t;
            bottomPanel.anchoredPosition=bottomShown+Vector2.down*BottomTravel*b;
            topGroup.alpha=1-t;bottomGroup.alpha=1-b;
            topGroup.interactable=!TopHidden;bottomGroup.interactable=!BottomHidden;
            topGroup.blocksRaycasts=topProgress<.99f;bottomGroup.blocksRaycasts=bottomProgress<.99f;
            topHandle.anchoredPosition=new Vector2(0,Mathf.Lerp(topHandleShown,-76,t));
            bottomHandle.anchoredPosition=new Vector2(0,Mathf.Lerp(bottomHandleShown,76,b));
            bool modal=(GameManager.Instance&&GameManager.Instance.IsPaused)||(screenFlow&&((screenFlow.pausePanel&&screenFlow.pausePanel.activeInHierarchy)||(screenFlow.settingsPanel&&screenFlow.settingsPanel.activeInHierarchy)||(screenFlow.tutorialPanel&&screenFlow.tutorialPanel.activeInHierarchy)));
            topHandle.gameObject.SetActive(!modal);bottomHandle.gameObject.SetActive(!modal);
            floatingPauseGroup.gameObject.SetActive(topProgress>.01f&&!modal);floatingPauseGroup.alpha=t;
            floatingPauseGroup.interactable=floatingPauseGroup.blocksRaycasts=topProgress>.9f;
            returnBattleGroup.gameObject.SetActive(bottomProgress>.01f&&!modal);returnBattleGroup.alpha=b;
            returnBattleGroup.interactable=returnBattleGroup.blocksRaycasts=bottomProgress>.9f;
            bool allow=!GuidanceActive;
            topButton.interactable=bottomButton.interactable=allow;
            if(exploreHint)exploreHint.alpha=!modal&&BottomHidden&&Time.unscaledTime<hintUntil?Mathf.Clamp01((hintUntil-Time.unscaledTime)*2):0;
        }
        void OnDisable()
        {
            if(topPanel){topPanel.anchoredPosition=topShown;topGroup.alpha=1;topGroup.interactable=topGroup.blocksRaycasts=true;}
            if(bottomPanel){bottomPanel.anchoredPosition=bottomShown;bottomGroup.alpha=1;bottomGroup.interactable=bottomGroup.blocksRaycasts=true;}
        }
    }
}
