using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SoulCrystalDefense.UI
{
    // All artwork, controls, panels and events are authored in MainMenu.unity.
    public sealed class MainMenuController : MonoBehaviour
    {
        public GameObject settingsPanel;
        public GameObject shopPanel;
        public GameObject howToPlayPanel;
        public GameObject exitPanel;
        public Slider volumeSlider;
        public Text volumeLabel;
        public Text exitMessage;
        public Button playButton;
        private bool loading;

        private void Start()
        {
            ClosePanels();
            float volume = PlayerPrefs.GetFloat("MainMenu.MasterVolume", 1f);
            volumeSlider.SetValueWithoutNotify(volume);
            AudioListener.volume = volume;
            UpdateVolumeLabel(volume);
        }

        public void Play()
        {
            if (loading) return;
            loading = true;
            playButton.interactable = false;
            Time.timeScale = 1f;
            SceneManager.LoadSceneAsync("SoulCrystalDefenseGameplay");
        }

        public void ClosePanels()
        {
            settingsPanel.SetActive(false);
            shopPanel.SetActive(false);
            howToPlayPanel.SetActive(false);
            exitPanel.SetActive(false);
        }

        public void OpenSettings() { ClosePanels(); settingsPanel.SetActive(true); }
        public void OpenShop() { ClosePanels(); shopPanel.SetActive(true); }
        public void OpenHowToPlay() { ClosePanels(); howToPlayPanel.SetActive(true); }
        public void OpenExit() { ClosePanels(); exitPanel.SetActive(true); }

        public void SetVolume(float value)
        {
            AudioListener.volume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("MainMenu.MasterVolume", AudioListener.volume);
            PlayerPrefs.Save();
            UpdateVolumeLabel(AudioListener.volume);
        }

        private void UpdateVolumeLabel(float value)
        {
            volumeLabel.text = "MASTER VOLUME  " + Mathf.RoundToInt(value * 100f) + "%";
        }

        public void ConfirmExit()
        {
            // iOS owns application lifecycle; keep the requested Exit control useful
            // without terminating the process on Apple devices.
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                exitMessage.text = "To leave the game, swipe up to return to your Home Screen.";
                return;
            }
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

