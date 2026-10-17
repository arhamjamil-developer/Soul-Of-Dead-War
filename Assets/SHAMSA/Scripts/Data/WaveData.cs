using System.Collections.Generic;
using UnityEngine;

namespace SoulCrystalDefense.Data
{
    [CreateAssetMenu(fileName = "NewWaveData", menuName = "SoulCrystalDefense/Wave Data")]
    public class WaveData : ScriptableObject
    {
        [Header("Wave Configuration")]
        public int waveNumber = 1;
        public string waveTitle = "Wave 1";
        public float waveDuration = 45f;

        [Header("Spawns")]
        public List<EnemySpawnEntry> spawns = new List<EnemySpawnEntry>();
    }
}
