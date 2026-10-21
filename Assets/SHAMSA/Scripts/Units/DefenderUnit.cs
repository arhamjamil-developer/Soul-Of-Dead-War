using System;
using UnityEngine;
using SoulCrystalDefense.Data;
using SoulCrystalDefense.Combat;
using SoulCrystalDefense.Grid;
using SoulCrystalDefense.Systems;

namespace SoulCrystalDefense.Units
{
    public class DefenderUnit : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private DefenderData defenderData;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private UnitAnimationPlayer animPlayer;
        [SerializeField] private HealthBar healthBar;
        [SerializeField] private Transform attackOrigin;

        private PlacementCell cell;
        private float currentHealth;
        private float attackTimer = 0f;
        private bool isDead = false;

        private Color originalColor = Color.white;
        private float flashTimer = 0f;

        public DefenderData Data => defenderData;
        public int LaneIndex => cell != null ? cell.Lane : 0;
        public int RowIndex => cell != null ? cell.Row : 0;
        // Landing/punch visuals must not move the logical combat anchor off its grid cell.
        public Vector3 CombatPosition => cell != null ? cell.transform.position : transform.position;
        public float CurrentHealth => currentHealth;
        public bool IsDead => isDead;
        public UnitAnimationPlayer AnimPlayer => animPlayer;

        public event Action<DefenderUnit> OnDefenderDestroyed;

        [Header("Progression & Upgrades")]
        private int currentLevel = 1;
        private GameObject levelAuraObj;
        public int CurrentLevel => currentLevel;
        public int MaxLevel => 5;
        public bool CanUpgrade => currentLevel < MaxLevel && defenderData != null && defenderData.defenderType != DefenderType.ElephantPreview;
        public int UpgradeCost => defenderData != null ? Mathf.RoundToInt(defenderData.soulCost * 0.5f * currentLevel) : 75;
        public float DamageMultiplier => 1f + (currentLevel - 1) * 0.35f;
        public float AttackSpeedMultiplier => 1f + (currentLevel - 1) * 0.15f;
        public float HealthMultiplier => 1f + (currentLevel - 1) * 0.25f;

        public bool TryUpgrade()
        {
            if (!CanUpgrade || isDead) return false;
            int cost = UpgradeCost;
            if (SoulEnergyWallet.Instance == null || !SoulEnergyWallet.Instance.TrySpendEnergy(cost))
            {
                FindFirstObjectByType<SoulCrystalDefense.Presentation.BattlePresentation>()?.Show("NOT ENOUGH SOUL ENERGY", $"NEED {cost} SOUL TO UPGRADE TO LV.{currentLevel + 1}");
                return false;
            }

            currentLevel++;
            float newMax = defenderData.maxHealth * HealthMultiplier;
            currentHealth = Mathf.Min(newMax, currentHealth + (newMax - defenderData.maxHealth));
            if (healthBar != null) healthBar.Initialize(currentHealth, new Color(0.2f, 0.9f, 0.35f, 1f));

            // Visual evolution: scale up subtly and spawn glowing runic aura
            transform.localScale = transform.localScale * 1.07f;
            ApplyUpgradeVisuals();

            SoundManager.Instance?.PlayCrystalDamage();
            SoulCrystalDefense.Presentation.GameplayEffects.Instance?.Ring(transform.position, new Color(1f, 0.85f, 0.2f), 1.3f, 1.2f);
            FindFirstObjectByType<SoulCrystalDefense.Presentation.BattlePresentation>()?.Show(
                $"{defenderData.displayName.ToUpper()} LV.{currentLevel}!",
                $"+35% DAMAGE  /  +15% SPEED  /  RUNIC EVOLUTION"
            );
            return true;
        }

        private void ApplyUpgradeVisuals()
        {
            if (levelAuraObj == null)
            {
                levelAuraObj = new GameObject("RunicAuraRing");
                levelAuraObj.transform.SetParent(transform);
                levelAuraObj.transform.localPosition = new Vector3(0, -0.2f, 0.05f);
                var sr = levelAuraObj.AddComponent<SpriteRenderer>();
                sr.sprite = HealthBar.GetWhitePixelSprite();
                sr.sortingOrder = 1;
                levelAuraObj.transform.localScale = new Vector3(0.7f, 0.15f, 1f);
            }

            var auraSr = levelAuraObj.GetComponent<SpriteRenderer>();
            if (auraSr != null)
            {
                // Evolution color by level
                Color auraColor = currentLevel switch
                {
                    2 => new Color(0.2f, 0.9f, 1f, 0.65f),   // Cyan
                    3 => new Color(0.9f, 0.5f, 1f, 0.75f),   // Purple Arcane
                    4 => new Color(1f, 0.85f, 0.2f, 0.85f),  // Radiant Gold
                    5 => new Color(1f, 0.25f, 0.4f, 0.95f),  // Mythic Crimson/Prismatic
                    _ => Color.white
                };
                auraSr.color = auraColor;
            }
        }

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (animPlayer == null) animPlayer = GetComponentInChildren<UnitAnimationPlayer>();
            if (healthBar == null) healthBar = GetComponentInChildren<HealthBar>();
            if (spriteRenderer != null) originalColor = spriteRenderer.color;
        }

        public void Initialize(DefenderData data, PlacementCell placementCell)
        {
            defenderData = data;
            cell = placementCell;
            currentHealth = data.maxHealth;
            isDead = false;
            attackTimer = 0f;

            if (cell != null)
            {
                cell.SetOccupant(this);
            }

            if (healthBar != null)
            {
                healthBar.Initialize(currentHealth, new Color(0.2f, 0.8f, 0.3f, 1f));
            }

            if (animPlayer != null)
            {
                animPlayer.PlayIdle();
            }
        }

        private void Update()
        {
            if (isDead) return;

            // Handle hit flash
            if (flashTimer > 0f)
            {
                flashTimer -= Time.deltaTime;
                if (flashTimer <= 0f && spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }
            }

            attackTimer += Time.deltaTime * (cell != null ? cell.AttackSpeedMultiplier : 1f) * AttackSpeedMultiplier;
            if (attackTimer >= defenderData.attackCooldown)
            {
                if (TryAttack())
                {
                    attackTimer = 0f;
                }
            }
        }

        private bool TryAttack()
        {
            // If already performing attack animation, wait
            if (animPlayer != null && animPlayer.CurrentState == UnitAnimationPlayer.AnimState.Attack)
            {
                return false;
            }

            // Find enemies in the same lane above this defender
            var enemies = FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);
            EnemyUnit closestEnemy = null;
            float closestDist = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.IsDead || enemy.HasBreached || enemy.LaneIndex != LaneIndex) continue;

                float dy = enemy.transform.position.y - CombatPosition.y;
                if (dy >= -0.45f && dy <= defenderData.attackRange)
                {
                    if (Mathf.Abs(dy) < closestDist)
                    {
                        closestDist = Mathf.Abs(dy);
                        closestEnemy = enemy;
                    }
                }
            }

            if (closestEnemy == null) return false;

            // Perform attack
            if (defenderData.isRanged)
            {
                LaunchRangedAttack(closestEnemy);
            }
            else
            {
                PerformMeleeAttack(closestEnemy);
            }

            return true;
        }

        private void PerformMeleeAttack(EnemyUnit target)
        {
            // Visual punch/bob toward target
            StartCoroutine(PunchAnimation(0.12f));
            SoundManager.Instance?.PlayDwarfImpact();

            float effectiveDmg = defenderData.damage * DamageMultiplier;
            float effectiveKnockback = Mathf.Min(defenderData.knockback, 0.16f);

            if (defenderData.defenderType == DefenderType.ThunderGuardian)
            {
                effectiveKnockback *= 1.8f;
                TriggerChainLightning(target, 3);
                SoulCrystalDefense.Presentation.GameplayEffects.Instance?.Ring(transform.position, new Color(1f, 0.9f, 0.2f), 1.5f, 0.8f);
            }

            if (animPlayer != null && animPlayer.HasAttackFrames)
            {
                animPlayer.PlayAttack(() =>
                {
                    if (target != null && !target.IsDead)
                    {
                        target.TakeDamage(effectiveDmg);
                        if (effectiveKnockback > 0.01f)
                        {
                            target.ApplyKnockback(effectiveKnockback);
                        }
                    }
                });
            }
            else
            {
                target.TakeDamage(effectiveDmg);
                if (effectiveKnockback > 0.01f)
                {
                    target.ApplyKnockback(effectiveKnockback);
                }
            }
        }

        private System.Collections.IEnumerator PunchAnimation(float duration)
        {
            var visual=animPlayer != null && animPlayer.TargetRenderer != null ? animPlayer.TargetRenderer.transform : (spriteRenderer != null ? spriteRenderer.transform : null);
            if(visual==null || visual==transform)yield break;
            Vector3 origin = visual.localPosition;
            Vector3 target = origin + Vector3.up * 0.18f;
            float elapsed = 0f;
            while (elapsed < duration * 0.5f)
            {
                elapsed += Time.deltaTime;
                if(!visual)yield break;
                visual.localPosition = Vector3.Lerp(origin, target, elapsed / (duration * 0.5f));
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < duration * 0.5f)
            {
                elapsed += Time.deltaTime;
                if(!visual)yield break;
                visual.localPosition = Vector3.Lerp(target, origin, elapsed / (duration * 0.5f));
                yield return null;
            }
            if(visual)visual.localPosition = origin;
        }

        private void LaunchRangedAttack(EnemyUnit target)
        {
            if (animPlayer != null && animPlayer.HasAttackFrames)
            {
                animPlayer.PlayAttack(() =>
                {
                    SpawnProjectile(target);
                });
            }
            else
            {
                SpawnProjectile(target);
            }
        }

        private void TriggerChainLightning(EnemyUnit target, int maxBounces)
        {
            if (target == null || target.IsDead) return;
            Vector3 previous = transform.position;
            Vector3 chainCenter = target.transform.position;
            var victims = FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);
            EnemyUnit next = target;
            var hit = new System.Collections.Generic.HashSet<EnemyUnit>();
            for (int bounce = 0; bounce < maxBounces && next != null; bounce++)
            {
                Vector3 pos = next.transform.position;
                SoulCrystalDefense.Presentation.GameplayEffects.Instance?.Lightning(previous, pos);
                hit.Add(next);
                next.TakeDamage(defenderData.damage * DamageMultiplier * Mathf.Pow(0.75f, bounce));
                previous = pos;
                next = null;
                float distance = 1.6f;
                foreach (var enemy in victims)
                {
                    if (enemy == null || enemy.IsDead || enemy.HasBreached || hit.Contains(enemy)) continue;
                    float fromPrevious = Vector2.Distance(pos, enemy.transform.position);
                    float fromCenter = Vector2.Distance(chainCenter, enemy.transform.position);
                    float d = Mathf.Min(fromPrevious, fromCenter);
                    if (d < distance)
                    {
                        distance = d;
                        next = enemy;
                        previous = fromPrevious <= 1.6f ? pos : chainCenter;
                    }
                }
            }
        }

        private void SpawnProjectile(EnemyUnit target)
        {
            if (target == null || target.IsDead) return;

            if (defenderData.defenderType == DefenderType.BoneMage)
            {
                TriggerChainLightning(target, 3);
                return;
            }

            if (defenderData.defenderType == DefenderType.FrostWitch)
            {
                // Glacial Frost Orb
                Vector3 spawnPos = attackOrigin != null ? attackOrigin.position : transform.position + Vector3.up * 0.3f;
                var projObj = new GameObject("GlacialFrostOrb");
                projObj.transform.position = spawnPos;
                var sr = projObj.AddComponent<SpriteRenderer>();
                sr.sprite = HealthBar.GetWhitePixelSprite();
                sr.color = new Color(0.2f, 0.85f, 1f, 1f);
                projObj.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
                var proj = projObj.AddComponent<Projectile>();
                proj.Initialize(defenderData.damage * DamageMultiplier, defenderData.projectileSpeed, 0.65f, Vector3.up, target);
                proj.SetChillOnHit(0.40f, 3.5f);
                SoundManager.Instance?.PlayMagicAttack();
                return;
            }

            if (defenderData.defenderType == DefenderType.SoulArcher)
            {
                SoundManager.Instance?.PlayArcherAttack();
            }
            else if (defenderData.defenderType == DefenderType.BoneMage || defenderData.defenderType == DefenderType.FrostWitch)
            {
                SoundManager.Instance?.PlayMagicAttack();
            }
            else
            {
                SoundManager.Instance?.PlayChefFireImpact();
            }

            Vector3 projSpawnPos = attackOrigin != null ? attackOrigin.position : transform.position + Vector3.up * 0.3f;

            if (defenderData.projectilePrefab != null)
            {
                var projObj = Instantiate(defenderData.projectilePrefab, projSpawnPos, Quaternion.identity);
                var proj = projObj.GetComponent<Projectile>();
                if (proj != null)
                {
                    proj.Initialize(defenderData.damage * DamageMultiplier, defenderData.projectileSpeed, defenderData.splashRadius, Vector3.up, target);
                    if (defenderData.defenderType == DefenderType.SoulArcher) proj.SetPiercing(3);
                }
            }
            else
            {
                // Procedural projectile fallback
                var projObj = new GameObject("RangedFireball");
                projObj.transform.position = projSpawnPos;
                var sr = projObj.AddComponent<SpriteRenderer>();
                sr.sprite = HealthBar.GetWhitePixelSprite();
                sr.color = new Color(1f, 0.55f, 0.1f, 1f);
                projObj.transform.localScale = new Vector3(0.25f, 0.25f, 1f);
                var proj = projObj.AddComponent<Projectile>();
                proj.Initialize(defenderData.damage * DamageMultiplier, defenderData.projectileSpeed, defenderData.splashRadius, Vector3.up, target);
            }
        }

        public void TakeDamage(float dmg)
        {
            if (isDead) return;

            if (defenderData != null && defenderData.defenderType == DefenderType.IronPaladin)
            {
                // Heavy Armor absorbs 40% damage
                dmg *= 0.60f;
                SoulCrystalDefense.Presentation.GameplayEffects.Instance?.Ring(transform.position, new Color(0.2f, 0.6f, 1f), 0.6f, 0.4f);
            }

            currentHealth -= dmg;
            if (healthBar != null) healthBar.UpdateHealth(currentHealth);

            // Hit flash
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1f, 0.3f, 0.3f, 1f);
                flashTimer = 0.1f;
            }

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            if (cell != null)
            {
                cell.ClearOccupant();
            }

            OnDefenderDestroyed?.Invoke(this);

            if (animPlayer != null && animPlayer.HasDeathFrames)
            {
                animPlayer.PlayDeath(() => Destroy(gameObject, 0.3f));
            }
            else
            {
                Destroy(gameObject, 0.1f);
            }
        }

        private void OnDestroy()
        {
            if (cell != null && cell.Occupant == this)
            {
                cell.ClearOccupant();
            }
        }
    }
}


