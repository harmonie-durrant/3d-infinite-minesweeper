public sealed class WorldManager : Component
{
    public static WorldManager Instance { get; set; }

    public static int SAFE_SIZE = 2; // Safe zone size around the first click
    public static int MIN_BOMB_CHUNK = 42; // Percentage of bombs in the world
    public static int MAX_BOMB_CHUNK = 84; // Percentage of bombs in the world

    [Property] public GameObject ChunkPrefab { get; set; }  // Assign this in Inspector
    [Property] public GameObject CellPrefab { get; set; }   // Assign this in Inspector
    [Property] public GameObject Camera { get; set; }

    private Dictionary<Vector2Int, Chunk> LoadedChunks = new();

    public Random random = new();

    protected override void OnAwake()
    {
        Instance = this;
    }

    protected override void OnStart()
    {
        // Generate the first chunk at (0,0)
        GetOrCreateChunk(Vector2Int.Zero);
        GetSurroundingChunks(Vector2Int.Zero);
    }

    public bool IsChunkLoaded(Vector2Int chunkPos)
    {
        return LoadedChunks.ContainsKey(chunkPos);
    }

    public Chunk GetChunk(Vector2Int chunkPos)
    {
        if (LoadedChunks.TryGetValue(chunkPos, out var chunk))
        {
            return chunk;
        }
        // if chunk is near the camera, load it
        Vector3 placePos = new Vector3((chunkPos.x * Chunk.SIZE * Cell.SIZE) - (Chunk.SIZE * Cell.SIZE / 2), (chunkPos.y * Chunk.SIZE * Cell.SIZE) - (Chunk.SIZE * Cell.SIZE / 2));
        if (placePos.Distance(GameObject.WorldPosition) < 1000)
        {
            GetOrCreateChunk(chunkPos);
            return LoadedChunks[chunkPos];
        }
        return null; // Chunk not found
    }

    public Chunk GetOrCreateChunk(Vector2Int chunkPos)
    {
        if (!LoadedChunks.ContainsKey(chunkPos))
        {
            GameObject chunkObject = ChunkPrefab.Clone();
            chunkObject.Parent = GameObject; // Set the parent to this world manager
            chunkObject.WorldPosition = new Vector3((chunkPos.x * Chunk.SIZE * Cell.SIZE) - (Chunk.SIZE * Cell.SIZE / 2), (chunkPos.y * Chunk.SIZE * Cell.SIZE) - (Chunk.SIZE * Cell.SIZE / 2));
            chunkObject.Name = $"Chunk_{chunkPos.x}_{chunkPos.y}"; // Set a unique name for the chunk object

            //Chunk newChunk = new(chunkPos, chunkObject);
            Chunk newChunk = chunkObject.GetOrAddComponent<Chunk>();
            newChunk.CellPrefab = CellPrefab; // Assign the cell prefab to the chunk
            newChunk.ChunkPosition = chunkPos;
            newChunk.GenerateCells(); // Generate cells for the chunk
            LoadedChunks[chunkPos] = newChunk;
        }
        return LoadedChunks[chunkPos];
    }

    public List<Chunk> GetSurroundingChunks(Vector2Int center)
    {
        List<Chunk> chunks = new();
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                chunks.Add(GetOrCreateChunk(center + new Vector2Int(dx, dy)));
            }
        }
        return chunks;
    }
}
