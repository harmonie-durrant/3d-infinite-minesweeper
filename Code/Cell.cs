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
	[Property] public CellState State { get; set; } = CellState.Hidden;
	[Property] public CellType Type { get; set; } = CellType.Empty;
	[Property] public int Number { get; set; } = 0;
	[Property] public bool HasExploded { get; set; } = false;
	public HUD hud {
		get => HUD.Instance;
	}
	public void Show()
	{
		if (State == CellState.Shown || State == CellState.Flagged)
		{
			return;
		}
		var _renderer = GameObject.GetComponents<SkinnedModelRenderer>().FirstOrDefault();
		if (_renderer == null)
		{
			Log.Error("Failed to get renderer component");
			return;
		}
		State = CellState.Shown;
		if (Type == CellType.Mine)
		{
			bool other_has_exploded = false;
			var chunk = GameObject.Parent.Parent.GetComponent<Chunk>();
			if (chunk == null)
			{
				Log.Error("Failed to get chunk component");
				return;
			}
			foreach (var cell in chunk.Cells)
			{
				var _cell = cell.GetComponentInChildren<Cell>();
				if (_cell == null)
				{
					continue;
				}
				if (_cell.HasExploded)
				{
					other_has_exploded = true;
					break;
				}
			}
			if (other_has_exploded)
			{
				_renderer.MaterialOverride = Material.Load("materials/bomb.vmat");
				return;
			}
			_renderer.MaterialOverride = Material.Load("materials/bomb_exploded.vmat");
			HasExploded = true;
			foreach (var cell in chunk.Cells)
			{
				var _cell = cell.GetComponentInChildren<Cell>();
				if (_cell == null)
				{
					continue;
				}
				if (_cell.Type == CellType.Mine)
				{
					_cell.Show();
				}
			}

			//TODO: ulong id = Game.SteamId;
			//TODO: Save Score in database

			GameOver.Instance.Show = true;
			CameraMovement.Instance.IsGameOver = true;
			return;
		}
		hud.Score = hud.Score + 1;
		if (Type == CellType.Number)
		{
			switch (Number)
			{
				case 1:
					_renderer.MaterialOverride = Material.Load("materials/1.vmat");
					break;
				case 2:
					_renderer.MaterialOverride = Material.Load("materials/2.vmat");
					break;
				case 3:
					_renderer.MaterialOverride = Material.Load("materials/3.vmat");
					break;
				case 4:
					_renderer.MaterialOverride = Material.Load("materials/4.vmat");
					break;
				case 5:
					_renderer.MaterialOverride = Material.Load("materials/5.vmat");
					break;
				case 6:
					_renderer.MaterialOverride = Material.Load("materials/6.vmat");
					break;
				case 7:
					_renderer.MaterialOverride = Material.Load("materials/7.vmat");
					break;
				case 8:
					_renderer.MaterialOverride = Material.Load("materials/8.vmat");
					break;
				default:
					_renderer.MaterialOverride = Material.Load("materials/unknown.vmat");
					break;
			}
			Log.Info($"Number: {Number}");
			return;
		}
		if (Type == CellType.Empty || Number == 0)
		{
			//TODO: Show all nearby empty cells (recursively) and numbers
			Log.Info($"Revealing Nearby Cells");
			// get all cells around the current cell that are hidden and Empty
			// var nearby = GameObject.Parent.GetComponentsInChildren<Cell>().Where(c => c.State == CellState.Hidden && c.Type == CellType.Empty && c.Number == 0);
			// foreach (var cell in nearby)
			// {
			// 	if (cell.IsValid())
			// 		cell.Show();
			// }
			_renderer.MaterialOverride = Material.Load("materials/empty.vmat");
			//GameObject.Parent.Destroy();
			return;
		}
	}

	public void Flag()
	{
		if (State == CellState.Hidden)
		{
			State = CellState.Flagged;
			var _renderer = GameObject.GetComponents<SkinnedModelRenderer>().FirstOrDefault();
			_renderer.MaterialOverride = Material.Load("materials/flag.vmat");
		}
		else if (State == CellState.Flagged)
		{
			State = CellState.Hidden;
			var _renderer = GameObject.GetComponents<SkinnedModelRenderer>().FirstOrDefault();
			_renderer.MaterialOverride = Material.Load("materials/unknown.vmat");
		}
	}
}
