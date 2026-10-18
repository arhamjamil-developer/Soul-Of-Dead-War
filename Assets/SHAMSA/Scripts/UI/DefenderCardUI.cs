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
        [SerializeField] private Sprite presentationPortrait;
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
        private Vector3 restingScale;
        private Vector3 targetScale;

        private void Awake()
        {
            restingScale = transform.localScale;
            targetScale = restingScale;
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
                if (iconImage != null)
                    iconImage.sprite = presentationPortrait != null ? presentationPortrait : defenderData.cardIcon;

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
            targetScale = restingScale * (selected ? 1.035f : 1f);
            if (selectionBorder != null)
            {
                selectionBorder.gameObject.SetActive(isSelected);
            }
        }

        public void UpdateAffordability(int currentEnergy)
        {
            if (isLocked || defenderData == null) return;

            bool canAfford = currentEnergy >= defenderData.soulCost;
            if (costText != null) costText.color = canAfford ? new Color(.55f, .94f, 1f) : new Color(1f, .52f, .36f);
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

        private void Update()
        {
            if ((transform.localScale - targetScale).sqrMagnitude > .000001f)
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, 1f - Mathf.Exp(-18f * Time.unscaledDeltaTime));
        }
    }
}
