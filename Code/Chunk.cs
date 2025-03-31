public sealed class Chunk : Component
{
    public const int SIZE = 16; // 16x16 grid
    public Vector2Int ChunkPosition { get; set; } // (ChunkX, ChunkY)
    public GameObject CellPrefab { get; set; }   // Assign this in Inspector
    public Dictionary<Vector2Int, Cell> Cells { get; set; } = new();

    public void GenerateCells()
    {
        for (int x = 0; x < SIZE; x++)
        {
            for (int y = 0; y < SIZE; y++)
            {
                var localPos = new Vector2Int(x, y);
                GameObject newCellObject = CellPrefab.Clone();
                newCellObject.Parent = GameObject; // Set the parent to this chunk
                newCellObject.LocalPosition = new Vector3(x * Cell.SIZE, y * Cell.SIZE, 0); // Adjust the spacing as needed
                newCellObject.LocalPosition += new Vector3(Cell.SIZE / 2, Cell.SIZE / 2, 0); // Center the cell in it's position
                Cell newCell = newCellObject.GetComponent<Cell>();
                newCell.Position = localPos;
                newCell.ParentChunk = this;
                Cells[localPos] = newCell;
                newCellObject.NetworkSpawn(); // Spawn the cell object in the network
                newCell.SpriteRenderer = newCellObject.GetOrAddComponent<SpriteRenderer>();
                newCell.SpriteRenderer.Texture = Texture.Load(SpriteBank.Sprites["unknown"]);
            }
        }
    }

    public void PlaceMines(int mineCount, Vector2Int safeZone)
    {
        List<Cell> validCells = Cells.Values.Where(c => c.Position.Distance(safeZone) > WorldManager.SAFE_SIZE).ToList();
        Random random = new();

        for (int i = 0; i < mineCount && validCells.Count > 0; i++)
        {
            int index = random.Next(validCells.Count);
            Cell mineCell = validCells[index];
            mineCell.IsMine = true;
            validCells.RemoveAt(index);
        }

        UpdateMineCounts();
        //Get all neighboring chunks and update their mine counts too
        foreach (var offset in GetNeighborOffsets())
        {
            Vector2Int neighborPos = ChunkPosition + offset;
            Chunk neighboringChunk = WorldManager.Instance.GetChunk(neighborPos);
            if (neighboringChunk != null)
            {
                neighboringChunk.UpdateMineCounts();
            }
        }
    }

    private void UpdateMineCounts()
    {
        foreach (var cell in Cells.Values)
        {
            var old = cell.NeighborMineCount;
            cell.NeighborMineCount = GetNeighboringMines(cell.Position); // Count the mines around this cell
            if (old != cell.NeighborMineCount)
                cell.UpdateVisuals(); // Update the visuals for the cell if the count changed
        }
    }

    private int GetNeighboringMines(Vector2Int pos)
    {
        int count = 0;

        foreach (var offset in GetNeighborOffsets())
        {
            Vector2Int neighborPos = pos + offset;

            // Check if the neighbor is within the current chunk
            if (Cells.ContainsKey(neighborPos))
            {
                if (Cells[neighborPos].IsMine)
                    count++;
                continue;
            }

            // Handle neighbors outside the current chunk
            Vector2Int wrappedNeighborPos = WrapPositionWithinChunk(neighborPos);
            Vector2Int chunkOffset = GetChunkOffset(neighborPos);

            // Check the neighboring chunk
            Chunk neighboringChunk = WorldManager.Instance.GetChunk(ChunkPosition + chunkOffset);
            if (neighboringChunk != null && neighboringChunk.Cells.ContainsKey(wrappedNeighborPos))
            {
                if (neighboringChunk.Cells[wrappedNeighborPos].IsMine)
                    count++;
            }
        }

        return count;
    }

    private Vector2Int WrapPositionWithinChunk(Vector2Int pos)
    {
        // Wrap the position to stay within the bounds of the chunk
        int x = (pos.x < 0) ? SIZE - 1 : (pos.x >= SIZE) ? 0 : pos.x;
        int y = (pos.y < 0) ? SIZE - 1 : (pos.y >= SIZE) ? 0 : pos.y;
        return new Vector2Int(x, y);
    }

    private Vector2Int GetChunkOffset(Vector2Int pos)
    {
        // Determine the chunk offset based on the position
        int offsetX = (pos.x < 0) ? -1 : (pos.x >= SIZE) ? 1 : 0;
        int offsetY = (pos.y < 0) ? -1 : (pos.y >= SIZE) ? 1 : 0;
        return new Vector2Int(offsetX, offsetY);
    }

    public void RevealEmptyArea(Vector2Int startPos)
    {
        if (!Cells.ContainsKey(startPos) || Cells[startPos].IsMine)
        {
            //Log.Info("Cannot reveal empty areas yet (not implemented).");
            return;
        }
        Queue<Cell> queue = new();
        HashSet<Cell> visited = new();
        queue.Enqueue(Cells[startPos]);

        while (queue.Count > 0)
        {
            Cell cell = queue.Dequeue();
            if (visited.Contains(cell) || cell.IsMine) continue;
            visited.Add(cell);
            cell.Reveal();

            if (cell.NeighborMineCount == 0)
            {
                foreach (var offset in GetNeighborOffsets())
                {
                    Vector2Int neighborPos = cell.Position + offset;
                    if (Cells.ContainsKey(neighborPos) && !visited.Contains(Cells[neighborPos]))
                        queue.Enqueue(Cells[neighborPos]);
                    // TODO: Get neighbor chunks too
                }
            }
        }
    }

    private static List<Vector2Int> GetNeighborOffsets()
    {
        return new()
        {
            new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
            new Vector2Int(-1,  0),                      new Vector2Int(1,  0),
            new Vector2Int(-1,  1), new Vector2Int(0,  1), new Vector2Int(1,  1)
        };
    }

    public Cell GetCell(Vector3 worldPosition)
    {
        Vector2Int localPos = WorldToLocalPosition(worldPosition);
        return Cells.TryGetValue(localPos, out var cell) ? cell : null;
    }

    private Vector2Int WorldToLocalPosition(Vector3 worldPosition)
    {
        int x = (int)(worldPosition.x % SIZE);
        int y = (int)(worldPosition.y % SIZE);
        return new Vector2Int(x, y);
    }
}
