using UnityEngine;
using UnityEngine.UI;
using SoulCrystalDefense.Combat;
using SoulCrystalDefense.Systems;

namespace SoulCrystalDefense.UI
{
    public class GameplayHUD : MonoBehaviour
    {
        [Header("Crystal Health")]
        [SerializeField] private Text crystalHealthText;
        [SerializeField] private Image crystalHealthBar;

        [Header("Wave Progress")]
        [SerializeField] private Text waveText;
        [SerializeField] private Text countdownText;
        [SerializeField] private GameObject countdownContainer;

        [Header("Soul Energy")]
        [SerializeField] private Text energyText;

        [Header("Controls")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Text pauseButtonText;
        public GameScreenFlow screenFlow;

        private void Start()
        {
            if (SoulCrystal.Instance != null)
            {
                SoulCrystal.Instance.OnHealthChanged += UpdateCrystalHealth;
                UpdateCrystalHealth(SoulCrystal.Instance.CurrentHealth, SoulCrystal.Instance.MaxHealth);
            }

            if (SoulEnergyWallet.Instance != null)
            {
                SoulEnergyWallet.Instance.OnEnergyChanged += UpdateEnergyText;
                UpdateEnergyText(SoulEnergyWallet.Instance.CurrentEnergy);
            }

            if (WaveDirector.Instance != null)
            {
                WaveDirector.Instance.OnCountdownTick += HandleCountdownTick;
                WaveDirector.Instance.OnCountdownFinished += HandleCountdownFinished;
            }

            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(OnPauseClicked);
            }

            if (countdownContainer != null)
            {
                countdownContainer.SetActive(false);
            }

            // GameManager owns the wave count and initializes this label.
            // Resetting it here made the first procedural wave display 1/1.
        }

        private void OnDestroy()
        {
            if (SoulCrystal.Instance != null)
                SoulCrystal.Instance.OnHealthChanged -= UpdateCrystalHealth;

            if (SoulEnergyWallet.Instance != null)
                SoulEnergyWallet.Instance.OnEnergyChanged -= UpdateEnergyText;

            if (WaveDirector.Instance != null)
            {
                WaveDirector.Instance.OnCountdownTick -= HandleCountdownTick;
                WaveDirector.Instance.OnCountdownFinished -= HandleCountdownFinished;
            }
        }

        public void SetWaveText(int currentWave, int totalWaves)
        {
            if (waveText != null)
            {
                waveText.text = $"WAVE {currentWave}/{totalWaves}";
            }
        }

        private void UpdateCrystalHealth(int current, int max)
        {
            if (crystalHealthText != null)
            {
                crystalHealthText.text = $"{current}/{max}";
            }
            if (crystalHealthBar != null && max > 0)
            {
                crystalHealthBar.fillAmount = (float)current / max;
            }
        }

        private void UpdateEnergyText(int energy)
        {
            if (energyText != null)
            {
                energyText.text = energy.ToString();
            }
        }

        private void HandleCountdownTick(float remaining)
        {
            if (countdownContainer != null) countdownContainer.SetActive(true);
            if (countdownText != null)
            {
                int val = Mathf.CeilToInt(remaining);
                countdownText.text = val > 0 ? val.ToString() : "PREPARE!";
            }
        }

        private void HandleCountdownFinished()
        {
            if (countdownContainer != null) countdownContainer.SetActive(false);
        }

        private void OnPauseClicked()
        {
            if (screenFlow != null) { screenFlow.TogglePause(); return; }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TogglePause();
                if (pauseButtonText != null)
                {
                    pauseButtonText.text = GameManager.Instance.IsPaused ? ">" : "II";
                }
            }
        }
    }
}
