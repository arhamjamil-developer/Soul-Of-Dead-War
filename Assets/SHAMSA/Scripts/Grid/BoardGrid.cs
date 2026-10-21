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

        [Header("Prefab & Visuals")]
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private Sprite[] stonePatches;

        private PlacementCell[,] cells = new PlacementCell[LANE_COUNT, ROW_COUNT];
        public float LaneSpacing => laneSpacing;
        public float RowSpacing => rowSpacing;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            EnsureGridGenerated();
            ApplyStonePatches();
        }

        public void ApplyStonePatches()
        {
            if (stonePatches == null || stonePatches.Length == 0) return;
            for (int l = 0; l < LANE_COUNT; l++)
            {
                for (int r = 0; r < ROW_COUNT; r++)
                {
                    var cell = cells[l, r];
                    if (cell == null) continue;
                    var art=cell.transform.Find("GroundArt");
                    var sr = art ? art.GetComponent<SpriteRenderer>() : cell.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        // Gray alternating slabs keep the battlefield readable between lane accents.
                        int index=stonePatches.Length>=6?((r%2==0)?4:(l==4?5:l)):(l+r)%stonePatches.Length;
                        sr.sprite = stonePatches[index];
                        sr.color = Color.white;
                        sr.sortingOrder = -12;
                        sr.transform.localScale=new Vector3(laneSpacing*.94f/sr.sprite.bounds.size.x,rowSpacing*.91f/sr.sprite.bounds.size.y,1);
                        if(art)cell.SetVisualState(PlacementCell.CellVisualState.Normal);
                    }
                }
            }
        }

        public void EnsureGridGenerated()
        {
            // If already child cells exist, map them
            var existingCells = GetComponentsInChildren<PlacementCell>();
            if (existingCells != null)
            {
                foreach (var cell in existingCells)
                {
                    if(cell.Lane<0||cell.Lane>=LANE_COUNT||cell.Row<0||cell.Row>=ROW_COUNT)continue;
                    if(cells[cell.Lane,cell.Row]!=null&&cells[cell.Lane,cell.Row]!=cell)continue;
                    cells[cell.Lane, cell.Row] = cell;
                    cell.transform.position=CalculateCellPosition(cell.Lane,cell.Row);
                }
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
            return transform.TransformPoint(new Vector3(x, y, 0f));
        }

        public float GetLaneX(int lane)
        {
            return transform.TransformPoint(new Vector3((lane - 2) * laneSpacing,0,0)).x;
        }

        public Vector3 GetPortalPosition(int lane)
        {
            return transform.TransformPoint(new Vector3((lane-2)*laneSpacing,portalY,0f));
        }

        public Vector3 GetSpawnPosition(int lane)
        {
            return transform.TransformPoint(new Vector3((lane-2)*laneSpacing,spawnY,0f));
        }

        public float CrystalBreachY => transform.TransformPoint(new Vector3(0,crystalBreachY,0)).y;
        public float CrystalY => transform.TransformPoint(new Vector3(0,crystalY,0)).y;

        public PlacementCell GetCell(int lane, int row)
        {
            if (lane < 0 || lane >= LANE_COUNT || row < 0 || row >= ROW_COUNT) return null;
            return cells[lane, row];
        }

        public PlacementCell GetCellAtWorldPosition(Vector3 worldPos)
        {
            var local=transform.InverseTransformPoint(worldPos);
            int lane=Mathf.FloorToInt(local.x/laneSpacing+2.5f);
            int row=Mathf.FloorToInt((local.y-gridCenterY)/rowSpacing+4f);
            return GetCell(lane,row);
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
