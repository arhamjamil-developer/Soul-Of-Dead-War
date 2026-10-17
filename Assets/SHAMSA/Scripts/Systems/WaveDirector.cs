using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SoulCrystalDefense.Data;
using SoulCrystalDefense.Units;
using SoulCrystalDefense.Grid;

namespace SoulCrystalDefense.Systems
{
    public class WaveDirector : MonoBehaviour
    {
        public static WaveDirector Instance { get; private set; }

        [Header("Runtime State")]
        [SerializeField] private WaveData currentWaveData;
        [SerializeField] private float waveTimer = 0f;
        [SerializeField] private bool isWaveActive = false;
        [SerializeField] private bool isCountingDown = false;
        [SerializeField] private float countdownRemaining = 2.0f;

        [Header("Enemy Prefab Lookups")]
        [SerializeField] private GameObject boneRaiderPrefab;
        [SerializeField] private GameObject soulReaperPrefab;
        [SerializeField] private EnemyData boneRaiderData;
        [SerializeField] private EnemyData soulReaperData;

        [Header("Portals")]
        [SerializeField] private PortalVisual[] portals = new PortalVisual[BoardGrid.LANE_COUNT];

        public event Action<float> OnCountdownTick;
        public event Action OnCountdownFinished;
        public event Action<int, int> OnWaveProgress; // (currentWave, totalWaves)
        public event Action<WaveData> OnWaveStarted;
        public event Action<WaveData> OnWaveCompleted;
        public event Action<EnemyUnit> OnEnemySpawned;

        public bool IsWaveActive => isWaveActive;
        public float WaveTimer => waveTimer;
        public float CountdownRemaining => countdownRemaining;

        private List<EnemySpawnEntry> pendingSpawns = new List<EnemySpawnEntry>();
        private HashSet<EnemySpawnEntry> telegraphedSpawns = new HashSet<EnemySpawnEntry>();
        private List<EnemyUnit> activeEnemies = new List<EnemyUnit>();
        private int totalSpawnsInWave = 0;
        private int completedSpawnsCount = 0;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void RegisterPortals(PortalVisual[] lanePortals)
        {
            portals = lanePortals;
        }

        public void StartWave(WaveData waveData)
        {
            currentWaveData = waveData;
            pendingSpawns = new List<EnemySpawnEntry>(waveData.spawns);
            telegraphedSpawns.Clear();
            activeEnemies.Clear();
            totalSpawnsInWave = waveData.spawns.Count;
            completedSpawnsCount = 0;
            waveTimer = 0f;

            StartCoroutine(CountdownRoutine(2.0f));
        }

        private IEnumerator CountdownRoutine(float duration)
        {
            isCountingDown = true;
            countdownRemaining = duration;

            while (countdownRemaining > 0f)
            {
                OnCountdownTick?.Invoke(countdownRemaining);
                yield return null;
                countdownRemaining -= Time.deltaTime;
            }

            countdownRemaining = 0f;
            isCountingDown = false;
            isWaveActive = true;

            SoundManager.Instance?.PlayWaveStart();

            OnCountdownFinished?.Invoke();
            OnWaveStarted?.Invoke(currentWaveData);
        }

        private void Update()
        {
            if (!isWaveActive) return;

            waveTimer += Time.deltaTime;

            // Check spawns and telegraphs
            for (int i = pendingSpawns.Count - 1; i >= 0; i--)
            {
                var entry = pendingSpawns[i];

                // Telegraph 1.0s before spawn
                if (!telegraphedSpawns.Contains(entry) && waveTimer >= entry.spawnTime - 1.0f)
                {
                    telegraphedSpawns.Add(entry);
                    if (entry.laneIndex >= 0 && entry.laneIndex < portals.Length && portals[entry.laneIndex] != null)
                    {
                        portals[entry.laneIndex].TriggerTelegraph(1.0f);
                    }
                }

                // Execute spawn
                if (waveTimer >= entry.spawnTime)
                {
                    SpawnEnemy(entry);
                    pendingSpawns.RemoveAt(i);
                }
            }

            // Check wave completion
            if (pendingSpawns.Count == 0 && activeEnemies.Count == 0 && completedSpawnsCount >= totalSpawnsInWave)
            {
                CompleteWave();
            }
        }

        private void SpawnEnemy(EnemySpawnEntry entry)
        {
            SoundManager.Instance?.PlayPortalSpawn();

            Vector3 spawnPos = BoardGrid.Instance != null 
                ? BoardGrid.Instance.GetSpawnPosition(entry.laneIndex) 
                : new Vector3((entry.laneIndex - 2) * 1.1f, 5.2f, 0f);

            GameObject prefabToUse = null;
            EnemyData dataToUse = null;

            switch (entry.enemyType)
            {
                case EnemyType.BoneRaider:
                    prefabToUse = boneRaiderPrefab;
                    dataToUse = boneRaiderData;
                    break;
                case EnemyType.SoulReaper:
                    prefabToUse = soulReaperPrefab;
                    dataToUse = soulReaperData;
                    break;
                default:
                    prefabToUse = boneRaiderPrefab;
                    dataToUse = boneRaiderData;
                    break;
            }

            GameObject enemyObj = null;
            if (prefabToUse != null)
            {
                enemyObj = Instantiate(prefabToUse, spawnPos, Quaternion.identity);
            }
            else
            {
                enemyObj = new GameObject($"Enemy_{entry.enemyType}_L{entry.laneIndex}");
                enemyObj.transform.position = spawnPos;
                var sr = enemyObj.AddComponent<SpriteRenderer>();
                sr.color = Color.white;
            }

            var enemyUnit = enemyObj.GetComponent<EnemyUnit>();
            if (enemyUnit == null) enemyUnit = enemyObj.AddComponent<EnemyUnit>();

            if (dataToUse != null)
            {
                enemyUnit.Initialize(dataToUse, entry.laneIndex);
            }

            // Portal flash
            if (entry.laneIndex >= 0 && entry.laneIndex < portals.Length && portals[entry.laneIndex] != null)
            {
                portals[entry.laneIndex].TriggerSpawnFlash();
            }

            activeEnemies.Add(enemyUnit);
            enemyUnit.OnDeath += HandleEnemyResolved;
            enemyUnit.OnBreach += HandleEnemyResolved;

            OnEnemySpawned?.Invoke(enemyUnit);
        }

        private void HandleEnemyResolved(EnemyUnit enemy)
        {
            if (activeEnemies.Contains(enemy))
            {
                activeEnemies.Remove(enemy);
                completedSpawnsCount++;
            }
        }

        private void CompleteWave()
        {
            isWaveActive = false;
            OnWaveCompleted?.Invoke(currentWaveData);
        }

        public void StopAllWaveActivity()
        {
            isWaveActive = false;
            isCountingDown = false;
            StopAllCoroutines();
        }
    }
}
