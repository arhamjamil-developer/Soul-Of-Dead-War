using System;
using UnityEngine;
using SoulCrystalDefense.Data;
using SoulCrystalDefense.Combat;
using SoulCrystalDefense.Grid;
using SoulCrystalDefense.Systems;

namespace SoulCrystalDefense.Units
{
    public class EnemyUnit : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private EnemyData enemyData;
        [SerializeField] private UnitAnimationPlayer animPlayer;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private HealthBar healthBar;

        private int laneIndex;
        private float currentHealth;
        private float attackTimer = 0f;
        private bool isDead = false;
        private bool hasBreached = false;
        private DefenderUnit currentTargetDefender = null;

        private Color originalColor = Color.white;
        private float flashTimer = 0f;

        public EnemyData Data => enemyData;
        public int LaneIndex => laneIndex;
        public float CurrentHealth => currentHealth;
        public bool IsDead => isDead;
        public bool HasBreached => hasBreached;

        public event Action<EnemyUnit> OnDeath;
        public event Action<EnemyUnit> OnBreach;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (animPlayer == null) animPlayer = GetComponentInChildren<UnitAnimationPlayer>();
            if (healthBar == null) healthBar = GetComponentInChildren<HealthBar>();

            if (spriteRenderer != null) originalColor = spriteRenderer.color;
        }

        public void Initialize(EnemyData data, int lane)
        {
            enemyData = data;
            laneIndex = lane;
            currentHealth = data.maxHealth;
            isDead = false;
            hasBreached = false;
            attackTimer = 0f;

            if (healthBar != null)
            {
                healthBar.Initialize(currentHealth, new Color(0.95f, 0.25f, 0.25f, 1f));
            }

            if (animPlayer != null)
            {
                animPlayer.PlayWalk();
            }
        }

        private void Update()
        {
            if (isDead || hasBreached) return;

            // Handle hit flash
            if (flashTimer > 0f)
            {
                flashTimer -= Time.deltaTime;
                if (flashTimer <= 0f && spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }
            }

            // Check if crystal breach reached
            float breachY = BoardGrid.Instance != null ? BoardGrid.Instance.CrystalBreachY : -4.2f;
            if (transform.position.y <= breachY)
            {
                TriggerBreach();
                return;
            }

            // Search for defender in front within attack range in the same lane
            FindTargetDefender();

            if (currentTargetDefender != null && !currentTargetDefender.IsDead)
            {
                // In combat range: stop moving and attack
                attackTimer += Time.deltaTime;
                if (attackTimer >= enemyData.attackCooldown)
                {
                    attackTimer = 0f;
                    PerformAttack();
                }
            }
            else
            {
                // Advance downward along lane
                currentTargetDefender = null;
                transform.position += Vector3.down * enemyData.moveSpeed * Time.deltaTime;
                if (animPlayer != null)
                {
                    animPlayer.PlayWalk();
                }
            }
        }

        private void FindTargetDefender()
        {
            var defenders = FindObjectsByType<DefenderUnit>(FindObjectsSortMode.None);
            DefenderUnit closest = null;
            float closestDist = float.MaxValue;

            foreach (var def in defenders)
            {
                if (def == null || def.IsDead || def.LaneIndex != laneIndex) continue;

                // Defender must be below enemy
                float dy = transform.position.y - def.transform.position.y;
                if (dy >= 0f && dy <= enemyData.attackRange)
                {
                    if (dy < closestDist)
                    {
                        closestDist = dy;
                        closest = def;
                    }
                }
            }

            currentTargetDefender = closest;
        }

        private void PerformAttack()
        {
            if (enemyData != null && enemyData.enemyType == EnemyType.SoulReaper)
            {
                SoundManager.Instance?.PlaySoulReaperCast();
            }

            if (animPlayer != null)
            {
                animPlayer.PlayAttack(() =>
                {
                    if (currentTargetDefender != null && !currentTargetDefender.IsDead)
                    {
                        currentTargetDefender.TakeDamage(enemyData.damage);
                    }
                });
            }
            else
            {
                if (currentTargetDefender != null && !currentTargetDefender.IsDead)
                {
                    currentTargetDefender.TakeDamage(enemyData.damage);
                }
            }
        }

        public void TakeDamage(float dmg)
        {
            if (isDead || hasBreached) return;

            currentHealth -= dmg;
            if (healthBar != null) healthBar.UpdateHealth(currentHealth);

            // Hit flash
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1f, 0.4f, 0.4f, 1f);
                flashTimer = 0.12f;
            }

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        public void ApplyKnockback(float distance)
        {
            if (isDead) return;
            transform.position += Vector3.up * distance;
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            SoundManager.Instance?.PlayBoneDeath();

            // Reward soul energy
            if (SoulEnergyWallet.Instance != null)
            {
                SoulEnergyWallet.Instance.AddEnergy(enemyData.soulReward);
            }

            OnDeath?.Invoke(this);

            if (animPlayer != null)
            {
                animPlayer.PlayDeath(() =>
                {
                    Destroy(gameObject, 0.5f);
                });
            }
            else
            {
                Destroy(gameObject, 0.2f);
            }
        }

        private void TriggerBreach()
        {
            if (hasBreached) return;
            hasBreached = true;

            if (SoulCrystal.Instance != null)
            {
                SoulCrystal.Instance.TakeBreachDamage(enemyData.crystalBreachDamage);
            }

            OnBreach?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
