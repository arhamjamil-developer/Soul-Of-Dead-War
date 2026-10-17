using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using SoulCrystalDefense.Data;
using SoulCrystalDefense.Combat;
using SoulCrystalDefense.Units;
using SoulCrystalDefense.UI;
using SoulCrystalDefense.Grid;

namespace SoulCrystalDefense.Systems
{
    public class GameplayQAAutomator : MonoBehaviour
    {
        public static GameplayQAAutomator Instance { get; private set; }

        [Header("QA Settings")]
        [SerializeField] private bool autoRun = false;
        [SerializeField] private float testTimeScale = 2.5f;
        [SerializeField] private int maxWavesToTest = 20;

        private PlacementController placementController;
        private DefenderData dwarfData;
        private DefenderData chefData;

        public struct QAWaveLog
        {
            public int waveIndex;
            public bool passed;
            public int startingCrystalHp;
            public int finalCrystalHp;
            public float duration;
            public int defendersPlaced;
            public float skillEma;
            public string notes;
        }

        private List<QAWaveLog> recordedLogs = new List<QAWaveLog>();
        public List<QAWaveLog> RecordedLogs => recordedLogs;

        private void Awake()
        {
            Instance = this;
            placementController = FindFirstObjectByType<PlacementController>();

            #if UNITY_EDITOR
            if (dwarfData == null)
                dwarfData = UnityEditor.AssetDatabase.LoadAssetAtPath<DefenderData>("Assets/SHAMSA/GameplayData/DwarfGuardian_Data.asset");
            if (chefData == null)
                chefData = UnityEditor.AssetDatabase.LoadAssetAtPath<DefenderData>("Assets/SHAMSA/GameplayData/BloodChef_Data.asset");
            #endif
        }

        private void Start()
        {
            if (autoRun)
            {
                RunAutomatedCampaignTest(maxWavesToTest, testTimeScale);
            }
        }

        public void SetAutoRun(bool val)
        {
            autoRun = val;
        }

        public void RunAutomatedCampaignTest(int totalWaves = 5, float playSpeed = 2.5f)
        {
            StartCoroutine(CampaignTestRoutine(totalWaves, playSpeed));
        }

        private IEnumerator CampaignTestRoutine(int totalWaves, float playSpeed)
        {
            Application.runInBackground = true;
            Time.timeScale = playSpeed;
            recordedLogs.Clear();

            Debug.Log($"[QA] Starting Automated Campaign Test for {totalWaves} waves at {playSpeed}x speed.");

            int currentTestedWave = 0;

            while (GameManager.Instance != null && GameManager.Instance.IsGameActive && currentTestedWave < totalWaves)
            {
                int waveNum = GameManager.Instance.CurrentWaveIndex + 1;
                int startHp = SoulCrystal.Instance != null ? SoulCrystal.Instance.CurrentHealth : 20;
                float waveStartRealTime = Time.realtimeSinceStartup;
                int startDefenders = GameManager.Instance.DefendersDeployedCount;

                Debug.Log($"[QA] >>> Testing Wave {waveNum}/{totalWaves} <<<");

                // Execute reactive AI defender placement throughout wave
                float waveTimeout = 60f;
                float elapsed = 0f;

                while (WaveDirector.Instance != null && (WaveDirector.Instance.IsWaveActive || WaveDirector.Instance.CountdownRemaining > 0f) && GameManager.Instance.IsGameActive)
                {
                    PerformTacticalDefenderPlacement();
                    yield return new WaitForSeconds(0.4f);
                    elapsed += 0.4f * Time.timeScale;

                    // Periodic screenshot captures at key narrative beats
                    if (waveNum == 1 && elapsed >= 15f && elapsed <= 17f)
                    {
                        Capture("Assets/Screenshots/QA_Wave01_MidCombat.png");
                    }
                    else if (waveNum == 5 && elapsed >= 18f && elapsed <= 20f)
                    {
                        Capture("Assets/Screenshots/QA_Wave05_ElitePressure.png");
                    }
                    else if (waveNum == 20 && elapsed >= 20f && elapsed <= 22f)
                    {
                        Capture("Assets/Screenshots/QA_Wave20_BossArrival.png");
                    }

                    if (elapsed > waveTimeout) break;
                }

                // Wave transition beat
                int endHp = SoulCrystal.Instance != null ? SoulCrystal.Instance.CurrentHealth : 0;
                bool passed = endHp > 0;
                float waveDuration = Time.realtimeSinceStartup - waveStartRealTime;
                int defendersUsed = GameManager.Instance.DefendersDeployedCount - startDefenders;
                float currentEma = WaveProgressionGenerator.Instance != null ? WaveProgressionGenerator.Instance.CurrentSkillEma : 0.7f;

                recordedLogs.Add(new QAWaveLog
                {
                    waveIndex = waveNum,
                    passed = passed,
                    startingCrystalHp = startHp,
                    finalCrystalHp = endHp,
                    duration = waveDuration,
                    defendersPlaced = defendersUsed,
                    skillEma = currentEma,
                    notes = passed ? $"Victory (Health: {endHp}/20)" : "Breached"
                });

                Debug.Log($"[QA] Wave {waveNum} Finished | Result: {(passed ? "PASS" : "FAIL")} | End HP: {endHp}/20 | EMA: {currentEma:F2}");

                currentTestedWave++;
                yield return new WaitForSeconds(1.0f);
            }

            Time.timeScale = 1.0f;
            ExportQAReport();
            Debug.Log("[QA] Automated Campaign Test Complete!");
        }

        /// <summary>
        /// Tactical placement AI: evaluates enemy lanes and places Dwarf blocker (Row 2) and Chef splash (Row 0)
        /// </summary>
        private void PerformTacticalDefenderPlacement()
        {
            if (placementController == null || SoulEnergyWallet.Instance == null) return;

            var activeEnemies = FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);
            HashSet<int> threatenedLanes = new HashSet<int>();

            foreach (var enemy in activeEnemies)
            {
                if (enemy != null && !enemy.IsDead && !enemy.HasBreached)
                {
                    threatenedLanes.Add(enemy.LaneIndex);
                }
            }

            // Always prioritize defending center lanes (2, 1, 3, 0, 4)
            int[] priorityLanes = new int[] { 2, 1, 3, 0, 4 };
            foreach (int lane in priorityLanes)
            {
                if (threatenedLanes.Contains(lane) || activeEnemies.Length == 0)
                {
                    // Check if frontline Dwarf exists
                    var dwarfCell = BoardGrid.Instance != null ? BoardGrid.Instance.GetCell(lane, 2) : null;
                    if (dwarfCell != null && !dwarfCell.IsOccupied && dwarfData != null)
                    {
                        if (SoulEnergyWallet.Instance.CanAfford(dwarfData.soulCost))
                        {
                            placementController.DeployDefender(dwarfData, lane, 2);
                            return; // One deployment per tick
                        }
                    }

                    // Check if backline Chef exists
                    var chefCell = BoardGrid.Instance != null ? BoardGrid.Instance.GetCell(lane, 0) : null;
                    if (chefCell != null && !chefCell.IsOccupied && chefData != null)
                    {
                        if (SoulEnergyWallet.Instance.CanAfford(chefData.soulCost))
                        {
                            placementController.DeployDefender(chefData, lane, 0);
                            return; // One deployment per tick
                        }
                    }
                }
            }
        }

        public void ExportQAReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== SOUL CRYSTAL DEFENSE - AUTOMATED QA TEST REPORT ===");
            sb.AppendLine($"Timestamp: {System.DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine($"Total Waves Simulated: {recordedLogs.Count}");
            sb.AppendLine();
            sb.AppendLine("| Wave | Outcome | Start HP | End HP | Defenders | Skill EMA | Notes |");
            sb.AppendLine("|---:|---|---:|---:|---:|---:|---|");

            int passCount = 0;
            foreach (var log in recordedLogs)
            {
                if (log.passed) passCount++;
                sb.AppendLine($"| {log.waveIndex} | {(log.passed ? "PASS" : "FAIL")} | {log.startingCrystalHp} | {log.finalCrystalHp} | {log.defendersPlaced} | {log.skillEma:F2} | {log.notes} |");
            }

            float passRate = recordedLogs.Count > 0 ? (float)passCount / recordedLogs.Count * 100f : 0f;
            sb.AppendLine();
            sb.AppendLine($"Overall Pass Rate: {passRate:F1}% ({passCount}/{recordedLogs.Count})");

            string reportDir = Path.Combine(Application.dataPath, "..", "Logs");
            if (!Directory.Exists(reportDir)) Directory.CreateDirectory(reportDir);
            string reportPath = Path.Combine(reportDir, "WaveQA_Report.txt");
            File.WriteAllText(reportPath, sb.ToString());
            Debug.Log($"[QA] Report exported to: {reportPath}");
        }

        public void Capture(string path)
        {
            string fullPath = Path.Combine(Application.dataPath, "..", path);
            string dir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log($"[QA] Screenshot captured: {path}");
        }
    }
}
