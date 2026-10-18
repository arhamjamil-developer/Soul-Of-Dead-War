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

        [Header("Scaling & Elite State")]
        [SerializeField] private bool isEliteUnit = false;
        [SerializeField] private float hpMultiplier = 1.0f;
        [SerializeField] private float speedMultiplier = 1.0f;
        private float effectiveDamage = 9f;
        private float effectiveMoveSpeed = 0.55f;

        public EnemyData Data => enemyData;
        public int LaneIndex => laneIndex;
        public float CurrentHealth => currentHealth;
        public bool IsDead => isDead;
        public bool HasBreached => hasBreached;
        public bool IsElite => isEliteUnit;
        public float MaxHealth => (enemyData != null ? enemyData.maxHealth : 60f) * hpMultiplier * (isEliteUnit ? 2f : 1f);

        public event Action<EnemyUnit> OnDeath;
        public event Action<EnemyUnit> OnBreach;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (animPlayer == null) animPlayer = GetComponentInChildren<UnitAnimationPlayer>();
            if (healthBar == null) healthBar = GetComponentInChildren<HealthBar>();

            if (spriteRenderer != null) originalColor = spriteRenderer.color;
        }

        public void Initialize(EnemyData data, int lane, float hpMult = 1.0f, float spdMult = 1.0f, bool isElite = false)
        {
            enemyData = data;
            laneIndex = lane;
            isEliteUnit = isElite;
            hpMultiplier = Mathf.Max(0.1f, hpMult);
            speedMultiplier = Mathf.Max(0.1f, spdMult);

            float baseHp = data != null ? data.maxHealth : 60f;
            if (isEliteUnit)
            {
                baseHp *= 2.0f; // Elite has 2.0x base health
            }
            currentHealth = baseHp * hpMultiplier;

            float baseDmg = data != null ? data.damage : 9f;
            effectiveDamage = isEliteUnit ? baseDmg * 1.35f : baseDmg;

            float baseSpd = data != null ? data.moveSpeed : 0.55f;
            effectiveMoveSpeed = baseSpd * speedMultiplier;

            isDead = false;
            hasBreached = false;
            attackTimer = 0f;

            if (healthBar != null)
            {
                Color barColor = isEliteUnit
                    ? new Color(1.0f, 0.78f, 0.15f, 1f) // Gold health bar for elite
                    : new Color(0.95f, 0.25f, 0.25f, 1f); // Red health bar for standard
                healthBar.Initialize(currentHealth, barColor);
            }

            if (isEliteUnit && spriteRenderer != null)
            {
                // Distinct elite silhouette aura and scale
                transform.localScale = transform.localScale * 1.22f;
                spriteRenderer.color = new Color(1.0f, 0.88f, 0.98f, 1f);
                originalColor = spriteRenderer.color;
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
                // In combat range: stop moving and transition from Walk to Idle stance
                if (animPlayer != null && animPlayer.CurrentState == UnitAnimationPlayer.AnimState.Walk)
                {
                    animPlayer.PlayIdle();
                }

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
                transform.position += Vector3.down * effectiveMoveSpeed * Time.deltaTime;
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
                        currentTargetDefender.TakeDamage(effectiveDamage);
                    }
                });
            }
            else
            {
                if (currentTargetDefender != null && !currentTargetDefender.IsDead)
                {
                    currentTargetDefender.TakeDamage(effectiveDamage);
                }
            }
        }

        public void TakeDamage(float dmg)
        {
            if (isDead || hasBreached) return;

            currentHealth -= dmg;
            SoulCrystalDefense.Presentation.GameplayEffects.Instance?.Impact(transform.position, dmg);
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

            // Reward soul energy (with elite bonus)
            int reward = enemyData != null ? enemyData.soulReward : 15;
            if (isEliteUnit) reward = Mathf.RoundToInt(reward * 2.5f);

            if (SoulEnergyWallet.Instance != null)
            {
                SoulEnergyWallet.Instance.AddEnergy(reward);
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

            int breach = enemyData != null ? enemyData.crystalBreachDamage : 2;
            if (isEliteUnit) breach += 2;

            if (SoulCrystal.Instance != null)
            {
                SoulCrystal.Instance.TakeBreachDamage(breach);
            }

            OnBreach?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
