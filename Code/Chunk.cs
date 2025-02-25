using System;
using System.Threading;
using System.Threading.Tasks;
using Sandbox;

public sealed class Chunk : Component
{
    [Property] public int X { get; set; }
    [Property] public int Z { get; set; }
    // 16x16 grid of cells
    [Property] public List<GameObject> Cells { get; set; } = new List<GameObject>();
    [Property] public GameObject prefab_cell { get; set; }

    public static Chunk Instance {
        get
        {
            if ( !_instance.IsValid() )
            {
                _instance = Game.ActiveScene.GetAllComponents<Chunk>().FirstOrDefault();
            }
            return _instance;
        }
    }
    private static Chunk _instance { get; set; } = null;

    protected override void OnStart()
    {
        base.OnStart();

        GenerateCells();
        CalculateMinesAroundCells();
    }

    private void GenerateCells()
    {
        var random = new Random();
        for (int z = 0; z < 16; z++)
        {
            for (int x = 0; x < 16; x++)
            {
                var obj = prefab_cell.Clone();
                obj.Name = $"Cell-{X},{Z}/{x},{z}";
                obj.LocalPosition = new Vector3((x * 50) - (8 * 50), 0, (z * 50) - (8 * 50));
                obj.Parent = GameObject;
                Cells.Add(obj);
                var cell = obj.GetComponentInChildren<Cell>();
                if (cell == null)
                {
                    Log.Error("Failed to get cell component");
                    return;
                }
                if (random.Next(0, 100) < 20)
                {
                    cell.Type = CellType.Mine;
                    continue;
                }
                cell.Type = CellType.Number;
                cell.Number = -1;
                cell.X = x;
                cell.Z = z;
            }
        }
        UpdateChunkBorders();
    }

    private void CalculateMinesAroundCells()
    {
        foreach (var cell in Cells)
        {
            var cell_comp = cell.GetComponentInChildren<Cell>();
            if (cell_comp == null)
            {
                Log.Error("Failed to get cell component");
                return;
            }
            if (cell_comp.Type == CellType.Mine)
            {
                continue;
            }
            var nearby = Cells.Where(c => c.LocalPosition.Distance(cell.LocalPosition) <= 75 && c.LocalPosition.Distance(cell.LocalPosition) > 0);
            cell_comp.Number = nearby.Count(c => c.GetComponentInChildren<Cell>().Type == CellType.Mine);
            if (cell_comp.Number == 0)
            {
                cell_comp.Type = CellType.Empty;
            }
        }
    }

    private void UpdateChunkBorders()
    {
        var nearby_chunks = new List<Chunk>
		{
			ChunkRenderer.GetChunk( X - 1, Z ),
			ChunkRenderer.GetChunk( X + 1, Z ),
			ChunkRenderer.GetChunk( X, Z - 1 ),
			ChunkRenderer.GetChunk( X, Z + 1 ),
			ChunkRenderer.GetChunk( X + 1, Z + 1 ),
			ChunkRenderer.GetChunk( X - 1, Z - 1 ),
			ChunkRenderer.GetChunk( X + 1, Z - 1 ),
			ChunkRenderer.GetChunk( X - 1, Z + 1 )
		};
        foreach (var chunk in nearby_chunks)
        {
            if (chunk != null)
            {
                chunk.UpdateChunkBorder();
            }
        }
    }

    private void UpdateChunkBorder()
    {
        var border_cells = Cells.Where(c => {
            var cell = c.GetComponentInChildren<Cell>();
            return cell != null && (cell.X == 0 || cell.X == 15 || cell.Z == 0 || cell.Z == 15);
        });
        foreach (var cell in border_cells)
        {
            var cell_comp = cell.GetComponentInChildren<Cell>();
            if (cell_comp == null || cell_comp.Type == CellType.Mine)
            {
                continue;
            }
            var nearby = Game.ActiveScene.GetAllComponents<Cell>().Where(c => {
                return c.GameObject.WorldPosition.Distance(cell.WorldPosition) <= 75 && c.GameObject.WorldPosition.Distance(cell.WorldPosition) > 1;
            });
            cell_comp.Number = nearby.Count(c => c.Type == CellType.Mine);
            cell_comp.Type = (cell_comp.Number == 0) ? CellType.Empty : CellType.Number;
        }
    }

    public Cell GetCell(Vector3 position)
    {
        var localx = position.x - GameObject.LocalPosition.x;
        var localz = position.z - GameObject.LocalPosition.z;
        var x = (int)Math.Floor((localx + 25) / 50);
        var z = (int)Math.Floor((localz + 25) / 50);
        var cell = Cells.FirstOrDefault(c => c.LocalPosition.x == x * 50 && c.LocalPosition.z == z * 50);
        if (cell == null)
        {
            Log.Error("Failed to get cell at position");
            return null;
        }
        return cell.GetComponentInChildren<Cell>();
    }

    public Cell GetCellFromGrid(int x, int z)
    {
        var cell = Cells.FirstOrDefault(c => {
            var cell_comp = c.GetComponentInChildren<Cell>();
            if (cell_comp == null)
            {
                Log.Error("Failed to get cell component");
                return false;
            }
            return cell_comp.X == x && cell_comp.Z == z;
        });
        if (cell == null)
        {
            Log.Error("Failed to get cell at position");
            return null;
        }
        return cell.GetComponentInChildren<Cell>();
    }

    public void Update()
    {
        var camera = Scene.Camera;
        if (camera.WorldPosition.Distance(GameObject.WorldPosition) < 1500)
        {
            GameObject.Enabled = true;
        }
        else
        {
            GameObject.Enabled = false;
        }
    }
}
