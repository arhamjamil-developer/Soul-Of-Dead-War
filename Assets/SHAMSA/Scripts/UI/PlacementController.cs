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
        public static PlacementController Instance { get; private set; }

        [Header("Card References")]
        [SerializeField] private List<DefenderCardUI> cards = new List<DefenderCardUI>();

        [Header("Preview Ghost")]
        [SerializeField] private GameObject previewObject;
        [SerializeField] private SpriteRenderer previewRenderer;

        private DefenderCardUI selectedCard = null;
        private bool isDragging = false;
        private PlacementCell lastHoveredCell = null;
        private Camera mainCamera;
        private readonly List<RaycastResult> pointerUiHits = new List<RaycastResult>();

        public bool IsDragging => isDragging;
        public DefenderCardUI SelectedCard => selectedCard;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

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
                // Preview the actual battlefield miniature, never the large card portrait.
                SpriteRenderer visual = null;
                if (card.Data.unitPrefab != null)
                {
                    var animator = card.Data.unitPrefab.GetComponentInChildren<UnitAnimationPlayer>();
                    if (animator != null) visual = animator.TargetRenderer;
                    if (visual == null)
                        foreach (var candidate in card.Data.unitPrefab.GetComponentsInChildren<SpriteRenderer>())
                            if (candidate.enabled && candidate.name != "Shadow") { visual = candidate; break; }
                }
                previewRenderer.sprite = visual != null ? visual.sprite : card.Data.cardIcon;
                var worldScale = visual != null ? visual.transform.lossyScale : Vector3.one;
                if (visual == null && previewRenderer.sprite != null)
                    worldScale = Vector3.one * (1.05f / Mathf.Max(.01f, previewRenderer.sprite.bounds.size.y));
                var parentScale = previewObject.transform.parent.lossyScale;
                previewObject.transform.localScale = new Vector3(worldScale.x / Mathf.Max(.001f, Mathf.Abs(parentScale.x)), worldScale.y / Mathf.Max(.001f, Mathf.Abs(parentScale.y)), 1);
                previewRenderer.color = new Color(0.2f, 0.9f, 1f, 0.6f);
            }
        }

        public void StartDragFromCard(DefenderCardUI card, PointerEventData eventData)
        {
            if (card == null || card.IsLocked || card.Data == null) return;
            if (SoulEnergyWallet.Instance != null && !SoulEnergyWallet.Instance.CanAfford(card.Data.soulCost))
            {
                FindFirstObjectByType<SoulCrystalDefense.Presentation.BattlePresentation>()?.Show("NOT ENOUGH SOUL ENERGY", "WAIT FOR REGENERATION OR DEFEAT ENEMIES");
                return;
            }

            isDragging = true;
            SelectCard(card);
            UpdateDragPosition(eventData.position);
        }

        public void UpdateDrag(PointerEventData eventData)
        {
            if (!isDragging || selectedCard == null) return;
            UpdateDragPosition(eventData.position);
        }

        public void EndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            isDragging = false;

            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) { CancelPlacement(); return; }

            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, 10f));
            worldPos.z = 0f;

            PlacementCell targetCell = BoardGrid.Instance != null ? BoardGrid.Instance.GetCellAtWorldPosition(worldPos) : null;

            if (targetCell != null && !targetCell.IsOccupied)
            {
                TryPlaceUnit(targetCell);
            }
            else if (targetCell != null && targetCell.IsOccupied)
            {
                // Check if dropping onto an existing unit triggers Soul Fusion!
                var existing = targetCell.Occupant;
                if (existing != null && selectedCard != null && selectedCard.Data != null)
                {
                    if (CheckFusionEligibility(existing.Data.defenderType, selectedCard.Data.defenderType))
                    {
                        TriggerSoulFusion(existing, selectedCard.Data, targetCell);
                        CancelPlacement();
                        return;
                    }
                }

                FindFirstObjectByType<SoulCrystalDefense.Presentation.BattlePresentation>()?.Show("TILE OCCUPIED", "CHOOSE AN EMPTY TILE");
                CancelPlacement();
            }
            else
            {
                CancelPlacement();
            }
        }

        private void UpdateDragPosition(Vector2 screenPos)
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null || previewObject == null || previewRenderer == null) return;

            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
            worldPos.z = 0f;

            previewObject.SetActive(true);
            previewRenderer.enabled = true;

            PlacementCell hoveredCell = BoardGrid.Instance != null ? BoardGrid.Instance.GetCellAtWorldPosition(worldPos) : null;

            if (hoveredCell != lastHoveredCell)
            {
                if (lastHoveredCell != null)
                {
                    lastHoveredCell.SetVisualState(lastHoveredCell.IsOccupied
                        ? PlacementCell.CellVisualState.InvalidHighlight
                        : PlacementCell.CellVisualState.ValidHighlight);
                }

                if (hoveredCell != null)
                {
                    if (hoveredCell.IsOccupied)
                    {
                        hoveredCell.SetVisualState(PlacementCell.CellVisualState.InvalidHighlight);
                    }
                    else
                    {
                        hoveredCell.SetVisualState(PlacementCell.CellVisualState.HoverHighlight);
                    }
                }
                lastHoveredCell = hoveredCell;
            }

            if (hoveredCell != null)
            {
                previewObject.transform.position = hoveredCell.transform.position;
                if (hoveredCell.IsOccupied)
                {
                    previewRenderer.color = new Color(1f, 0.25f, 0.25f, 0.85f);
                }
                else
                {
                    previewRenderer.color = new Color(0.2f, 1f, 0.5f, 0.9f);
                }
            }
            else
            {
                previewObject.transform.position = worldPos;
                previewRenderer.color = new Color(0.9f, 0.95f, 1f, 0.65f);
            }
        }

        public void CancelPlacement()
        {
            if (lastHoveredCell != null)
            {
                lastHoveredCell.SetVisualState(lastHoveredCell.IsOccupied
                    ? PlacementCell.CellVisualState.InvalidHighlight
                    : PlacementCell.CellVisualState.Normal);
                lastHoveredCell = null;
            }

            if (selectedCard != null)
            {
                selectedCard.SetSelected(false);
                selectedCard = null;
            }

            isDragging = false;

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
            // Cancel input handling
            if ((UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame) ||
                (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame))
            {
                CancelPlacement();
                return;
            }

            Vector2 screenPos = Vector2.zero;
            bool pointerDown = false;
            if (UnityEngine.InputSystem.Pointer.current != null)
            {
                screenPos = UnityEngine.InputSystem.Pointer.current.position.ReadValue();
                pointerDown = UnityEngine.InputSystem.Pointer.current.press.wasPressedThisFrame;
            }

            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            // Tap-to-Place workflow when not dragging
            if (selectedCard != null && !isDragging)
            {
                UpdateDragPosition(screenPos);

                if (pointerDown)
                {
                    if (EventSystem.current != null)
                    {
                        pointerUiHits.Clear();
                        EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = screenPos }, pointerUiHits);
                        if (pointerUiHits.Count > 0) return;
                    }

                    Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
                    worldPos.z = 0f;
                    PlacementCell cell = BoardGrid.Instance != null ? BoardGrid.Instance.GetCellAtWorldPosition(worldPos) : null;

                    if (cell != null && !cell.IsOccupied)
                    {
                        TryPlaceUnit(cell);
                    }
                    else if (cell != null && cell.IsOccupied)
                    {
                        var existing = cell.Occupant;
                        if (existing != null && selectedCard != null && selectedCard.Data != null)
                        {
                            if (CheckFusionEligibility(existing.Data.defenderType, selectedCard.Data.defenderType))
                            {
                                TriggerSoulFusion(existing, selectedCard.Data, cell);
                                CancelPlacement();
                                return;
                            }
                        }
                        FindFirstObjectByType<SoulCrystalDefense.Presentation.BattlePresentation>()?.Show("TILE OCCUPIED", "CHOOSE AN EMPTY TILE");
                    }
                    else
                    {
                        CancelPlacement();
                    }
                }
            }
            else if (selectedCard == null && !isDragging && pointerDown)
            {
                // In-Battle Inspection & Upgrade on Tap
                if (EventSystem.current != null)
                {
                    pointerUiHits.Clear();
                    EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = screenPos }, pointerUiHits);
                    if (pointerUiHits.Count > 0) return;
                }

                Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
                worldPos.z = 0f;
                PlacementCell cell = BoardGrid.Instance != null ? BoardGrid.Instance.GetCellAtWorldPosition(worldPos) : null;

                if (cell != null && cell.IsOccupied && cell.Occupant != null)
                {
                    var unit = cell.Occupant;
                    if (unit.CanUpgrade)
                    {
                        unit.TryUpgrade();
                    }
                    else
                    {
                        FindFirstObjectByType<SoulCrystalDefense.Presentation.BattlePresentation>()?.Show(
                            $"{unit.Data.displayName.ToUpper()} (LV. {unit.CurrentLevel} MAX)",
                            "MAX LEVEL REACHED"
                        );
                    }
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

            StartCoroutine(PlacementBounce(unitObj.transform));
            SoundManager.Instance?.PlayDefenderPlacement();
            SoulCrystalDefense.Presentation.GameplayEffects.Instance?.Ring(cell.transform.position, new Color(0.2f, 1f, 0.95f), 0.65f, 0.45f);
            SoulCrystalDefense.Presentation.GameplayEffects.Instance?.Ring(cell.transform.position, new Color(1f, 0.85f, 0.35f), 1.25f, 0.75f);
            SoulCrystalDefense.Systems.MobileSmoothPerformanceSystem.Instance?.TriggerShake(0.045f, 0.15f);

            return true;
        }

        private System.Collections.IEnumerator PlacementBounce(Transform tr)
        {
            if (tr == null) yield break;
            Vector3 originalScale = tr.localScale;
            Vector3 originalPos = tr.position;

            // Start slightly elevated and stretched
            tr.position = originalPos + Vector3.up * 0.45f;
            tr.localScale = new Vector3(originalScale.x * 0.7f, originalScale.y * 1.35f, originalScale.z);

            float elapsed = 0f;
            float duration = 0.32f;
            while (elapsed < duration && tr != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Fast drop down to ground position
                float dropT = Mathf.Clamp01(t * 2.2f);
                float dropCurve = dropT * dropT;
                tr.position = Vector3.Lerp(originalPos + Vector3.up * 0.45f, originalPos, dropCurve);

                // Dynamic Squash & Stretch
                float currentScaleY;
                float currentScaleX;
                if (t < 0.45f)
                {
                    float impactT = t / 0.45f;
                    currentScaleY = Mathf.Lerp(originalScale.y * 1.35f, originalScale.y * 0.75f, impactT);
                    currentScaleX = Mathf.Lerp(originalScale.x * 0.7f, originalScale.x * 1.3f, impactT);
                }
                else
                {
                    float settleT = (t - 0.45f) / 0.55f;
                    float bounceDamp = Mathf.Exp(-settleT * 4.5f) * Mathf.Cos(settleT * Mathf.PI * 3f);
                    currentScaleY = originalScale.y * (1f + bounceDamp * 0.25f);
                    currentScaleX = originalScale.x * (1f - bounceDamp * 0.2f);
                }

                tr.localScale = new Vector3(currentScaleX, currentScaleY, originalScale.z);
                yield return null;
            }

            if (tr != null)
            {
                tr.position = originalPos;
                tr.localScale = originalScale;
            }
        }

        private bool CheckFusionEligibility(DefenderType a, DefenderType b)
        {
            // Dwarf Guardian + Bone Mage = Thunder Guardian
            return (a == DefenderType.DwarfGuardian && b == DefenderType.BoneMage) ||
                   (a == DefenderType.BoneMage && b == DefenderType.DwarfGuardian);
        }

        public bool TriggerSoulFusion(DefenderUnit existing, DefenderData incomingData, PlacementCell cell)
        {
            if (existing == null || incomingData == null || cell == null) return false;

            int fusionCost = 100;
            if (SoulEnergyWallet.Instance != null && !SoulEnergyWallet.Instance.TrySpendEnergy(fusionCost))
            {
                FindFirstObjectByType<SoulCrystalDefense.Presentation.BattlePresentation>()?.Show("NOT ENOUGH SOUL ENERGY", "NEED 100 SOUL FOR SOUL FUSION");
                return false;
            }

            Vector3 pos = cell.transform.position;
            Destroy(existing.gameObject);

            // Create Thunder Guardian
            GameObject thunderObj = new GameObject("Defender_ThunderGuardian");
            thunderObj.transform.position = pos;
            var sr = thunderObj.AddComponent<SpriteRenderer>();
            sr.sprite = incomingData.cardIcon != null ? incomingData.cardIcon : existing.Data.cardIcon;
            sr.color = new Color(0.95f, 0.9f, 1f, 1f);

            var thunderData = ScriptableObject.CreateInstance<DefenderData>();
            thunderData.defenderType = DefenderType.ThunderGuardian;
            thunderData.displayName = "THUNDER GUARDIAN";
            thunderData.maxHealth = 480f;
            thunderData.damage = 52f;
            thunderData.attackCooldown = 0.9f;
            thunderData.attackRange = 1.2f;
            thunderData.knockback = 0.32f;
            thunderData.isRanged = false;
            thunderData.soulCost = 250;

            var defender = thunderObj.AddComponent<DefenderUnit>();
            defender.Initialize(thunderData, cell);

            StartCoroutine(PlacementBounce(thunderObj.transform));
            SoulCrystalDefense.Presentation.GameplayEffects.Instance?.Lightning(pos + Vector3.up * 4.5f, pos);
            SoulCrystalDefense.Presentation.GameplayEffects.Instance?.Ring(pos, new Color(1f, 0.9f, 0.2f), 2.2f, 1.4f);
            SoundManager.Instance?.PlayCrystalDamage();
            FindFirstObjectByType<SoulCrystalDefense.Presentation.BattlePresentation>()?.Show(
                "⚡ SOUL FUSION UNLEASHED ⚡",
                "THUNDER GUARDIAN AWAKENED"
            );
            return true;
        }
    }
}


