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
        public float CurrentHealth => currentHealth;
        public bool IsDead => isDead;

        public event Action<DefenderUnit> OnDefenderDestroyed;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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

            attackTimer += Time.deltaTime;
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
            // Find enemies in the same lane above this defender
            var enemies = FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);
            EnemyUnit closestEnemy = null;
            float closestDist = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.IsDead || enemy.LaneIndex != LaneIndex) continue;

                float dy = enemy.transform.position.y - transform.position.y;
                if (dy > 0f && dy <= defenderData.attackRange)
                {
                    if (dy < closestDist)
                    {
                        closestDist = dy;
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

            target.TakeDamage(defenderData.damage);
            if (defenderData.knockback > 0.01f)
            {
                target.ApplyKnockback(defenderData.knockback);
            }
        }

        private System.Collections.IEnumerator PunchAnimation(float duration)
        {
            Vector3 origin = transform.position;
            Vector3 target = origin + Vector3.up * 0.18f;
            float elapsed = 0f;
            while (elapsed < duration * 0.5f)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(origin, target, elapsed / (duration * 0.5f));
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < duration * 0.5f)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(target, origin, elapsed / (duration * 0.5f));
                yield return null;
            }
            transform.position = origin;
        }

        private void LaunchRangedAttack(EnemyUnit target)
        {
            SoundManager.Instance?.PlayChefFireImpact();

            Vector3 spawnPos = attackOrigin != null ? attackOrigin.position : transform.position + Vector3.up * 0.3f;

            if (defenderData.projectilePrefab != null)
            {
                var projObj = Instantiate(defenderData.projectilePrefab, spawnPos, Quaternion.identity);
                var proj = projObj.GetComponent<Projectile>();
                if (proj != null)
                {
                    proj.Initialize(defenderData.damage, defenderData.projectileSpeed, defenderData.splashRadius, Vector3.up, target);
                }
            }
            else
            {
                // Procedural projectile fallback
                var projObj = new GameObject("RangedFireball");
                projObj.transform.position = spawnPos;
                var sr = projObj.AddComponent<SpriteRenderer>();
                sr.sprite = HealthBar.GetWhitePixelSprite();
                sr.color = new Color(1f, 0.55f, 0.1f, 1f);
                projObj.transform.localScale = new Vector3(0.25f, 0.25f, 1f);
                var proj = projObj.AddComponent<Projectile>();
                proj.Initialize(defenderData.damage, defenderData.projectileSpeed, defenderData.splashRadius, Vector3.up, target);
            }
        }

        public void TakeDamage(float dmg)
        {
            if (isDead) return;

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
            Destroy(gameObject, 0.1f);
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
