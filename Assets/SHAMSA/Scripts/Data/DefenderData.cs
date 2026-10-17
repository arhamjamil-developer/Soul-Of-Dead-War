using UnityEngine;

namespace SoulCrystalDefense.Data
{
    [CreateAssetMenu(fileName = "NewDefenderData", menuName = "SoulCrystalDefense/Defender Data")]
    public class DefenderData : ScriptableObject
    {
        [Header("General Info")]
        public DefenderType defenderType;
        public string displayName;
        public Sprite cardIcon;
        public GameObject unitPrefab;

        [Header("Economy")]
        public int soulCost = 100;
        public float placementCooldown = 1.0f;

        [Header("Combat Stats")]
        public float maxHealth = 220f;
        public float damage = 24f;
        public float attackCooldown = 0.9f;
        public float attackRange = 0.9f;
        public float knockback = 0.15f;

        [Header("Ranged / Splash Options")]
        public bool isRanged = false;
        public float projectileSpeed = 8.0f;
        public float splashRadius = 0.75f;
        public GameObject projectilePrefab;
    }
}
