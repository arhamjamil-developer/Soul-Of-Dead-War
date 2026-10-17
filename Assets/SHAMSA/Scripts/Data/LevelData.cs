using System.Collections.Generic;
using UnityEngine;

namespace SoulCrystalDefense.Data
{
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "SoulCrystalDefense/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Level Information")]
        public string levelId = "Level01";
        public string levelTitle = "Forgotten Graveyard";
        public Sprite backgroundSprite;

        [Header("Board Setup")]
        public int laneCount = 5;
        public int rowCount = 8;

        [Header("Crystal & Economy")]
        public int maxCrystalHealth = 20;
        public int startingSoulEnergy = 250;
        public int passiveEnergyAmount = 25;
        public float passiveEnergyInterval = 4.0f;

        [Header("Roster")]
        public List<DefenderData> availableDefenders = new List<DefenderData>();

        [Header("Waves")]
        public List<WaveData> waves = new List<WaveData>();

        [Header("Victory Rewards")]
        public int rewardFragmentCount = 1;
        public int rewardCoins = 100;

        [Header("Star Criteria")]
        public int minHealthForThreeStars = 16;
        public float maxTimeForThreeStars = 55f;
        public int maxDefendersForThreeStars = 5;
    }
}
