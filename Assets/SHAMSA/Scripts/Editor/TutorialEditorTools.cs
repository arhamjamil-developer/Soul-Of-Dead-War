using UnityEditor;
using UnityEngine;
using SoulCrystalDefense.UI;

namespace SoulCrystalDefense.EditorTools
{
    public static class TutorialEditorTools
    {
        [MenuItem("SoulCrystalDefense/Tutorial/Reset Tutorial (Show Next Run)", false, 10)]
        [MenuItem("Tools/Tutorial/Reset Tutorial (Show Next Run)", false, 10)]
        public static void ResetTutorial()
        {
            PlayerPrefs.DeleteKey(FirstWaveTutorial.SeenKey);
            PlayerPrefs.DeleteKey(FirstWaveTutorial.ReplayKey);
            PlayerPrefs.Save();
            Debug.Log("<color=green>[TutorialEditorTools] Tutorial state RESET! The tutorial will show on the next game start.</color>");
        }

        [MenuItem("SoulCrystalDefense/Tutorial/Mark Tutorial as Completed (Don't Show)", false, 11)]
        [MenuItem("Tools/Tutorial/Mark Tutorial as Completed (Don't Show)", false, 11)]
        public static void CompleteTutorial()
        {
            PlayerPrefs.SetInt(FirstWaveTutorial.SeenKey, 1);
            PlayerPrefs.Save();
            Debug.Log("<color=yellow>[TutorialEditorTools] Tutorial marked COMPLETED. The tutorial will NOT show on the next game start.</color>");
        }

        [MenuItem("SoulCrystalDefense/Tutorial/Clear All PlayerPrefs", false, 20)]
        [MenuItem("Tools/PlayerPrefs/Clear All PlayerPrefs", false, 20)]
        public static void ClearAllPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("<color=cyan>[TutorialEditorTools] ALL PlayerPrefs CLEARED! Tutorial and audio/progress settings reset to defaults.</color>");
        }

        [MenuItem("SoulCrystalDefense/Tutorial/Check Tutorial Status", false, 30)]
        public static void CheckStatus()
        {
            bool seen = PlayerPrefs.GetInt(FirstWaveTutorial.SeenKey, 0) == 1;
            bool replay = PlayerPrefs.GetInt(FirstWaveTutorial.ReplayKey, 0) == 1;
            string status = (!seen || replay) ? "WILL SHOW" : "WILL NOT SHOW (already completed)";
            Debug.Log($"<color=white>[TutorialEditorTools] Tutorial Status: Seen={seen}, Replay={replay} -> Tutorial {status} next time you play.</color>");
        }
    }
}
