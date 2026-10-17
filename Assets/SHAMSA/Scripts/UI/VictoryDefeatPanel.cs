using UnityEngine;
using UnityEngine.UI;
using SoulCrystalDefense.Systems;

namespace SoulCrystalDefense.UI
{
    public class VictoryDefeatPanel : MonoBehaviour
    {
        [Header("Containers")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private GameObject victoryContainer;
        [SerializeField] private GameObject defeatContainer;

        [Header("Victory Elements")]
        [SerializeField] private Text victoryTitleText;
        [SerializeField] private Image[] starImages;
        [SerializeField] private Color starActiveColor = new Color(1f, 0.85f, 0.1f, 1f);
        [SerializeField] private Color starInactiveColor = new Color(0.3f, 0.3f, 0.3f, 0.7f);
        [SerializeField] private Text rewardFragmentText;
        [SerializeField] private Text totalFragmentsText;
        [SerializeField] private Button victoryReplayButton;
        [SerializeField] private Button victoryNextButton;

        [Header("Defeat Elements")]
        [SerializeField] private Text defeatTitleText;
        [SerializeField] private Text defeatHintText;
        [SerializeField] private Button defeatRetryButton;

        private void Awake()
        {
            if (panelRoot == null) panelRoot = gameObject;
            panelRoot.SetActive(false);

            if (victoryReplayButton != null)
                victoryReplayButton.onClick.AddListener(OnReplayClicked);

            if (victoryNextButton != null)
                victoryNextButton.onClick.AddListener(OnReplayClicked);

            if (defeatRetryButton != null)
                defeatRetryButton.onClick.AddListener(OnReplayClicked);
        }

        public void ShowVictory(int stars, int fragmentReward, int totalFragments)
        {
            panelRoot.SetActive(true);
            if (victoryContainer != null) victoryContainer.SetActive(true);
            if (defeatContainer != null) defeatContainer.SetActive(false);

            if (starImages != null)
            {
                for (int i = 0; i < starImages.Length; i++)
                {
                    if (starImages[i] != null)
                    {
                        starImages[i].color = (i < stars) ? starActiveColor : starInactiveColor;
                    }
                }
            }

            if (rewardFragmentText != null)
            {
                rewardFragmentText.text = $"+{fragmentReward} Crystal Fragment";
            }

            if (totalFragmentsText != null)
            {
                totalFragmentsText.text = $"Total Fragments: {totalFragments} (Forged: {totalFragments / 3})";
            }
        }

        public void ShowDefeat(string hint)
        {
            panelRoot.SetActive(true);
            if (victoryContainer != null) victoryContainer.SetActive(false);
            if (defeatContainer != null) defeatContainer.SetActive(true);

            if (defeatHintText != null)
            {
                defeatHintText.text = hint;
            }
        }

        private void OnReplayClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartLevel();
            }
        }
    }
}
