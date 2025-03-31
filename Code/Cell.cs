public sealed class Cell : Component
{
    public SpriteRenderer SpriteRenderer { get; set; } // Reference to the sprite renderer for visual updates
    public Vector2Int Position { get; set; } // Position inside the chunk (0-15, 0-15)
    public Chunk ParentChunk { get; set; } // The chunk this cell belongs to

    public static int SIZE = 50; // Size of the cell in world units

    public bool IsMine { get; set; } = false;
    public bool IsRevealed { get; set; } = false;
    public bool IsFlagged { get; set; } = false;
    public int NeighborMineCount { get; set; } = 0;

    protected override void OnStart()
    {
        ParentChunk = GameObject.Parent.GetComponent<Chunk>();
    }

    public void UpdateVisuals()
    {
        if (IsRevealed)
        {
            if (IsMine)
            {
                // TODO: Handle mine explosion visuals for clicked mine that caused the game over
                if (CameraMovement.Instance.IsGameOver && IsFlagged)
                    SpriteRenderer.Texture = Texture.Load(SpriteBank.Sprites["flag_incorrect"]);
                SpriteRenderer.Texture = Texture.Load(SpriteBank.Sprites["bomb"]);
            }
            else if (NeighborMineCount > 0)
                SpriteRenderer.Texture = Texture.Load(SpriteBank.Sprites[NeighborMineCount.ToString()]);
            else
                SpriteRenderer.Texture = Texture.Load(SpriteBank.Sprites["empty"]);
        }
        else if (IsFlagged)
        {
            SpriteRenderer.Texture = Texture.Load(SpriteBank.Sprites["flag"]);
        }
        else
        {
            SpriteRenderer.Texture = Texture.Load(SpriteBank.Sprites["unknown"]);
        }
    }

    public void Reveal()
    {
        if (IsRevealed || IsFlagged) return;
        IsRevealed = true;

        if (IsMine)
        {
            CameraMovement.Instance.IsGameOver = true;
            UpdateVisuals();
            return;
        }

        if (NeighborMineCount == 0)
            ParentChunk.RevealEmptyArea(Position);
        UpdateVisuals();
    }

    public void ToggleFlag()
    {
        if (IsRevealed) return;
        IsFlagged = !IsFlagged;
        UpdateVisuals();
    }
}
