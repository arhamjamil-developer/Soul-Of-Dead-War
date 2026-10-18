using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using SoulCrystalDefense.Data;
using SoulCrystalDefense.Grid;
using SoulCrystalDefense.Units;
using SoulCrystalDefense.Systems;

namespace SoulCrystalDefense.UI
{
    public class PlacementController : MonoBehaviour
    {
        [Header("Card References")]
        [SerializeField] private List<DefenderCardUI> cards = new List<DefenderCardUI>();

        [Header("Preview Ghost")]
        [SerializeField] private GameObject previewObject;
        [SerializeField] private SpriteRenderer previewRenderer;

        private DefenderCardUI selectedCard = null;
        private Camera mainCamera;
        private readonly List<RaycastResult> pointerUiHits = new List<RaycastResult>();

        private void Awake()
        {
            mainCamera = Camera.main;

            if (previewObject == null)
            {
                previewObject = new GameObject("DefenderPlacementPreview");
                previewObject.transform.SetParent(transform);
                previewRenderer = previewObject.AddComponent<SpriteRenderer>();
                previewRenderer.sortingOrder = 20;
                previewObject.SetActive(false);
            }
        }

        private void Start()
        {
            if (cards.Count == 0)
            {
                cards.AddRange(GetComponentsInChildren<DefenderCardUI>());
            }

            foreach (var card in cards)
            {
                if (card != null)
                {
                    card.OnCardClicked += HandleCardClicked;
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var card in cards)
            {
                if (card != null)
                {
                    card.OnCardClicked -= HandleCardClicked;
                }
            }
        }

        private void HandleCardClicked(DefenderCardUI card)
        {
            if (selectedCard == card)
            {
                // Deselect
                CancelPlacement();
                return;
            }

            if (card.IsLocked || card.Data == null) return;

            if (SoulEnergyWallet.Instance != null && !SoulEnergyWallet.Instance.CanAfford(card.Data.soulCost))
            {
                FindFirstObjectByType<SoulCrystalDefense.Presentation.BattlePresentation>()?.Show("NOT ENOUGH SOUL ENERGY", "WAIT FOR REGENERATION OR DEFEAT ENEMIES");
                return;
            }

            SelectCard(card);
        }

        public void SelectCard(DefenderCardUI card)
        {
            if (selectedCard != null)
            {
                selectedCard.SetSelected(false);
            }

            selectedCard = card;
            selectedCard.SetSelected(true);

            if (BoardGrid.Instance != null)
            {
                BoardGrid.Instance.SetHighlightAll(true);
            }

            // Setup preview
            if (previewObject != null && previewRenderer != null)
            {
                previewObject.SetActive(true);
                previewObject.transform.localScale = card.Data.unitPrefab != null ? card.Data.unitPrefab.transform.localScale : Vector3.one;
                if (card.Data.cardIcon != null)
                {
                    previewRenderer.sprite = card.Data.cardIcon;
                }
                previewRenderer.color = new Color(0.2f, 0.9f, 1f, 0.6f);
            }
        }

        public void CancelPlacement()
        {
            if (selectedCard != null)
            {
                selectedCard.SetSelected(false);
                selectedCard = null;
            }

            if (BoardGrid.Instance != null)
            {
                BoardGrid.Instance.SetHighlightAll(false);
            }

            if (previewObject != null)
            {
                previewObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (selectedCard == null) return;

            // Handle Cancel inputs (Escape or Right-click)
            bool cancelPressed = false;
            Vector2 screenPos = Vector2.zero;
            bool pointerDown = false;

            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                cancelPressed = true;
            }

            if (UnityEngine.InputSystem.Mouse.current != null &&
                UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame)
            {
                cancelPressed = true;
            }

            if (cancelPressed)
            {
                CancelPlacement();
                return;
            }

            if (UnityEngine.InputSystem.Pointer.current != null)
            {
                screenPos = UnityEngine.InputSystem.Pointer.current.position.ReadValue();
                pointerDown = UnityEngine.InputSystem.Pointer.current.press.wasPressedThisFrame;
            }

            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
            worldPos.z = 0f;

            PlacementCell hoveredCell = null;
            if (BoardGrid.Instance != null)
            {
                hoveredCell = BoardGrid.Instance.GetCellAtWorldPosition(worldPos);
            }

            if (hoveredCell != null)
            {
                previewObject.transform.position = hoveredCell.transform.position;
                if (hoveredCell.IsOccupied)
                {
                    previewRenderer.color = new Color(1f, 0.2f, 0.2f, 0.6f); // Red
                }
                else
                {
                    previewRenderer.color = new Color(0.2f, 0.9f, 1f, 0.7f); // Cyan
                }
            }
            else
            {
                previewObject.transform.position = worldPos;
                previewRenderer.color = new Color(1f, 1f, 1f, 0.4f);
            }

            // Handle Click / Tap
            if (previewRenderer != null) previewRenderer.enabled = hoveredCell != null;
            if (pointerDown)
            {
                // If clicked over UI card or button, let UI handle it
                if (EventSystem.current != null)
                {
                    pointerUiHits.Clear();
                    EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = screenPos }, pointerUiHits);
                    if (pointerUiHits.Count > 0) return;
                }

                if (hoveredCell != null && !hoveredCell.IsOccupied)
                {
                    TryPlaceUnit(hoveredCell);
                }
                else if (hoveredCell != null && hoveredCell.IsOccupied)
                {
                    FindFirstObjectByType<SoulCrystalDefense.Presentation.BattlePresentation>()?.Show("TILE OCCUPIED", "CHOOSE AN EMPTY TILE");
                }
                else if (hoveredCell == null)
                {
                    CancelPlacement();
                }
            }
        }

        private void TryPlaceUnit(PlacementCell cell)
        {
            if (selectedCard == null || selectedCard.Data == null) return;
            DeployDefender(selectedCard.Data, cell.Lane, cell.Row);
            CancelPlacement();
        }

        public bool DeployDefender(DefenderData data, int lane, int row)
        {
            if (data == null || (GameManager.Instance != null && (!GameManager.Instance.IsGameActive || GameManager.Instance.IsPaused))) return false;
            var cell = BoardGrid.Instance != null ? BoardGrid.Instance.GetCell(lane, row) : null;
            if (cell == null || cell.IsOccupied) return false;

            if (SoulEnergyWallet.Instance == null || !SoulEnergyWallet.Instance.TrySpendEnergy(data.soulCost))
            {
                return false;
            }

            // Spawn unit
            GameObject unitObj;
            if (data.unitPrefab != null)
            {
                unitObj = Instantiate(data.unitPrefab, cell.transform.position, Quaternion.identity);
            }
            else
            {
                unitObj = new GameObject($"Defender_{data.defenderType}_L{cell.Lane}_R{cell.Row}");
                unitObj.transform.position = cell.transform.position;
                var sr = unitObj.AddComponent<SpriteRenderer>();
                sr.sprite = data.cardIcon;
            }

            var defender = unitObj.GetComponent<DefenderUnit>();
            if (defender == null) defender = unitObj.AddComponent<DefenderUnit>();
            defender.Initialize(data, cell);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterDefenderDeployed();
            }

            SoundManager.Instance?.PlayDefenderPlacement();
            SoulCrystalDefense.Presentation.GameplayEffects.Instance?.Ring(cell.transform.position, new Color(.4f,1,1), .7f, .6f);

            return true;
        }
    }
}
