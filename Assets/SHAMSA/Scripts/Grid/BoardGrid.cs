using UnityEngine;

namespace SoulCrystalDefense.Grid
{
    public class BoardGrid : MonoBehaviour
    {
        public static BoardGrid Instance { get; private set; }

        [Header("Grid Dimensions")]
        public const int LANE_COUNT = 5;
        public const int ROW_COUNT = 8;

        [Header("Spacing & Offsets")]
        [SerializeField] private float laneSpacing = 1.1f;
        [SerializeField] private float rowSpacing = 1.0f;
        [SerializeField] private float gridCenterY = -0.5f;
        [SerializeField] private float portalY = 4.7f;
        [SerializeField] private float spawnY = 5.2f;
        [SerializeField] private float crystalY = -4.6f;
        [SerializeField] private float crystalBreachY = -4.2f;

        [Header("Prefab")]
        [SerializeField] private GameObject cellPrefab;

        private PlacementCell[,] cells = new PlacementCell[LANE_COUNT, ROW_COUNT];

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            EnsureGridGenerated();
        }

        public void EnsureGridGenerated()
        {
            // If already child cells exist, map them
            var existingCells = GetComponentsInChildren<PlacementCell>();
            if (existingCells != null && existingCells.Length == LANE_COUNT * ROW_COUNT)
            {
                foreach (var cell in existingCells)
                {
                    cells[cell.Lane, cell.Row] = cell;
                }
                return;
            }

            // Otherwise instantiate them
            for (int l = 0; l < LANE_COUNT; l++)
            {
                for (int r = 0; r < ROW_COUNT; r++)
                {
                    if (cells[l, r] != null) continue;

                    Vector3 pos = CalculateCellPosition(l, r);
                    GameObject go;
                    if (cellPrefab != null)
                    {
                        go = Instantiate(cellPrefab, pos, Quaternion.identity, transform);
                    }
                    else
                    {
                        go = new GameObject($"Cell_L{l}_R{r}");
                        go.transform.SetParent(transform);
                        go.transform.position = pos;
                        var sr = go.AddComponent<SpriteRenderer>();
                        sr.sortingOrder = -1;
                        var box = go.AddComponent<BoxCollider2D>();
                        box.size = new Vector2(laneSpacing * 0.9f, rowSpacing * 0.9f);
                    }

                    var cell = go.GetComponent<PlacementCell>();
                    if (cell == null) cell = go.AddComponent<PlacementCell>();
                    cell.Initialize(l, r);
                    cells[l, r] = cell;
                }
            }
        }

        public Vector3 CalculateCellPosition(int lane, int row)
        {
            float x = (lane - 2) * laneSpacing;
            float y = gridCenterY + (row - 3.5f) * rowSpacing;
            return new Vector3(x, y, 0f);
        }

        public float GetLaneX(int lane)
        {
            return (lane - 2) * laneSpacing;
        }

        public Vector3 GetPortalPosition(int lane)
        {
            return new Vector3(GetLaneX(lane), portalY, 0f);
        }

        public Vector3 GetSpawnPosition(int lane)
        {
            return new Vector3(GetLaneX(lane), spawnY, 0f);
        }

        public float CrystalBreachY => crystalBreachY;
        public float CrystalY => crystalY;

        public PlacementCell GetCell(int lane, int row)
        {
            if (lane < 0 || lane >= LANE_COUNT || row < 0 || row >= ROW_COUNT) return null;
            return cells[lane, row];
        }

        public PlacementCell GetCellAtWorldPosition(Vector3 worldPos)
        {
            // Find closest cell within threshold
            float halfLane = laneSpacing * 0.5f;
            float halfRow = rowSpacing * 0.5f;

            for (int l = 0; l < LANE_COUNT; l++)
            {
                for (int r = 0; r < ROW_COUNT; r++)
                {
                    Vector3 cellPos = CalculateCellPosition(l, r);
                    if (Mathf.Abs(worldPos.x - cellPos.x) <= halfLane &&
                        Mathf.Abs(worldPos.y - cellPos.y) <= halfRow)
                    {
                        return cells[l, r];
                    }
                }
            }
            return null;
        }

        public void SetHighlightAll(bool active)
        {
            for (int l = 0; l < LANE_COUNT; l++)
            {
                for (int r = 0; r < ROW_COUNT; r++)
                {
                    var c = cells[l, r];
                    if (c == null) continue;

                    if (!active)
                    {
                        c.SetVisualState(PlacementCell.CellVisualState.Normal);
                    }
                    else
                    {
                        if (c.IsOccupied)
                            c.SetVisualState(PlacementCell.CellVisualState.InvalidHighlight);
                        else
                            c.SetVisualState(PlacementCell.CellVisualState.ValidHighlight);
                    }
                }
            }
        }
    }
}
