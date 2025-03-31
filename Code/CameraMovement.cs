using Sandbox;

public sealed class CameraMovement : Component
{
    [Property] public float MoveSpeed { get; set; } = 100f;
    [Property] public GameOver GameOver { get; set; }
    public bool IsGameOver { get; set; } = false;
    public bool FirstClick { get; set; } = true;
    public Cell previousCell { get; set; } = null;
    private Queue<Vector2Int> chunkLoadQueue = new Queue<Vector2Int>();
    public RealTimeUntil ChunkLoadingDelay { get; set; } = 0.1f; // Delay between chunk loads

    public static CameraMovement Instance {
        get
        {
            if ( !_instance.IsValid() )
            {
                _instance = Game.ActiveScene.GetAllComponents<CameraMovement>().FirstOrDefault();
            }
            return _instance;
        }
    }
    private static CameraMovement _instance { get; set; } = null;

    protected override void OnAwake()
    {
        base.OnAwake();
        _instance = this;

        MoveCamera(Vector3.Zero); // Initialize camera position
        GameObject.WorldPosition = new Vector3(0, 0, 500); // Set initial camera position
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        if (IsGameOver)
        {
            if (GameOver.IsValid())
                GameOver.Show = true;
            return;
        }
        EnsureMouseVisibility();
        HandleMovement();
        HandleMouseInput();
    }

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        // Load one chunk per FixedUpdate frame
        if (ChunkLoadingDelay)
        {
            if (chunkLoadQueue.Count > 0)
            {
                Vector2Int chunkToLoad = chunkLoadQueue.Dequeue();
                WorldManager.Instance.GetOrCreateChunk(chunkToLoad);
            }
            ChunkLoadingDelay = 0.2f; // Reset the delay
        }
    }

    private void EnsureMouseVisibility()
    {
        if (!Mouse.Visible)
        {
            Mouse.Visible = true;
            Mouse.CursorType = "default";
        }
    }

    private void HandleMovement()
    {
        if (FirstClick)
            return; // Don't allow movement until the first click is made
        if (Input.Down("Forward"))
            MoveCamera(new Vector3(1, 0, 0));
        if (Input.Down("Backward"))
            MoveCamera(new Vector3(-1, 0, 0));
        if (Input.Down("Left"))
            MoveCamera(new Vector3(0, 1, 0));
        if (Input.Down("Right"))
            MoveCamera(new Vector3(0, -1, 0));
        // TODO: Add camera zoom controls (max zoom, min zoom, etc.)
    }

    private void MoveCamera(Vector3 direction)
    {
        // TODO: Limit movement to the explored grid (e.g., within the bounds of the revieled cells)
        GameObject.WorldPosition += direction * Time.Delta * MoveSpeed;

        // Calculate the 9 chunk positions around the camera
        for (int dx = -2; dx <= 2; dx++)
        {
            for (int dy = -2; dy <= 2; dy++)
            {
                Vector2Int chunkPos = new Vector2Int(
                    (int)(GameObject.WorldPosition.x / (Chunk.SIZE * Cell.SIZE)) + dx,
                    (int)(GameObject.WorldPosition.y / (Chunk.SIZE * Cell.SIZE)) + dy
                );

                // Add the chunk position to the queue if not already loaded
                if (!WorldManager.Instance.IsChunkLoaded(chunkPos) && !chunkLoadQueue.Contains(chunkPos))
                {
                    chunkLoadQueue.Enqueue(chunkPos);
                }
            }
        }
    }

    private void HandleFirstClick(Cell cell)
    {
        Vector2Int safeZone = cell.Position;

        Random random = new();
        foreach (var chunk in WorldManager.Instance.GetSurroundingChunks(cell.ParentChunk.ChunkPosition))
        {
            // random number between WorldManager.MIN_BOMB_CHUNK and WorldManager.MAX_BOMB_CHUNK
            int nmines = random.Next(
                WorldManager.MIN_BOMB_CHUNK, WorldManager.MAX_BOMB_CHUNK + 1);
            chunk.PlaceMines(nmines, safeZone); // Avoid mines near first click
        }

        if (cell.Reveal())
            HUD.Instance.Score += 1;
        FirstClick = false;
    }

    private void HandleMouseInput()
    {
        HandleMouseEvent(cell => {
            if (Input.Released("attack1"))
            {
                if (FirstClick)
                    HandleFirstClick(cell);
                else
                {
                    if (cell.Reveal())
                        HUD.Instance.Score += 1;
                }
            }
            else if (Input.Released("attack2"))
                cell.ToggleFlag();
            else
            {
                if (previousCell != null && previousCell != cell)
                    previousCell.setHoverState(false);
                cell.setHoverState(true);
            }
            previousCell = cell;
        });
    }

    private void HandleMouseEvent(Action<Cell> cellAction)
    {
        var tr = Scene.Trace.Ray(Scene.Camera.ScreenPixelToRay(Mouse.Position), 1000).Run();
        if (!tr.Hit)
            return;
        if (tr.GameObject == null)
            return;
        var chunk = tr.GameObject.GetComponent<Chunk>();
        if (chunk == null)
            return;
        var cell = chunk.GetCell(tr.HitPosition);
        if (cell == null)
            return;
        cellAction(cell);
    }
}
