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

	protected override void OnStart()
	{
		base.OnStart();
		// create 16x16 grid of cells
		// for (int z = 0; z < 16; z++)
		// {
		// 	for (int x = 0; x < 16; x++)
		// 	{
		// 		// Create from prefab
		// 		var obj = prefab_cell.Clone();
		// 		obj.Name = $"Cell-{X},{Z}/{x},{z}";
		// 		obj.LocalPosition = new Vector3((x * 50) - (8 * 50), 0, (z * 50) - (8 * 50));
		// 		obj.Parent = GameObject;
		// 		Cells.Add(obj);
		// 	}
		// }

		// Create using an algorythm for better performance and generating based on minesweeper rules
		var random = new Random();
		for (int z = 0; z < 16; z++)
		{
			for (int x = 0; x < 16; x++)
			{
				// Create from prefab
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
				// 20% chance of being a mine
				if (random.Next(0, 100) < 20)
				{
					cell.Type = CellType.Mine;
					continue;
				}
				// 80% chance of being a number
				cell.Type = CellType.Number;
				cell.Number = -1;
			}
		}
		// Calculate the number of mines around each cell after all cells have been created
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
			// get all cells around the current cell
			var nearby = Cells.Where(c => c.LocalPosition.Distance(cell.LocalPosition) <= 75 && c.LocalPosition.Distance(cell.LocalPosition) > 0);
			// get the number of mines around the current cell
			cell_comp.Number = nearby.Count(c => c.GetComponentInChildren<Cell>().Type == CellType.Mine);
			if (cell_comp.Number == 0)
			{
				cell_comp.Type = CellType.Empty;
			}
		}
	}

	public Cell GetCell(Vector3 position)
	{
		// the middle of the grid is -25 -25 so we need to add 25 to the position to get the correct cell
		var localx = position.x - GameObject.LocalPosition.x;
		var localz = position.z - GameObject.LocalPosition.z;
		var x = (int)Math.Floor((localx + 25) / 50);
		var z = (int)Math.Floor((localz + 25) / 50);
		// get the cell at the position
		var cell = Cells.FirstOrDefault(c => c.LocalPosition.x == x * 50 && c.LocalPosition.z == z * 50);
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
