using System;
using UnityEngine;

namespace SoulCrystalDefense.Data
{
    [Serializable]
    public class EnemySpawnEntry
    {
        [Tooltip("Spawn time in seconds from the end of the initial countdown")]
        public float spawnTime;

        [Range(0, 4)]
        [Tooltip("Lane index (0 = leftmost, 4 = rightmost)")]
        public int laneIndex;

        [Tooltip("Type of enemy to spawn")]
        public EnemyType enemyType;

        [Tooltip("Whether this enemy is an elite or boss variant")]
        public bool isElite;

        [Tooltip("Design notes or role of this spawn")]
        public string notes;
    }
}
