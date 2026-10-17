using UnityEngine;

namespace SoulCrystalDefense.Data
{
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "SoulCrystalDefense/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("General Info")]
        public EnemyType enemyType;
        public string displayName;
        public GameObject unitPrefab;

        [Header("Combat Stats")]
        public float maxHealth = 60f;
        public float moveSpeed = 0.55f;
        public float damage = 9f;
        public float attackCooldown = 1.2f;
        public float attackRange = 0.65f;

        [Header("Breach & Economy")]
        public int crystalBreachDamage = 2;
        public int soulReward = 15;

        [Header("Ranged Options")]
        public bool isRanged = false;
        public float projectileSpeed = 6.0f;
        public GameObject projectilePrefab;
    }
}
