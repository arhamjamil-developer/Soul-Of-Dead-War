using UnityEngine;
using SoulCrystalDefense.Units;

namespace SoulCrystalDefense.Grid
{
    public class PlacementCell : MonoBehaviour
    {
        [Header("Coordinates")]
        [SerializeField] private int lane;
        [SerializeField] private int row;

        [Header("Visual Components")]
        [SerializeField] private SpriteRenderer cellRenderer;
        [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.08f);
        [SerializeField] private Color validColor = new Color(0.1f, 0.9f, 1f, 0.45f);
        [SerializeField] private Color invalidColor = new Color(1f, 0.2f, 0.2f, 0.5f);

        private DefenderUnit occupant;

        public int Lane => lane;
        public int Row => row;
        public bool IsOccupied => occupant != null;
        public DefenderUnit Occupant => occupant;

        private void Awake()
        {
            if (cellRenderer == null)
                cellRenderer = GetComponent<SpriteRenderer>();

            SetVisualState(CellVisualState.Normal);
        }

        public void Initialize(int laneIndex, int rowIndex)
        {
            lane = laneIndex;
            row = rowIndex;
            gameObject.name = $"Cell_L{lane}_R{row}";
        }

        public void SetOccupant(DefenderUnit unit)
        {
            occupant = unit;
        }

        public void ClearOccupant()
        {
            occupant = null;
        }

        public enum CellVisualState
        {
            Normal,
            ValidHighlight,
            InvalidHighlight
        }

        public void SetVisualState(CellVisualState state)
        {
            if (cellRenderer == null) return;

            switch (state)
            {
                case CellVisualState.Normal:
                    cellRenderer.color = normalColor;
                    break;
                case CellVisualState.ValidHighlight:
                    cellRenderer.color = validColor;
                    break;
                case CellVisualState.InvalidHighlight:
                    cellRenderer.color = invalidColor;
                    break;
            }
        }
    }
}
