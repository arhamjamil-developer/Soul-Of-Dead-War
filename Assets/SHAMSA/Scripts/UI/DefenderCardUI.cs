using System;
using UnityEngine;
using UnityEngine.UI;
using SoulCrystalDefense.Data;
using SoulCrystalDefense.Systems;

namespace SoulCrystalDefense.UI
{
    public class DefenderCardUI : MonoBehaviour
    {
        [Header("Card Configuration")]
        [SerializeField] private DefenderData defenderData;
        [SerializeField] private bool isLocked = false;
        [SerializeField] private string lockedLabel = "LOCKED";

        [Header("UI Elements")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Text nameText;
        [SerializeField] private Text costText;
        [SerializeField] private Image selectionBorder;
        [SerializeField] private GameObject lockOverlay;
        [SerializeField] private Text lockText;
        [SerializeField] private GameObject unaffordableOverlay;
        [SerializeField] private Button cardButton;

        public event Action<DefenderCardUI> OnCardClicked;

        public DefenderData Data => defenderData;
        public bool IsLocked => isLocked;
        private bool isSelected = false;

        private void Awake()
        {
            if (cardButton == null) cardButton = GetComponent<Button>();
            if (cardButton != null)
            {
                cardButton.onClick.AddListener(HandleClick);
            }
        }

        private void Start()
        {
            if (SoulEnergyWallet.Instance != null)
            {
                SoulEnergyWallet.Instance.OnEnergyChanged += UpdateAffordability;
                UpdateAffordability(SoulEnergyWallet.Instance.CurrentEnergy);
            }
            RefreshVisuals();
        }

        private void OnDestroy()
        {
            if (SoulEnergyWallet.Instance != null)
            {
                SoulEnergyWallet.Instance.OnEnergyChanged -= UpdateAffordability;
            }
        }

        public void Setup(DefenderData data, bool locked, string lockedText = "LOCKED")
        {
            defenderData = data;
            isLocked = locked;
            lockedLabel = lockedText;
            RefreshVisuals();
        }

        public void RefreshVisuals()
        {
            if (isLocked)
            {
                if (lockOverlay != null) lockOverlay.SetActive(true);
                if (lockText != null) lockText.text = lockedLabel;
                if (costText != null) costText.text = "--";
                if (unaffordableOverlay != null) unaffordableOverlay.SetActive(false);
                if (selectionBorder != null) selectionBorder.gameObject.SetActive(false);
                return;
            }

            if (lockOverlay != null) lockOverlay.SetActive(false);

            if (defenderData != null)
            {
                if (iconImage != null && defenderData.cardIcon != null)
                    iconImage.sprite = defenderData.cardIcon;

                if (nameText != null)
                    nameText.text = defenderData.displayName;

                if (costText != null)
                    costText.text = defenderData.soulCost.ToString();
            }

            SetSelected(isSelected);
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            if (selectionBorder != null)
            {
                selectionBorder.gameObject.SetActive(isSelected);
            }
        }

        public void UpdateAffordability(int currentEnergy)
        {
            if (isLocked || defenderData == null) return;

            bool canAfford = currentEnergy >= defenderData.soulCost;
            if (unaffordableOverlay != null)
            {
                unaffordableOverlay.SetActive(!canAfford);
            }
        }

        private void HandleClick()
        {
            if (isLocked) return;
            OnCardClicked?.Invoke(this);
        }
    }
}
