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
                if (enemy == null || enemy.IsDead) continue;

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
            hasHit = true;

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
            Destroy(gameObject);
        }
    }
}
