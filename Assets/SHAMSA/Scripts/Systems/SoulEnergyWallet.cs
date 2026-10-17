using System;
using UnityEngine;

namespace SoulCrystalDefense.Systems
{
    public class SoulEnergyWallet : MonoBehaviour
    {
        public static SoulEnergyWallet Instance { get; private set; }

        [Header("Energy Settings")]
        [SerializeField] private int currentEnergy = 250;
        [SerializeField] private int passiveAmount = 25;
        [SerializeField] private float passiveInterval = 4.0f;
        [SerializeField] private bool autoGenerate = true;

        public event Action<int> OnEnergyChanged;

        public int CurrentEnergy => currentEnergy;

        private float passiveTimer = 0f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void Initialize(int startingEnergy, int pAmount, float pInterval)
        {
            currentEnergy = startingEnergy;
            passiveAmount = pAmount;
            passiveInterval = pInterval;
            passiveTimer = 0f;
            OnEnergyChanged?.Invoke(currentEnergy);
        }

        private void Update()
        {
            if (!autoGenerate) return;

            passiveTimer += Time.deltaTime;
            if (passiveTimer >= passiveInterval)
            {
                passiveTimer -= passiveInterval;
                AddEnergy(passiveAmount);
            }
        }

        public void SetAutoGenerate(bool active)
        {
            autoGenerate = active;
        }

        public void AddEnergy(int amount)
        {
            if (amount <= 0) return;
            currentEnergy += amount;
            OnEnergyChanged?.Invoke(currentEnergy);
        }

        public bool CanAfford(int cost)
        {
            return currentEnergy >= cost;
        }

        public bool TrySpendEnergy(int cost)
        {
            if (!CanAfford(cost)) return false;

            currentEnergy -= cost;
            OnEnergyChanged?.Invoke(currentEnergy);
            return true;
        }
    }
}
