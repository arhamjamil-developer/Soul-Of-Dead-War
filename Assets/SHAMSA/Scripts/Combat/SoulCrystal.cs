using System;
using UnityEngine;
using SoulCrystalDefense.Systems;

namespace SoulCrystalDefense.Combat
{
    public class SoulCrystal : MonoBehaviour
    {
        public static SoulCrystal Instance { get; private set; }

        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 20;
        private int currentHealth;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer crystalRenderer;
        [SerializeField] private Transform crystalVisualTransform;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color damageFlashColor = new Color(1f, 0.3f, 0.3f, 1f);

        public event Action<int, int> OnHealthChanged;
        public event Action OnCrystalDestroyed;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsDestroyed => currentHealth <= 0;

        private Vector3 initialVisualPos;
        private float shakeTimer = 0f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (crystalVisualTransform != null)
                initialVisualPos = crystalVisualTransform.localPosition;
            else
                initialVisualPos = Vector3.zero;
        }

        public void Initialize(int startingMaxHealth)
        {
            maxHealth = startingMaxHealth;
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeBreachDamage(int amount)
        {
            if (currentHealth <= 0) return;

            currentHealth = Mathf.Max(0, currentHealth - amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            SoundManager.Instance?.PlayCrystalDamage();

            StartShake();

            if (currentHealth <= 0)
            {
                OnCrystalDestroyed?.Invoke();
            }
        }

        private void StartShake()
        {
            shakeTimer = 0.25f;
            if (crystalRenderer != null)
            {
                crystalRenderer.color = damageFlashColor;
                Invoke(nameof(ResetColor), 0.15f);
            }
        }

        private void ResetColor()
        {
            if (crystalRenderer != null)
            {
                crystalRenderer.color = normalColor;
            }
        }

        private void Update()
        {
            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.deltaTime;
                if (crystalVisualTransform != null)
                {
                    float offset = Mathf.Sin(Time.time * 50f) * 0.06f;
                    crystalVisualTransform.localPosition = initialVisualPos + new Vector3(offset, 0f, 0f);
                }
            }
            else if (crystalVisualTransform != null && crystalVisualTransform.localPosition != initialVisualPos)
            {
                crystalVisualTransform.localPosition = initialVisualPos;
            }
        }
    }
}
