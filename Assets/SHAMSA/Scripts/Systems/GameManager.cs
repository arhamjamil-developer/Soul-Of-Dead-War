using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using SoulCrystalDefense.Data;
using SoulCrystalDefense.Combat;
using SoulCrystalDefense.UI;

namespace SoulCrystalDefense.Systems
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Level Configuration")]
        [SerializeField] private LevelData currentLevelData;

        [Header("Subsystems")]
        [SerializeField] private SoulEnergyWallet energyWallet;
        [SerializeField] private SoulCrystal soulCrystal;
        [SerializeField] private WaveDirector waveDirector;
        [SerializeField] private GameplayHUD gameplayHUD;
        [SerializeField] private VictoryDefeatPanel victoryDefeatPanel;

        [Header("Procedural Progression & Balancing")]
        [SerializeField] private bool useProceduralProgression = true;
        [SerializeField] private int totalWavesInRun = 20;

        [Header("Session Tracking")]
        private int defendersDeployedCount = 0;
        private float levelElapsedTime = 0f;
        private bool isGameActive = false;
        private bool isPaused = false;
        private int currentWaveIndex = 0;
        private int waveStartCrystalHealth = 20;
        private float waveStartTime = 0f;
        private int defendersAtWaveStart = 0;

        public LevelData CurrentLevelData => currentLevelData;
        public int DefendersDeployedCount => defendersDeployedCount;
        public float LevelElapsedTime => levelElapsedTime;
        public bool IsGameActive => isGameActive;
        public bool IsPaused => isPaused;
        public int CurrentWaveIndex => currentWaveIndex;
        public int TotalWavesInRun => totalWavesInRun;

        public event Action<int> OnDefendersCountChanged;

        private void Awake()
        {
            Application.runInBackground = true;
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            StartLevel(currentLevelData);
        }

        public void StartLevel(LevelData level)
        {
            currentLevelData = level;
            if (currentLevelData == null) return;

            defendersDeployedCount = 0;
            levelElapsedTime = 0f;
            isGameActive = true;
            isPaused = false;
            Time.timeScale = 1.0f;
            currentWaveIndex = 0;

            // Initialize Crystal
            if (soulCrystal != null)
            {
                soulCrystal.Initialize(currentLevelData.maxCrystalHealth);
                soulCrystal.OnCrystalDestroyed -= HandleCrystalDestroyed;
                soulCrystal.OnCrystalDestroyed += HandleCrystalDestroyed;
            }

            // Initialize Energy
            if (energyWallet != null)
            {
                energyWallet.Initialize(
                    currentLevelData.startingSoulEnergy,
                    currentLevelData.passiveEnergyAmount,
                    currentLevelData.passiveEnergyInterval
                );
            }

            // Initialize Wave Director
            if (waveDirector != null)
            {
                waveDirector.OnWaveCompleted -= HandleWaveCompleted;
                waveDirector.OnWaveCompleted += HandleWaveCompleted;

                int totalWaves = useProceduralProgression ? totalWavesInRun : currentLevelData.waves.Count;
                if (gameplayHUD != null)
                {
                    gameplayHUD.SetWaveText(1, totalWaves);
                }

                StartNextWave();
            }
        }

        private void StartNextWave()
        {
            int waveNumber = currentWaveIndex + 1;
            int totalWaves = useProceduralProgression ? totalWavesInRun : currentLevelData.waves.Count;

            waveStartCrystalHealth = soulCrystal != null ? soulCrystal.CurrentHealth : 20;
            waveStartTime = levelElapsedTime;
            defendersAtWaveStart = defendersDeployedCount;

            WaveData waveToPlay = null;
            float hpMult = 1.0f;
            float spdMult = 1.0f;

            if (useProceduralProgression)
            {
                if (WaveProgressionGenerator.Instance != null)
                {
                    waveToPlay = WaveProgressionGenerator.Instance.GenerateWave(waveNumber);
                    hpMult = WaveProgressionGenerator.CalculateHpMultiplier(waveNumber);
                    spdMult = WaveProgressionGenerator.CalculateSpeedMultiplier(waveNumber);
                }
            }
            else
            {
                if (currentWaveIndex < currentLevelData.waves.Count)
                {
                    waveToPlay = currentLevelData.waves[currentWaveIndex];
                }
            }

            if (waveToPlay != null)
            {
                if (gameplayHUD != null)
                {
                    gameplayHUD.SetWaveText(waveNumber, totalWaves);
                }
                waveDirector.StartWave(waveToPlay, hpMult, spdMult);
            }
            else
            {
                TriggerVictory();
            }
        }

        private void Update()
        {
            if (isGameActive && !isPaused)
            {
                levelElapsedTime += Time.deltaTime;
            }
        }

        public void RegisterDefenderDeployed()
        {
            defendersDeployedCount++;
            OnDefendersCountChanged?.Invoke(defendersDeployedCount);
        }

        private void HandleWaveCompleted(WaveData completedWave)
        {
            // Record telemetry for dynamic balancing
            if (useProceduralProgression && WaveProgressionGenerator.Instance != null)
            {
                int endHealth = soulCrystal != null ? soulCrystal.CurrentHealth : 0;
                int maxHp = currentLevelData != null ? currentLevelData.maxCrystalHealth : 20;
                float duration = levelElapsedTime - waveStartTime;
                int deployedInWave = defendersDeployedCount - defendersAtWaveStart;

                var telemetry = new PlayerWaveTelemetry
                {
                    waveNumber = currentWaveIndex + 1,
                    crystalHealthAtStart = waveStartCrystalHealth,
                    crystalHealthAtEnd = endHealth,
                    maxCrystalHealth = maxHp,
                    clearDurationSeconds = duration,
                    expectedDurationSeconds = completedWave != null ? completedWave.waveDuration : 45f,
                    defendersPlaced = deployedInWave,
                    totalDamageDealt = 120f,
                    totalDamageReceived = Mathf.Max(0, waveStartCrystalHealth - endHealth) * 20f,
                    leaksCount = Mathf.Max(0, waveStartCrystalHealth - endHealth)
                };

                WaveProgressionGenerator.Instance.RecordWaveResult(telemetry);
            }

            currentWaveIndex++;
            int totalWaves = useProceduralProgression ? totalWavesInRun : currentLevelData.waves.Count;

            if (currentWaveIndex < totalWaves)
            {
                StartNextWave();
            }
            else
            {
                // All waves completed! Player won!
                TriggerVictory();
            }
        }

        private void HandleCrystalDestroyed()
        {
            TriggerDefeat();
        }

        public void TriggerVictory()
        {
            if (!isGameActive) return;
            isGameActive = false;

            if (waveDirector != null)
                waveDirector.StopAllWaveActivity();

            if (energyWallet != null)
                energyWallet.SetAutoGenerate(false);

            // Calculate stars
            int stars = 1; // base 1 star for winning
            int finalCrystalHealth = soulCrystal != null ? soulCrystal.CurrentHealth : 0;

            if (finalCrystalHealth >= currentLevelData.minHealthForThreeStars)
            {
                stars++;
            }
            if (levelElapsedTime <= currentLevelData.maxTimeForThreeStars && defendersDeployedCount <= currentLevelData.maxDefendersForThreeStars)
            {
                stars++;
            }

            // Save persistent fragment reward
            int savedFragments = PlayerPrefs.GetInt("SoulCrystalFragments", 0);
            savedFragments += currentLevelData.rewardFragmentCount;
            PlayerPrefs.SetInt("SoulCrystalFragments", savedFragments);
            PlayerPrefs.Save();

            if (victoryDefeatPanel != null)
            {
                victoryDefeatPanel.ShowVictory(stars, currentLevelData.rewardFragmentCount, savedFragments);
            }

            SoundManager.Instance?.PlayVictory();
        }

        public void TriggerDefeat()
        {
            if (!isGameActive) return;
            isGameActive = false;

            if (waveDirector != null)
                waveDirector.StopAllWaveActivity();

            if (energyWallet != null)
                energyWallet.SetAutoGenerate(false);

            if (victoryDefeatPanel != null)
            {
                victoryDefeatPanel.ShowDefeat("Tip: Place Blood Chef behind Dwarf Guardian to splash grouped enemies!");
            }

            SoundManager.Instance?.PlayDefeat();
        }

        public void TogglePause()
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
        }

        public void RestartLevel()
        {
            Time.timeScale = 1.0f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
