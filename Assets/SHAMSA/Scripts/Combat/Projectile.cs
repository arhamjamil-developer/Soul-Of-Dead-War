using UnityEngine;
using SoulCrystalDefense.Units;

namespace SoulCrystalDefense.Combat
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 8.0f;
        [SerializeField] private float damage = 18f;
        [SerializeField] private float splashRadius = 0.75f;
        [SerializeField] private bool isPlayerProjectile = true;

        private Vector3 direction = Vector3.up;
        private EnemyUnit targetEnemy;
        private bool hasHit = false;
        private int hitsRemaining = 1;
        private float chillSlow = 0f;
        private float chillDuration = 0f;
        private readonly System.Collections.Generic.HashSet<EnemyUnit> struck = new System.Collections.Generic.HashSet<EnemyUnit>();
        public event System.Action<EnemyUnit> OnHitEnemy;

        public void SetPiercing(int count) { hitsRemaining = Mathf.Max(1, count); }
        public void SetChillOnHit(float slow, float duration) { chillSlow = slow; chillDuration = duration; }

        public void Initialize(float dmg, float spd, float splash, Vector3 dir, EnemyUnit target = null)
        {
            damage = dmg;
            speed = spd;
            splashRadius = splash;
            direction = dir.normalized;
            targetEnemy = target;
        }

        private void Update()
        {
            if (hasHit) return;

            transform.position += direction * speed * Time.deltaTime;

            // Check if out of bounds
            if (transform.position.y > 6.0f || transform.position.y < -6.0f)
            {
                Destroy(gameObject);
                return;
            }

            // Target homing or lane collision
            if (isPlayerProjectile)
            {
                CheckEnemyHits();
            }
        }

        private void CheckEnemyHits()
        {
            // Find all active enemies near projectile position
            var enemies = FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.IsDead || enemy.HasBreached || struck.Contains(enemy)) continue;

                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                if (dist < 0.4f)
                {
                    OnHit(enemy);
                    return;
                }
            }
        }

        private void OnHit(EnemyUnit primaryTarget)
        {
            struck.Add(primaryTarget);
            hitsRemaining--;
            hasHit = hitsRemaining <= 0;

            if (primaryTarget != null)
            {
                if (chillSlow > 0.01f) primaryTarget.ApplyChill(chillSlow, chillDuration);
                OnHitEnemy?.Invoke(primaryTarget);
            }

            if (splashRadius > 0.05f)
            {
                // Splash damage to all enemies in radius
                var enemies = FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);
                foreach (var enemy in enemies)
                {
                    if (enemy == null || enemy.IsDead) continue;
                    float dist = Vector2.Distance(transform.position, enemy.transform.position);
                    if (dist <= splashRadius)
                    {
                        enemy.TakeDamage(damage);
                        if (chillSlow > 0.01f) enemy.ApplyChill(chillSlow, chillDuration);
                    }
                }
            }
            else
            {
                if (primaryTarget != null)
                {
                    primaryTarget.TakeDamage(damage);
                }
            }

            // Spawn hit VFX if any
            if (hasHit) Destroy(gameObject);
        }
    }
}
