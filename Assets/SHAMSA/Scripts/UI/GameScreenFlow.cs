using UnityEngine;
using UnityEngine.SceneManagement;
using SoulCrystalDefense.Systems;
namespace SoulCrystalDefense.UI
{
    public sealed class GameScreenFlow : MonoBehaviour
    {
        public GameObject pausePanel,settingsPanel,tutorialPanel;
        public void TogglePause()
        {
            if(GameManager.Instance==null||!GameManager.Instance.IsGameActive)return;
            if(GameManager.Instance.IsPaused)Resume();
            else{PlacementController.Instance?.CancelPlacement();GameManager.Instance.TogglePause();ClosePanels();pausePanel.SetActive(true);}
        }
        public void Resume(){ClosePanels();if(GameManager.Instance!=null&&GameManager.Instance.IsPaused)GameManager.Instance.TogglePause();}
        public void Settings(){ClosePanels();settingsPanel.SetActive(true);}
        public void Tutorial(){ClosePanels();tutorialPanel.SetActive(true);}
        public void Back(){ClosePanels();pausePanel.SetActive(true);}
        public void Menu(){Time.timeScale=1;SceneManager.LoadScene("MainMenu");}
        private void ClosePanels(){pausePanel.SetActive(false);settingsPanel.SetActive(false);tutorialPanel.SetActive(false);}
    }
}
