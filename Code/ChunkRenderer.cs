using Sandbox;

public sealed class ChunkRenderer : Component
{
    [Property] public GameObject prefab_chunk { get; set; }
    public int RenderDistance { get; set; } = 2;

	public static ChunkRenderer Instance {
		get
		{
			if ( !_instance.IsValid() )
			{
				_instance = Game.ActiveScene.GetAllComponents<ChunkRenderer>().FirstOrDefault();
			}
			return _instance;
		}
	}
	private static ChunkRenderer _instance { get; set; } = null;

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
                    CreateChunk(i, j);
                    continue;
                }
                // if it does exist enable it
                if (!chunk.Enabled)
                    chunk.Enabled = true;
            }
        }
    }

    public Chunk CreateChunk(int x, int z)
    {
        GameObject new_chunk = prefab_chunk.Clone();
        new_chunk.Name = $"Chunk-{x},{z}";
        new_chunk.WorldPosition = new Vector3(x * 16 * 50, 0, z * 16 * 50);
        var chunk_comp = new_chunk.GetComponent<Chunk>();
        if (chunk_comp == null)
        {
            new_chunk.AddComponent<Chunk>();
            chunk_comp = new_chunk.GetComponent<Chunk>();
            if (chunk_comp == null)
            {
                Log.Error("Failed to add Chunk component to chunk");
                return null;
            }
        }
        chunk_comp.X = x;
        chunk_comp.Z = z;
		return chunk_comp;
    }

	public static Chunk GetChunk(int x, int z)
	{
		return Game.ActiveScene.GetAllComponents<Chunk>().FirstOrDefault(c => c.X == x && c.Z == z);
	}
}
