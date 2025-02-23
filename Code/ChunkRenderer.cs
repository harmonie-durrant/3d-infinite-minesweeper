using Sandbox;

public sealed class ChunkRenderer : Component
{
	[Property] public GameObject prefab_chunk { get; set; }
	public int RenderDistance { get; set; } = 2; // 1x1 grid of chunks

	protected override void OnStart()
	{
		base.OnStart();
		UpdateChunks();
	}

	public void UpdateChunks()
	{
		// get all points that chunks can be spawned at (chunks start at 0,0 and go every (16 * 50) units) on the x and z axis
		var camera = Scene.Camera;
		var x = (int)(camera.WorldPosition.x / (16 * 50));
		var z = (int)(camera.WorldPosition.z / (16 * 50));
		// loop through all chunks in render distance
		for (int i = x - RenderDistance; i <= x + RenderDistance; i++)
		{
			for (int j = z - RenderDistance; j <= z + RenderDistance; j++)
			{
				// get the chunk at the current position
				var chunk = Scene.GetAllComponents<Chunk>().FirstOrDefault(c => c.X == i && c.Z == j);
				// if the chunk doesn't exist, create it
				if (chunk == null)
				{
					GameObject new_chunk = prefab_chunk.Clone();
					new_chunk.Name = $"Chunk-{i},{j}";
					new_chunk.WorldPosition = new Vector3(i * 16 * 50, 0, j * 16 * 50);
					var chunk_comp = new_chunk.GetComponent<Chunk>();
					if (chunk_comp == null)
					{
						new_chunk.AddComponent<Chunk>();
						chunk_comp = new_chunk.GetComponent<Chunk>();
						if (chunk_comp == null)
						{
							Log.Error("Failed to add Chunk component to chunk");
							return;
						}
					}
					chunk_comp.X = i;
					chunk_comp.Z = j;
					continue;
				}
				// if it does exist enable it
				if (!chunk.Enabled)
					chunk.Enabled = true;
			}
		}
	}
}
