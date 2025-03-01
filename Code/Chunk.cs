using System;
using System.Threading;
using System.Threading.Tasks;
using Sandbox;

public sealed class Chunk : Component
{
    [Property] public int X { get; set; }
    [Property] public int Z { get; set; }
    // 16x16 grid of cells
    [Property] public List<Cell> Cells { get; set; } = new List<Cell>();

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

        var objs = GameObject.GetComponentsInChildren<Cell>().ToList();
        if (objs.Count == 0)
        {
            Log.Error("Failed to get cell objects");
            return;
        }
        foreach (var obj in objs)
        {
            Cells.Add(obj);
            if (random.Next(0, 100) < 20)
            {
                obj.Type = CellType.Mine;
            }
        }
        if (Cells.Count == 0)
        {
            Log.Error("Failed to generate cells");
            return;
        }
    }

    private void CalculateMinesAroundCells()
    {
        foreach (var cell in Cells)
        {
            if (cell.Type == CellType.Mine)
            {
                continue;
            }
            var nearby = Cells.Where(c => c.LocalPosition.Distance(cell.LocalPosition) <= 75 && c.LocalPosition.Distance(cell.LocalPosition) > 0);
            cell.Number = nearby.Count(c => c.Type == CellType.Mine);
            if (cell.Number == 0)
            {
                cell.Type = CellType.Empty;
            }
        }
        UpdateChunkBorders();
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
            return c != null && (c.X == 0 || c.X == 15 || c.Z == 0 || c.Z == 15);
        });
        foreach (var cell in border_cells)
        {
            var nearby = Game.ActiveScene.GetAllComponents<Cell>().Where(c => {
                return c.GameObject.WorldPosition.Distance(cell.WorldPosition) <= 75 && c.GameObject.WorldPosition.Distance(cell.WorldPosition) > 1;
            });
            cell.Number = nearby.Count(c => c.Type == CellType.Mine);
            cell.Type = (cell.Number == 0) ? CellType.Empty : CellType.Number;
        }
    }

    public Cell GetCell(Vector3 position)
    {
        var localx = position.x - GameObject.WorldPosition.x;
        var localz = position.z - GameObject.WorldPosition.z;
        // round to nearest 50
        localx = (int)Math.Round(localx / 50) * 50;
        localz = (int)Math.Round(localz / 50) * 50;
        var cell = Cells.FirstOrDefault(c => c.LocalPosition.x == localx && c.LocalPosition.z == localz);
        if (cell == null)
        {
            Log.Error($"Failed to get cell at position {localx},{localz}");
            return null;
        }
        return cell;
    }

    public Cell GetCellFromGrid(int x, int z)
    {
        var cell_comp = Cells.FirstOrDefault(c => c.X == x && c.Z == z);
        if (cell_comp == null)
        {
            Log.Error($"Failed to get cell at position {x},{z}");
            return null;
        }
        return cell_comp;
    }

    public void Update()
    {
        var camera = Scene.Camera;
        if (camera.WorldPosition.Distance(GameObject.WorldPosition) < 1000)
        {
            GameObject.Enabled = true;
        }
        else
        {
            GameObject.Enabled = false;
        }
    }
}
