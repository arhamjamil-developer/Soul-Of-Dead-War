using System;
using System.Collections.Generic;
using UnityEngine;
using SoulCrystalDefense.Data;

namespace SoulCrystalDefense.Systems
{
    public enum WavePattern
    {
        Tutorial,
        Rush,
        CrossLane,
        RangedPressure,
        SplitPortals,
        GuardedBoss,
        RecoveryWave
    }

    [Serializable]
    public struct WaveProjectionData
    {
        public int waveNumber;
        public int baseThreatBudget;
        public int adjustedThreatBudget;
        public float hpMultiplier;
        public float speedMultiplier;
        public int maxSimultaneousEnemies;
        public WavePattern pattern;
        public int boneRaiderCount;
        public int soulReaperCount;
        public int eliteCount;
        public int bossCount;
        public int totalEnemies;
        public float estimatedDuration;
        public float playerEnergyPotential;
        public string archetypeDescription;
    }

    [Serializable]
    public struct PlayerWaveTelemetry
    {
        public int waveNumber;
        public int crystalHealthAtStart;
        public int crystalHealthAtEnd;
        public int maxCrystalHealth;
        public float clearDurationSeconds;
        public float expectedDurationSeconds;
        public int defendersPlaced;
        public float totalDamageDealt;
        public float totalDamageReceived;
        public int soulEnergySpent;
        public int soulEnergyEarned;
        public int leaksCount;

        public float CrystalHealthPercent => maxCrystalHealth > 0 ? (float)crystalHealthAtEnd / maxCrystalHealth : 0f;
        public float LeakedHealthPercent => maxCrystalHealth > 0 ? (float)(crystalHealthAtStart - crystalHealthAtEnd) / maxCrystalHealth : 0f;
        public float ClearSpeedScore => expectedDurationSeconds > 0 ? Mathf.Clamp01(expectedDurationSeconds / Mathf.Max(1f, clearDurationSeconds)) : 1f;
        public float PlacementEfficiency => defendersPlaced <= 0 ? 0.5f : Mathf.Clamp01(4f / Mathf.Max(2, defendersPlaced));
        public float DamageEfficiency => (totalDamageDealt + totalDamageReceived) > 0 ? Mathf.Clamp01(totalDamageDealt / (totalDamageDealt + totalDamageReceived)) : 0.8f;
        public float RecoveryScore => CrystalHealthPercent >= 0.8f ? 1.0f : (CrystalHealthPercent >= 0.4f ? 0.65f : 0.3f);
    }

    public class WaveProgressionGenerator : MonoBehaviour
    {
        public static WaveProgressionGenerator Instance { get; private set; }

        public const int COST_BONE_RAIDER = 1;
        public const int COST_SOUL_REAPER = 2;
        public const int COST_ELITE_VARIANT = 4;
        public const int COST_ELEPHANT_BOSS = 12;

        public const float MAX_HP_MULTIPLIER = 4.5f;
        public const float MAX_SPEED_MULTIPLIER = 1.6f;
        public const int MAX_ACTIVE_ENEMIES_CAP = 16;
        public const int TUTORIAL_BUDGET_FLOOR = 10;

        [Header("Seed & Determinism")]
        [SerializeField] private int initialSeed = 42;
        [SerializeField] private int currentSeed;

        [Header("Dynamic Brain Balancing")]
        [SerializeField] private float currentSkillEma = 0.65f;
        [SerializeField] private int consecutiveHighSkillWaves = 0;
        [SerializeField] private float lastAppliedBudgetMultiplier = 1.0f;
        [SerializeField] private List<PlayerWaveTelemetry> telemetryHistory = new List<PlayerWaveTelemetry>();

        public float CurrentSkillEma => currentSkillEma;
        public float LastAppliedBudgetMultiplier => lastAppliedBudgetMultiplier;
        public int InitialSeed => initialSeed;

        public event Action<int, WaveProjectionData> OnWaveGenerated;
        public event Action<float, float> OnDynamicBalanceAdjusted; // (oldEma, newEma)

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                currentSeed = initialSeed;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Threat budget formula: budget(n) = round(8 + 2.5 * pow(n, 0.62))
        /// </summary>
        public static int CalculateBaseThreatBudget(int waveNumber)
        {
            if (waveNumber <= 1) return TUTORIAL_BUDGET_FLOOR;
            return Mathf.RoundToInt(8f + 2.5f * Mathf.Pow(waveNumber, 0.62f));
        }

        /// <summary>
        /// HP scaling curve capped at 4.5x base health
        /// </summary>
        public static float CalculateHpMultiplier(int waveNumber)
        {
            if (waveNumber <= 1) return 1.0f;
            float t = Mathf.Clamp01((waveNumber - 1f) / 998f);
            return Mathf.Min(MAX_HP_MULTIPLIER, 1.0f + 3.5f * Mathf.Pow(t, 0.75f));
        }

        /// <summary>
        /// Movement speed scaling curve capped at 1.6x base speed
        /// </summary>
        public static float CalculateSpeedMultiplier(int waveNumber)
        {
            if (waveNumber <= 1) return 1.0f;
            float t = Mathf.Clamp01((waveNumber - 1f) / 998f);
            return Mathf.Min(MAX_SPEED_MULTIPLIER, 1.0f + 0.6f * Mathf.Pow(t, 0.75f));
        }

        /// <summary>
        /// Determine composition archetype based on wave index and chapter pacing
        /// </summary>
        public static WavePattern DeterminePattern(int waveNumber)
        {
            if (waveNumber == 1) return WavePattern.Tutorial;
            if (waveNumber == 20 || waveNumber == 999) return WavePattern.GuardedBoss;
            if (waveNumber % 20 == 0 && waveNumber > 20) return WavePattern.GuardedBoss;
            if (waveNumber == 6 || waveNumber == 11 || waveNumber == 16 || (waveNumber > 20 && waveNumber % 10 == 1)) return WavePattern.RecoveryWave;
            if (waveNumber == 2 || waveNumber == 14 || (waveNumber % 7 == 0)) return WavePattern.Rush;
            if (waveNumber == 3 || waveNumber == 8 || (waveNumber % 4 == 0)) return WavePattern.CrossLane;
            if (waveNumber == 4 || waveNumber == 9 || waveNumber == 12) return WavePattern.SplitPortals;
            return WavePattern.RangedPressure;
        }

        /// <summary>
        /// Dynamic Brain Balancing calculation:
        /// performance = 0.45 * crystalHealthPercent + 0.20 * clearSpeedScore + 0.15 * placementEfficiency + 0.10 * damageEfficiency + 0.10 * recoveryScore
        /// skillEma = lerp(previousSkillEma, performance, 0.18)
        /// Tuning rules:
        /// - If skill EMA > 0.82 for 2 waves: next budget increased by +3-5% (+4% default).
        /// - If skill EMA < 0.42 or leaked > 35% crystal health: next budget reduced by -5-8% (-6% default).
        /// </summary>
        public float RecordWaveResult(PlayerWaveTelemetry telemetry)
        {
            telemetryHistory.Add(telemetry);

            float performance = 0.45f * Mathf.Clamp01(telemetry.CrystalHealthPercent)
                              + 0.20f * Mathf.Clamp01(telemetry.ClearSpeedScore)
                              + 0.15f * Mathf.Clamp01(telemetry.PlacementEfficiency)
                              + 0.10f * Mathf.Clamp01(telemetry.DamageEfficiency)
                              + 0.10f * Mathf.Clamp01(telemetry.RecoveryScore);
            performance = Mathf.Clamp01(performance);

            float prevEma = currentSkillEma;
            currentSkillEma = Mathf.Lerp(currentSkillEma, performance, 0.18f);

            if (currentSkillEma > 0.82f)
            {
                consecutiveHighSkillWaves++;
            }
            else
            {
                consecutiveHighSkillWaves = 0;
            }

            float newMultiplier = 1.0f;
            if (consecutiveHighSkillWaves >= 2)
            {
                // Skill EMA is high for 2 waves: apply +4% boost
                newMultiplier = 1.04f;
            }
            else if (currentSkillEma < 0.42f || telemetry.LeakedHealthPercent > 0.35f)
            {
                // Under high pressure or heavy breach: grant -6% relief
                newMultiplier = 0.94f;
            }

            // Pacing safety bounds: max +5% increase in one wave step
            float clampedMultiplier = Mathf.Clamp(newMultiplier, 0.92f, 1.05f);
            lastAppliedBudgetMultiplier = clampedMultiplier;

            OnDynamicBalanceAdjusted?.Invoke(prevEma, currentSkillEma);
            return currentSkillEma;
        }

        public void SetSeed(int seed)
        {
            initialSeed = seed;
            currentSeed = seed;
        }

        public void ResetBalancing()
        {
            currentSkillEma = 0.65f;
            consecutiveHighSkillWaves = 0;
            lastAppliedBudgetMultiplier = 1.0f;
            telemetryHistory.Clear();
            currentSeed = initialSeed;
        }

        /// <summary>
        /// Generates an actionable runtime WaveData instance for the given wave index.
        /// Deterministic and reproducible using pseudorandom seeding.
        /// </summary>
        public WaveData GenerateWave(int waveNumber, float customBudgetMultiplier = -1f)
        {
            float budgetMult = customBudgetMultiplier > 0f ? customBudgetMultiplier : lastAppliedBudgetMultiplier;
            int baseBudget = CalculateBaseThreatBudget(waveNumber);
            int effectiveBudget = Mathf.RoundToInt(baseBudget * budgetMult);
            effectiveBudget = Mathf.Max(TUTORIAL_BUDGET_FLOOR, effectiveBudget);

            WavePattern pattern = DeterminePattern(waveNumber);
            WaveData wave = ScriptableObject.CreateInstance<WaveData>();
            wave.waveNumber = waveNumber;
            wave.waveTitle = $"Wave {waveNumber}: {pattern}";
            wave.waveDuration = 35f + Mathf.Min(25f, waveNumber * 0.8f);

            // Seed deterministic RNG for this wave
            System.Random rng = new System.Random(initialSeed * 10007 + waveNumber * 239);

            if (waveNumber == 1)
            {
                // Hand-crafted Wave 1 tutorial contract from Section 221
                wave.spawns = BuildWave1TutorialSpawns();
                return wave;
            }

            if (waveNumber == 20)
            {
                // Chapter 1 Climax Boss Wave
                wave.spawns = BuildWave20BossSpawns(effectiveBudget, rng);
                return wave;
            }

            if (waveNumber == 999)
            {
                // Epic finale scenario
                wave.spawns = BuildWave999FinaleSpawns(effectiveBudget, rng);
                return wave;
            }

            wave.spawns = BuildProceduralSpawns(waveNumber, effectiveBudget, pattern, wave.waveDuration, rng);
            return wave;
        }

        private List<EnemySpawnEntry> BuildWave1TutorialSpawns()
        {
            // Exact table from GAMEPLAY_DATA.md Section 221
            return new List<EnemySpawnEntry>
            {
                new EnemySpawnEntry { spawnTime = 3.0f,  laneIndex = 2, enemyType = EnemyType.BoneRaider, isElite = false, notes = "First threat; center-lane tutorial" },
                new EnemySpawnEntry { spawnTime = 8.0f,  laneIndex = 1, enemyType = EnemyType.BoneRaider, isElite = false, notes = "Teaches a second lane" },
                new EnemySpawnEntry { spawnTime = 12.0f, laneIndex = 3, enemyType = EnemyType.BoneRaider, isElite = false, notes = "Teaches symmetrical pressure" },
                new EnemySpawnEntry { spawnTime = 18.0f, laneIndex = 2, enemyType = EnemyType.BoneRaider, isElite = false, notes = "Tests the first placement" },
                new EnemySpawnEntry { spawnTime = 22.0f, laneIndex = 0, enemyType = EnemyType.BoneRaider, isElite = false, notes = "Introduces an outer lane" },
                new EnemySpawnEntry { spawnTime = 25.0f, laneIndex = 4, enemyType = EnemyType.BoneRaider, isElite = false, notes = "Completes outer-lane pressure" },
                new EnemySpawnEntry { spawnTime = 31.0f, laneIndex = 2, enemyType = EnemyType.SoulReaper, isElite = false, notes = "Late ranged threat; signals escalation" },
                new EnemySpawnEntry { spawnTime = 35.0f, laneIndex = 1, enemyType = EnemyType.BoneRaider, isElite = false, notes = "Small final pressure" },
                new EnemySpawnEntry { spawnTime = 35.5f, laneIndex = 3, enemyType = EnemyType.BoneRaider, isElite = false, notes = "Small final pressure" }
            };
        }

        private List<EnemySpawnEntry> BuildWave20BossSpawns(int budget, System.Random rng)
        {
            // Armored Elephant Boss (12) + 1 Elite Reaper (4) + 4 Soul Reapers (8) = 24 threat points
            var list = new List<EnemySpawnEntry>();

            // Opening skirmish to build tension
            list.Add(new EnemySpawnEntry { spawnTime = 3.0f, laneIndex = 1, enemyType = EnemyType.BoneRaider, notes = "Boss vanguard" });
            list.Add(new EnemySpawnEntry { spawnTime = 5.0f, laneIndex = 3, enemyType = EnemyType.BoneRaider, notes = "Boss vanguard" });
            list.Add(new EnemySpawnEntry { spawnTime = 9.0f, laneIndex = 2, enemyType = EnemyType.SoulReaper, notes = "Ranged scouting" });

            // Mid-wave Boss Entrance in Center Lane (Lane 2)
            list.Add(new EnemySpawnEntry { spawnTime = 16.0f, laneIndex = 2, enemyType = EnemyType.ArmoredElephant, isElite = true, notes = "Armored Elephant Boss" });

            // Flanking escorts
            list.Add(new EnemySpawnEntry { spawnTime = 18.0f, laneIndex = 1, enemyType = EnemyType.SoulReaper, notes = "Boss left flank" });
            list.Add(new EnemySpawnEntry { spawnTime = 18.5f, laneIndex = 3, enemyType = EnemyType.SoulReaper, notes = "Boss right flank" });

            // Elite Reaper behind boss
            list.Add(new EnemySpawnEntry { spawnTime = 24.0f, laneIndex = 2, enemyType = EnemyType.SoulReaper, isElite = true, notes = "Elite Reaper artillery" });

            // Outer lane pressure
            list.Add(new EnemySpawnEntry { spawnTime = 29.0f, laneIndex = 0, enemyType = EnemyType.BoneRaider, notes = "Outer split" });
            list.Add(new EnemySpawnEntry { spawnTime = 29.5f, laneIndex = 4, enemyType = EnemyType.BoneRaider, notes = "Outer split" });
            list.Add(new EnemySpawnEntry { spawnTime = 35.0f, laneIndex = 2, enemyType = EnemyType.SoulReaper, notes = "Final ranged wave" });

            return list;
        }

        private List<EnemySpawnEntry> BuildWave999FinaleSpawns(int budget, System.Random rng)
        {
            var list = new List<EnemySpawnEntry>();
            // Designed multi-phase boss finale
            // Phase 1: Royal Vanguard
            for (int i = 0; i < 5; i++)
            {
                list.Add(new EnemySpawnEntry { spawnTime = 3.0f + i * 2.5f, laneIndex = i % 5, enemyType = EnemyType.SoulReaper, isElite = (i % 2 == 0), notes = "P1 Phalanx" });
            }

            // Phase 2: Dual Elephant Behemoths
            list.Add(new EnemySpawnEntry { spawnTime = 20.0f, laneIndex = 1, enemyType = EnemyType.ArmoredElephant, isElite = true, notes = "Twin Boss Left" });
            list.Add(new EnemySpawnEntry { spawnTime = 24.0f, laneIndex = 3, enemyType = EnemyType.ArmoredElephant, isElite = true, notes = "Twin Boss Right" });

            // Phase 3: Sovereign Core
            list.Add(new EnemySpawnEntry { spawnTime = 38.0f, laneIndex = 2, enemyType = EnemyType.ArmoredElephant, isElite = true, notes = "Sovereign Emperor" });
            for (int l = 0; l < 5; l++)
            {
                list.Add(new EnemySpawnEntry { spawnTime = 45.0f + l * 1.5f, laneIndex = l, enemyType = EnemyType.SoulReaper, isElite = true, notes = "Final Honor Guard" });
            }

            return list;
        }

        private List<EnemySpawnEntry> BuildProceduralSpawns(int waveNumber, int budget, WavePattern pattern, float duration, System.Random rng)
        {
            var list = new List<EnemySpawnEntry>();
            int remainingBudget = budget;

            // Determine if wave features an elite (every 5th wave or high waves)
            bool allowElite = (waveNumber % 5 == 0) || (waveNumber >= 15 && rng.NextDouble() < 0.45);
            bool allowElephant = (waveNumber >= 20 && waveNumber % 20 == 0);

            // Allocate Elephant if boss wave
            if (allowElephant && remainingBudget >= COST_ELEPHANT_BOSS)
            {
                list.Add(new EnemySpawnEntry
                {
                    spawnTime = duration * 0.45f,
                    laneIndex = 2,
                    enemyType = EnemyType.ArmoredElephant,
                    isElite = true,
                    notes = "Wave Boss"
                });
                remainingBudget -= COST_ELEPHANT_BOSS;
            }

            // Allocate Elite if applicable
            if (allowElite && remainingBudget >= COST_ELITE_VARIANT)
            {
                EnemyType eliteType = (waveNumber >= 10 && rng.NextDouble() < 0.6) ? EnemyType.SoulReaper : EnemyType.BoneRaider;
                float eliteTime = duration * 0.55f;
                int eliteLane = rng.Next(1, 4); // lanes 1, 2, or 3
                list.Add(new EnemySpawnEntry
                {
                    spawnTime = eliteTime,
                    laneIndex = eliteLane,
                    enemyType = eliteType,
                    isElite = true,
                    notes = $"Elite {eliteType}"
                });
                remainingBudget -= COST_ELITE_VARIANT;
            }

            // Allocate Soul Reapers (introduced gradually: wave 1-5 has max 1-2, wave 6-20 scales naturally)
            int maxReapers = waveNumber < 5 ? 1 : (waveNumber < 10 ? 3 : 6);
            int reapersToSpawn = 0;
            if (waveNumber >= 6)
            {
                reapersToSpawn = Mathf.Min(maxReapers, remainingBudget / COST_SOUL_REAPER);
                // Ensure some raiders remain for shielding
                if (remainingBudget - (reapersToSpawn * COST_SOUL_REAPER) < 4)
                {
                    reapersToSpawn = Mathf.Max(1, reapersToSpawn - 2);
                }
            }
            else if (waveNumber >= 3)
            {
                reapersToSpawn = 1;
            }

            int reapersAllocated = 0;
            while (reapersAllocated < reapersToSpawn && remainingBudget >= COST_SOUL_REAPER)
            {
                reapersAllocated++;
                remainingBudget -= COST_SOUL_REAPER;
            }

            // Remaining budget goes to Bone Raiders (cost 1)
            int boneRaidersCount = remainingBudget;

            // Generate spawn timestamps and lane assignments based on archetype
            float usableDuration = duration - 8f; // leave 8s buffer at the end for combat resolution
            float timeStep = usableDuration / Mathf.Max(1, (boneRaidersCount + reapersAllocated));

            float currentTime = 3.0f; // initial 3-second breathing room

            switch (pattern)
            {
                case WavePattern.CrossLane:
                    // Alternating complementary lanes: (1, 3), (0, 4)
                    int[] crossPairs = new int[] { 1, 3, 2, 0, 4 };
                    int pairIdx = 0;
                    for (int i = 0; i < boneRaidersCount; i++)
                    {
                        int lane = crossPairs[pairIdx % crossPairs.Length];
                        pairIdx++;
                        list.Add(new EnemySpawnEntry
                        {
                            spawnTime = (float)Math.Round(currentTime, 1),
                            laneIndex = lane,
                            enemyType = EnemyType.BoneRaider,
                            notes = "Cross-lane Raider"
                        });
                        currentTime += timeStep * (0.85f + (float)rng.NextDouble() * 0.3f);
                    }
                    for (int i = 0; i < reapersAllocated; i++)
                    {
                        int reaperLane = (i % 2 == 0) ? 1 : 3;
                        list.Add(new EnemySpawnEntry
                        {
                            spawnTime = (float)Math.Round(currentTime * 0.7f + i * 4f, 1),
                            laneIndex = reaperLane,
                            enemyType = EnemyType.SoulReaper,
                            notes = "Cross-lane Reaper"
                        });
                    }
                    break;

                case WavePattern.RangedPressure:
                    // Slower blocker in front, Soul Reaper trailing 2.0s behind in the SAME lane
                    int targetLane = 2;
                    for (int i = 0; i < reapersAllocated; i++)
                    {
                        targetLane = (i % 2 == 0) ? 2 : ((i % 4 == 1) ? 1 : 3);
                        float blockerTime = 6.0f + i * 7.0f;
                        list.Add(new EnemySpawnEntry
                        {
                            spawnTime = blockerTime,
                            laneIndex = targetLane,
                            enemyType = EnemyType.BoneRaider,
                            notes = "Frontline Blocker"
                        });
                        list.Add(new EnemySpawnEntry
                        {
                            spawnTime = blockerTime + 2.0f,
                            laneIndex = targetLane,
                            enemyType = EnemyType.SoulReaper,
                            notes = "Trailing Ranged Reaper"
                        });
                        boneRaidersCount--;
                    }
                    // Remaining raiders distributed across other lanes
                    for (int i = 0; i < Mathf.Max(0, boneRaidersCount); i++)
                    {
                        int lane = rng.Next(0, 5);
                        list.Add(new EnemySpawnEntry
                        {
                            spawnTime = (float)Math.Round(4.0f + i * (usableDuration / Mathf.Max(1, boneRaidersCount)), 1),
                            laneIndex = lane,
                            enemyType = EnemyType.BoneRaider,
                            notes = "Support Raider"
                        });
                    }
                    break;

                case WavePattern.SplitPortals:
                    // Focus on only 2 or 3 active portals (e.g. lanes 0, 2, 4)
                    int[] activeLanes = new int[] { 0, 2, 4 };
                    for (int i = 0; i < boneRaidersCount; i++)
                    {
                        int lane = activeLanes[rng.Next(0, activeLanes.Length)];
                        list.Add(new EnemySpawnEntry
                        {
                            spawnTime = (float)Math.Round(currentTime, 1),
                            laneIndex = lane,
                            enemyType = EnemyType.BoneRaider,
                            notes = "Split Portal Raider"
                        });
                        currentTime += timeStep * (0.8f + (float)rng.NextDouble() * 0.4f);
                    }
                    for (int i = 0; i < reapersAllocated; i++)
                    {
                        list.Add(new EnemySpawnEntry
                        {
                            spawnTime = (float)Math.Round(12.0f + i * 6.5f, 1),
                            laneIndex = activeLanes[i % activeLanes.Length],
                            enemyType = EnemyType.SoulReaper,
                            notes = "Split Portal Reaper"
                        });
                    }
                    break;

                case WavePattern.RecoveryWave:
                    // Generous spacing, low stress, clear rewards
                    currentTime = 4.0f;
                    float recoveryStep = usableDuration / Mathf.Max(1, (boneRaidersCount + reapersAllocated));
                    for (int i = 0; i < boneRaidersCount; i++)
                    {
                        int lane = (i * 2 + 1) % 5;
                        list.Add(new EnemySpawnEntry
                        {
                            spawnTime = (float)Math.Round(currentTime, 1),
                            laneIndex = lane,
                            enemyType = EnemyType.BoneRaider,
                            notes = "Recovery Raider"
                        });
                        currentTime += recoveryStep;
                    }
                    for (int i = 0; i < reapersAllocated; i++)
                    {
                        list.Add(new EnemySpawnEntry
                        {
                            spawnTime = (float)Math.Round(currentTime * 0.8f, 1),
                            laneIndex = 2,
                            enemyType = EnemyType.SoulReaper,
                            notes = "Recovery Reaper"
                        });
                        currentTime += 4.0f;
                    }
                    break;

                default: // Rush & general
                    currentTime = 3.0f;
                    for (int i = 0; i < boneRaidersCount; i++)
                    {
                        int lane = rng.Next(0, 5);
                        list.Add(new EnemySpawnEntry
                        {
                            spawnTime = (float)Math.Round(currentTime, 1),
                            laneIndex = lane,
                            enemyType = EnemyType.BoneRaider,
                            notes = "Rush Raider"
                        });
                        currentTime += timeStep * (0.8f + (float)rng.NextDouble() * 0.4f);
                    }
                    for (int i = 0; i < reapersAllocated; i++)
                    {
                        int lane = rng.Next(1, 4);
                        list.Add(new EnemySpawnEntry
                        {
                            spawnTime = (float)Math.Round(10.0f + i * 5.5f, 1),
                            laneIndex = lane,
                            enemyType = EnemyType.SoulReaper,
                            notes = "Support Reaper"
                        });
                    }
                    break;
            }

            // Sort spawns by ascending spawnTime
            list.Sort((a, b) => a.spawnTime.CompareTo(b.spawnTime));
            return list;
        }

        /// <summary>
        /// Projects analytical wave telemetry up to wave 999 for scalability proofs.
        /// </summary>
        public static WaveProjectionData ProjectWave(int waveNumber, float budgetMultiplier = 1.0f)
        {
            int baseBudget = CalculateBaseThreatBudget(waveNumber);
            int adjBudget = Mathf.RoundToInt(baseBudget * budgetMultiplier);
            adjBudget = Mathf.Max(TUTORIAL_BUDGET_FLOOR, adjBudget);

            float hpMult = CalculateHpMultiplier(waveNumber);
            float spdMult = CalculateSpeedMultiplier(waveNumber);
            WavePattern pattern = DeterminePattern(waveNumber);

            int bossCount = (waveNumber == 20 || (waveNumber >= 20 && waveNumber % 20 == 0) || waveNumber == 999) ? 1 : 0;
            int eliteCount = (waveNumber % 5 == 0) ? 1 : (waveNumber > 20 ? Mathf.Min(6, waveNumber / 30) : 0);

            int remaining = adjBudget - (bossCount * COST_ELEPHANT_BOSS) - (eliteCount * COST_ELITE_VARIANT);
            remaining = Mathf.Max(0, remaining);

            int reapers = waveNumber < 3 ? 0 : (waveNumber < 6 ? 1 : Mathf.Min(8, remaining / 3));
            remaining -= reapers * COST_SOUL_REAPER;
            remaining = Mathf.Max(0, remaining);

            int raiders = remaining;
            int totalEnemies = raiders + reapers + eliteCount + bossCount;
            float duration = 35f + Mathf.Min(25f, waveNumber * 0.8f);

            // Total energy available: 250 starting + passive (+25 every 4s) + enemy kill rewards
            float passiveEnergy = (duration / 4.0f) * 25f;
            float killEnergy = raiders * 15f + reapers * 25f + eliteCount * 50f + bossCount * 80f;
            float totalEnergy = 250f + passiveEnergy + killEnergy;

            string desc = $"{pattern} (Budget: {adjBudget}, HP: {hpMult:F2}x, Speed: {spdMult:F2}x)";

            return new WaveProjectionData
            {
                waveNumber = waveNumber,
                baseThreatBudget = baseBudget,
                adjustedThreatBudget = adjBudget,
                hpMultiplier = hpMult,
                speedMultiplier = spdMult,
                maxSimultaneousEnemies = MAX_ACTIVE_ENEMIES_CAP,
                pattern = pattern,
                boneRaiderCount = raiders,
                soulReaperCount = reapers,
                eliteCount = eliteCount,
                bossCount = bossCount,
                totalEnemies = totalEnemies,
                estimatedDuration = duration,
                playerEnergyPotential = totalEnergy,
                archetypeDescription = desc
            };
        }
    }
}
