using System.Collections;
using System.IO;
using UnityEngine;
using SoulCrystalDefense.Data;
using SoulCrystalDefense.UI;

namespace SoulCrystalDefense.Systems
{
    public class GameplayQAAutomator : MonoBehaviour
    {
        public static GameplayQAAutomator Instance { get; private set; }

        [SerializeField] private bool autoRun = false;
        private PlacementController placementController;
        private DefenderData dwarfData;
        private DefenderData chefData;

        private void Awake()
        {
            Instance = this;
            placementController = FindFirstObjectByType<PlacementController>();
            dwarfData = Resources.Load<DefenderData>("DwarfGuardian_Data");
            if (dwarfData == null)
            {
                #if UNITY_EDITOR
                dwarfData = UnityEditor.AssetDatabase.LoadAssetAtPath<DefenderData>("Assets/SHAMSA/GameplayData/DwarfGuardian_Data.asset");
                chefData = UnityEditor.AssetDatabase.LoadAssetAtPath<DefenderData>("Assets/SHAMSA/GameplayData/BloodChef_Data.asset");
                #endif
            }
        }

        private void Start()
        {
            if (autoRun)
            {
                RunFullGameplayVerification();
            }
        }

        public void SetAutoRun(bool val)
        {
            autoRun = val;
        }

        public void RunFullGameplayVerification()
        {
            StartCoroutine(GameplayVerificationRoutine());
        }

        private IEnumerator GameplayVerificationRoutine()
        {
            Application.runInBackground = true;

            // Wait for countdown
            yield return new WaitForSeconds(1.0f);
            Capture("Assets/Screenshots/QA_01_WaveStart.png");

            yield return new WaitForSeconds(1.5f); // ~2.5s
            if (placementController != null && dwarfData != null)
            {
                // Place Dwarf Guardian in center lane (Lane 2, Row 2)
                placementController.DeployDefender(dwarfData, 2, 2);
            }

            yield return new WaitForSeconds(2.0f); // ~4.5s
            if (placementController != null && chefData != null)
            {
                // Place Blood Chef behind Dwarf (Lane 2, Row 0)
                placementController.DeployDefender(chefData, 2, 0);
            }

            yield return new WaitForSeconds(5.0f); // ~9.5s
            if (placementController != null && dwarfData != null)
            {
                // Place Dwarf Guardian in Lane 1, Row 2
                placementController.DeployDefender(dwarfData, 1, 2);
            }

            yield return new WaitForSeconds(4.0f); // ~13.5s
            if (placementController != null && dwarfData != null)
            {
                // Place Dwarf Guardian in Lane 3, Row 2
                placementController.DeployDefender(dwarfData, 3, 2);
            }

            yield return new WaitForSeconds(6.5f); // ~20.0s
            Capture("Assets/Screenshots/QA_02_MidWave_Combat.png");

            yield return new WaitForSeconds(11.5f); // ~31.5s (Soul Reaper spawns at 31s)
            Capture("Assets/Screenshots/QA_03_SoulReaper_Spawn.png");

            // Wait until victory or defeat
            while (GameManager.Instance != null && GameManager.Instance.IsGameActive)
            {
                yield return new WaitForSeconds(0.5f);
            }

            yield return new WaitForSeconds(1.0f);
            Capture("Assets/Screenshots/QA_04_Victory_Screen.png");
            Debug.Log("[QA] Gameplay verification sequence completed!");
        }

        public void CaptureDefeatScreenshot()
        {
            StartCoroutine(DefeatScreenshotRoutine());
        }

        private IEnumerator DefeatScreenshotRoutine()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerDefeat();
            }
            yield return new WaitForSeconds(0.5f);
            Capture("Assets/Screenshots/QA_05_Defeat_Screen.png");
        }

        private void Capture(string path)
        {
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log($"[QA] Captured: {path}");
        }

        public static void CaptureCustomResolution(int width, int height, string relativePath)
        {
            var cam = Camera.main;
            if (cam == null) return;

            var responsiveCam = cam.GetComponent<ResponsiveCamera>();

            RenderTexture rt = new RenderTexture(width, height, 24);
            var prevTarget = cam.targetTexture;
            cam.targetTexture = rt;

            if (responsiveCam != null)
            {
                float targetAspect = 1080f / 1920f;
                float currentAspect = (float)width / (float)height;
                if (currentAspect < targetAspect)
                {
                    cam.orthographicSize = 6.0f * (targetAspect / currentAspect);
                }
                else
                {
                    cam.orthographicSize = 6.0f;
                }
            }

            cam.Render();

            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            cam.targetTexture = prevTarget;
            RenderTexture.active = null;
            if (responsiveCam != null) responsiveCam.ApplyResponsiveSize();
            rt.Release();
            DestroyImmediate(rt);

            byte[] bytes = tex.EncodeToPNG();
            DestroyImmediate(tex);

            string fullPath = Path.Combine(Application.dataPath, "..", relativePath);
            string dir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllBytes(fullPath, bytes);
            Debug.Log($"[QA] Custom Resolution Captured: {relativePath} ({width}x{height})");
        }
    }
}
