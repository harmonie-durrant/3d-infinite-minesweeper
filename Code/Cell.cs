using Sandbox;
using System.Linq;

public enum CellState
{
    Hidden,
    Shown,
    Flagged
}

public enum CellType
{
    Empty,
    Mine,
    Number
}

public sealed class Cell : Component
{
	public int X { get; set; } = 0;
	public int Z { get; set; } = 0;
    public CellState State { get; set; } = CellState.Hidden;
    public CellType Type { get; set; } = CellType.Empty;
    public int Number { get; set; } = 0;
    public bool HasExploded { get; set; } = false;
	public bool IsHovered { get; set; } = false;

    public HUD hud {
        get => HUD.Instance;
    }

	#region 

	public bool IsHidden => State == CellState.Hidden;
	public bool IsShown => State == CellState.Shown;
	public bool IsFlagged => State == CellState.Flagged;
	public bool IsEmpty => Type == CellType.Empty;
	public bool IsMine => Type == CellType.Mine;
	public bool IsNumber => Type == CellType.Number;

	#endregion

	public void ActivateHover()
	{
		if (IsShown || IsHovered)
		{
			return;
		}
		IsHovered = true;
		var renderer = GetRenderer();
        if (renderer == null)
        {
            return;
        }
		if (IsFlagged)
        {
            renderer.MaterialOverride = Material.Load("materials/flag_selected.vmat");
        }
        else
        {
            renderer.MaterialOverride = Material.Load("materials/unknown_selected.vmat");
        }
	}

	public void DeactivateHover()
	{
		if (IsShown || !IsHovered)
		{
			return;
		}
		IsHovered = false;
		var renderer = GetRenderer();
		if (renderer == null)
		{
			return;
		}
		if (IsFlagged)
		{
			renderer.MaterialOverride = Material.Load("materials/flag.vmat");
		}
		else
		{
			renderer.MaterialOverride = Material.Load("materials/unknown.vmat");
		}
	}

	public List<Cell> GetNearbyCells()
	{
		return Game.ActiveScene.GetAllComponents<Cell>().Where(cell => {
			return cell.WorldPosition.Distance(WorldPosition) <= 75 && cell.WorldPosition.Distance(WorldPosition) > 1;
		}).ToList();
	}

    public void Show()
    {
        if (IsShown || IsFlagged)
        {
			return;
        }

        var renderer = GetRenderer();
        if (renderer == null)
        {
			return;
        }

        State = CellState.Shown;

        if (IsMine)
        {
			HandleMineCell(renderer);
            return;
        }

        hud.Score += 1;

        if (IsNumber || Number > 0)
        {
			SetNumberMaterial(renderer);
            return;
        }

        if (IsEmpty && Number == 0)
        {
			RevealEmptyCell(renderer);
        }
    }

    private SkinnedModelRenderer GetRenderer()
    {
        var renderer = GameObject.GetComponents<SkinnedModelRenderer>().FirstOrDefault();
        if (renderer == null)
        {
            Log.Error("Failed to get renderer component");
        }
        return renderer;
    }

    private void HandleMineCell(SkinnedModelRenderer renderer)
    {

        if (CameraMovement.Instance.IsGameOver)
        {
            renderer.MaterialOverride = Material.Load("materials/bomb.vmat");
            return;
        }

        CameraMovement.Instance.IsGameOver = true;
        renderer.MaterialOverride = Material.Load("materials/bomb_exploded.vmat");
        HasExploded = true;
        RevealAllMines();
        GameOver.Instance.Show = true;
    }

    private void RevealAllMines()
    {
		var chunks = Game.ActiveScene.GetAllComponents<Chunk>();
		foreach (var chunk in chunks)
		{
			if (chunk == null)
			{
				Log.Error("Failed to get chunk component");
				return;
			}

			foreach (var cell in chunk.Cells)
			{
				var cellComponent = cell.GetComponentInChildren<Cell>();
				if (cellComponent.IsMine)
				{
					cellComponent.Show();
				}
				else if (cellComponent.IsFlagged)
				{
					ShowIncorrectFlag(cellComponent);
				}
			}
		}
    }

	private void ShowIncorrectFlag(Cell cellComponent)
	{
		var renderer = cellComponent.GetRenderer();
		if (renderer == null)
		{
			return;
		}
		renderer.MaterialOverride = Material.Load("materials/flag_incorrect.vmat");
	}

    private void SetNumberMaterial(SkinnedModelRenderer renderer)
    {
        string materialPath = Number switch
        {
            1 => "materials/1.vmat",
            2 => "materials/2.vmat",
            3 => "materials/3.vmat",
            4 => "materials/4.vmat",
            5 => "materials/5.vmat",
            6 => "materials/6.vmat",
            7 => "materials/7.vmat",
            8 => "materials/8.vmat",
            _ => "materials/unknown.vmat"
        };

		renderer.MaterialOverride = Material.Load(materialPath);
    }

    private void RevealEmptyCell(SkinnedModelRenderer renderer)
    {
        renderer.MaterialOverride = Material.Load("materials/empty.vmat");
        //RevealNearbyCells();
    }

    public void Flag()
    {
        var renderer = GetRenderer();
        if (renderer == null)
        {
            return;
        }

        if (IsHidden)
        {
            State = CellState.Flagged;
            renderer.MaterialOverride = Material.Load(IsHovered ? "materials/flag_hovered.vmat" : "materials/flag.vmat");
        }
        else if (IsFlagged)
        {
            State = CellState.Hidden;
            renderer.MaterialOverride = Material.Load(IsHovered ? "materials/unknown_hovered.vmat" : "materials/unknown.vmat");
        }
    }

	private void RevealNearbyCells()
	{
		var chunk = GameObject.Parent.Parent.GetComponent<Chunk>();
		if (chunk == null)
		{
			Log.Error("Failed to get chunk component");
			return;
		}
		GenerateChunksForReveal(chunk);
		var nearby = GetNearbyCells(chunk);
		foreach (var cell in nearby)
		{
			var cellComponent = cell.GetComponentInChildren<Cell>();
			if (cellComponent.IsHidden && cellComponent.Type != CellType.Mine)
			{
				cellComponent.Show();
				HUD.Instance.Score += 1;
			}
		}
	}

	private void GenerateChunksForReveal(Chunk chunk)
	{
		if (X == 1)
			ChunkRenderer.Instance.CreateChunk(chunk.X - 1, chunk.Z);
		if (Z == 1)
			ChunkRenderer.Instance.CreateChunk(chunk.X, chunk.Z - 1);
		if (X == 1 && Z == 1)
			ChunkRenderer.Instance.CreateChunk(chunk.X - 1, chunk.Z - 1);
		if (X == 14)
			ChunkRenderer.Instance.CreateChunk(chunk.X +1, chunk.Z);
		if (Z == 14)
			ChunkRenderer.Instance.CreateChunk(chunk.X, chunk.Z + 1);
		if (X == 14 && Z == 14)
			ChunkRenderer.Instance.CreateChunk(chunk.X + 1, chunk.Z + 1);
	}

	private List<GameObject> GetNearbyCells(Chunk chunk)
	{
		var nearby = chunk.Cells.Where(cell => {
			var cellComponent = cell.GetComponentInChildren<Cell>();
			if (cellComponent == null)
			{
				Log.Error("Failed to get cell component");
				return false;
			}
			return cellComponent.X >= X - 1 && cellComponent.X <= X + 1 && cellComponent.Z >= Z - 1 && cellComponent.Z <= Z + 1 && cellComponent.Type != CellType.Mine;
		});
		if (X == 0 || X == 15 || Z == 0 || Z == 15)
			nearby.Concat(GetNearbyCellsInNeighbouringChunks(chunk));
		return nearby.ToList();
	}

	private List<GameObject> GetNearbyCellsInNeighbouringChunks(Chunk chunk)
	{
		var nearby = new List<GameObject>();
		if (X == 0)
		{
			var leftChunk = ChunkRenderer.GetChunk(chunk.X - 1, chunk.Z);
			if (leftChunk != null)
			{
				nearby.AddRange(leftChunk.Cells.Where(cell => {
					var cellComponent = cell.GetComponentInChildren<Cell>();
					if (cellComponent == null)
					{
						Log.Error("Failed to get cell component");
						return false;
					}
					return cellComponent.X == 15 && cellComponent.Z >= Z - 1 && cellComponent.Z <= Z + 1 && cellComponent.Type != CellType.Mine;
				}));
			}
			if (Z == 0)
			{
				var bottomLeftChunk = ChunkRenderer.GetChunk(chunk.X - 1, chunk.Z - 1);
				if (bottomLeftChunk != null)
				{
					nearby.AddRange(bottomLeftChunk.Cells.Where(cell => {
						var cellComponent = cell.GetComponentInChildren<Cell>();
						if (cellComponent == null)
						{
							Log.Error("Failed to get cell component");
							return false;
						}
						return cellComponent.X == 15 && cellComponent.Z == 15 && cellComponent.Type != CellType.Mine;
					}));
				}
			}
			if (Z == 15)
			{
				var topLeftChunk = ChunkRenderer.GetChunk(chunk.X - 1, chunk.Z + 1);
				if (topLeftChunk != null)
				{
					nearby.AddRange(topLeftChunk.Cells.Where(cell => {
						var cellComponent = cell.GetComponentInChildren<Cell>();
						if (cellComponent == null)
						{
							Log.Error("Failed to get cell component");
							return false;
						}
						return cellComponent.X == 15 && cellComponent.Z == 0 && cellComponent.Type != CellType.Mine;
					}));
				}
			}
		}
		if (X == 15)
		{
			var rightChunk = ChunkRenderer.GetChunk(chunk.X + 1, chunk.Z);
			if (rightChunk != null)
			{
				nearby.AddRange(rightChunk.Cells.Where(cell => {
					var cellComponent = cell.GetComponentInChildren<Cell>();
					if (cellComponent == null)
					{
						Log.Error("Failed to get cell component");
						return false;
					}
					return cellComponent.X == 0 && cellComponent.Z >= Z - 1 && cellComponent.Z <= Z + 1 && cellComponent.Type != CellType.Mine;
				}));
			}
			if (Z == 0)
			{
				var bottomRightChunk = ChunkRenderer.GetChunk(chunk.X + 1, chunk.Z - 1);
				if (bottomRightChunk != null)
				{
					nearby.AddRange(bottomRightChunk.Cells.Where(cell => {
						var cellComponent = cell.GetComponentInChildren<Cell>();
						if (cellComponent == null)
						{
							Log.Error("Failed to get cell component");
							return false;
						}
						return cellComponent.X == 0 && cellComponent.Z == 15 && cellComponent.Type != CellType.Mine;
					}));
				}
			}
			if (Z == 15)
			{
				var topRightChunk = ChunkRenderer.GetChunk(chunk.X + 1, chunk.Z + 1);
				if (topRightChunk != null)
				{
					nearby.AddRange(topRightChunk.Cells.Where(cell => {
						var cellComponent = cell.GetComponentInChildren<Cell>();
						if (cellComponent == null)
						{
							Log.Error("Failed to get cell component");
							return false;
						}
						return cellComponent.X == 0 && cellComponent.Z == 0 && cellComponent.Type != CellType.Mine;
					}));
				}
			}
		}
		if (Z == 0)
		{
			var bottomChunk = ChunkRenderer.GetChunk(chunk.X, chunk.Z - 1);
			if (bottomChunk != null)
			{
				nearby.AddRange(bottomChunk.Cells.Where(cell => {
					var cellComponent = cell.GetComponentInChildren<Cell>();
					if (cellComponent == null)
					{
						Log.Error("Failed to get cell component");
						return false;
					}
					return cellComponent.Z == 15 && cellComponent.X >= X - 1 && cellComponent.X <= X + 1 && cellComponent.Type != CellType.Mine;
				}));
			}
		}
		if (Z == 15)
		{
			var topChunk = ChunkRenderer.GetChunk(chunk.X, chunk.Z + 1);
			if (topChunk != null)
			{
				nearby.AddRange(topChunk.Cells.Where(cell => {
					var cellComponent = cell.GetComponentInChildren<Cell>();
					if (cellComponent == null)
					{
						Log.Error("Failed to get cell component");
						return false;
					}
					return cellComponent.Z == 0 && cellComponent.X >= X - 1 && cellComponent.X <= X + 1 && cellComponent.Type != CellType.Mine;
				}));
			}
		}
		return nearby;
	}
}
