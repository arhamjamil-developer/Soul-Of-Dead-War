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
        private float slowTimer = 0f;
        private float slowSpeedMultiplier = 1.0f;
        private Color chillColor = new Color(0.4f, 0.85f, 1f, 1f);

        public bool IsChilled => slowTimer > 0f;

        public void ApplyChill(float slowAmount, float duration)
        {
            SoulCrystalDefense.Presentation.GameplayEffects.Instance?.Frost(transform.position);
            slowSpeedMultiplier = Mathf.Clamp(1.0f - slowAmount, 0.25f, 1.0f);
            slowTimer = Mathf.Max(slowTimer, duration);
            if (spriteRenderer != null)
            {
                spriteRenderer.color = chillColor;
            }
        }

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
            laneIndex = Mathf.Clamp(lane,0,BoardGrid.LANE_COUNT-1);
            if(BoardGrid.Instance!=null)
            {
                var position=transform.position;position.x=BoardGrid.Instance.GetLaneX(laneIndex);transform.position=position;
            }
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
            float waveScale = 1.0f + (hpMultiplier - 1.0f) * 0.35f;
            effectiveDamage = (isEliteUnit ? baseDmg * 1.35f : baseDmg) * waveScale;

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
                if(data.enemyType==EnemyType.ArmoredElephant&&spriteRenderer!=null)
                {
                    spriteRenderer.color=new Color(.8f,.45f,1,1);originalColor=spriteRenderer.color;
                }
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

            // Handle Chill recovery
            float currentMoveSpeed = effectiveMoveSpeed;
            if (slowTimer > 0f)
            {
                slowTimer -= Time.deltaTime;
                currentMoveSpeed *= slowSpeedMultiplier;
                if (slowTimer <= 0f && flashTimer <= 0f && spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }
            }

            // If playing attack animation, finish it before moving
            if (animPlayer != null && animPlayer.CurrentState == UnitAnimationPlayer.AnimState.Attack)
            {
                return;
            }

            // Search for defender in front within attack range in the same lane
            var prevDefender = currentTargetDefender;
            FindTargetDefender();

            if (currentTargetDefender != null && !currentTargetDefender.IsDead)
            {
                // When acquiring a target, prime the attack timer so it strikes promptly
                if (prevDefender == null)
                {
                    float cd = enemyData != null ? enemyData.attackCooldown : 1.2f;
                    attackTimer = Mathf.Max(attackTimer, cd * 0.7f);
                }

                // In combat range: stop moving and transition from Walk to Idle stance
                if (animPlayer != null && animPlayer.CurrentState == UnitAnimationPlayer.AnimState.Walk)
                {
                    animPlayer.PlayIdle();
                }

                attackTimer += Time.deltaTime;
                float cooldown = enemyData != null ? enemyData.attackCooldown : 1.2f;
                if (attackTimer >= cooldown)
                {
                    attackTimer = 0f;
                    PerformAttack();
                }
            }
            else
            {
                // Advance downward along lane
                currentTargetDefender = null;
                transform.position += Vector3.down * currentMoveSpeed * Time.deltaTime;
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

            float baseRange = enemyData != null ? enemyData.attackRange : 1.0f;

            foreach (var def in defenders)
            {
                if (def == null || def.IsDead || def.LaneIndex != laneIndex) continue;

                // dy is enemy y minus defender y.
                // -0.45f allows targeting defenders when overlapping or slightly past the defender's center.
                float dy = transform.position.y - def.CombatPosition.y;
                float engagementRange=baseRange;
                // A melee enemy must enter the defender's reach instead of stopping outside it.
                if((enemyData==null || !enemyData.isRanged) && def.Data!=null && !def.Data.isRanged)
                    engagementRange=Mathf.Min(baseRange,Mathf.Max(.1f,def.Data.attackRange*.85f));
                float maxForwardRange=engagementRange+(currentTargetDefender == def ? .05f : 0f);
                if (dy >= -0.45f && dy <= maxForwardRange)
                {
                    float dist = Mathf.Abs(dy);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = def;
                    }
                }
            }

            currentTargetDefender = closest;
        }

        private void PerformAttack()
        {
            if (currentTargetDefender == null || currentTargetDefender.IsDead) return;

            if (enemyData != null && enemyData.enemyType == EnemyType.SoulReaper)
            {
                SoundManager.Instance?.PlaySoulReaperCast();
            }

            DefenderUnit targetToHit = currentTargetDefender;
            float damageToDeal = effectiveDamage;

            if (animPlayer != null && animPlayer.HasAttackFrames)
            {
                animPlayer.PlayAttack(() =>
                {
                    if (targetToHit != null && !targetToHit.IsDead)
                    {
                        targetToHit.TakeDamage(damageToDeal);
                    }
                });
            }
            else
            {
                targetToHit.TakeDamage(damageToDeal);
            }
        }

        public void TakeDamage(float dmg)
        {
            if (isDead || hasBreached) return;
            if(enemyData!=null&&enemyData.enemyType==EnemyType.ShieldBearer)dmg*=.65f;

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
