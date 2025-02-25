using Sandbox;

public sealed class CameraMovement : Component
{
    [Property] public float MoveSpeed { get; set; } = 100f;
    [Property] public ChunkRenderer _renderer { get; set; }
    public bool IsGameOver { get; set; } = false;
    public Cell PreviousHover { get; set; } = null;

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

    protected override void OnUpdate()
    {
        if (IsGameOver)
        {
            return;
        }
        EnsureMouseVisibility();
        HandleMovement();
        HandleMouseInput();
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
        if (Input.Down("Forward"))
        {
            MoveCamera(Vector3.Up);
        }
        if (Input.Down("Backward"))
        {
            MoveCamera(Vector3.Down);
        }
        if (Input.Down("Left"))
        {
            MoveCamera(Vector3.Backward);
        }
        if (Input.Down("Right"))
        {
            MoveCamera(Vector3.Forward);
        }
    }

    private void MoveCamera(Vector3 direction)
    {
        GameObject.WorldPosition += direction * Time.Delta * MoveSpeed;
        //TODO: Limit movement to the explored grid
        _renderer.UpdateChunks();
    }

    private void HandleMouseInput()
    {
        if (Input.Released("attack1"))
        {
            HandleMouseClick(cell => cell.Show());
            return;
        }
        if (Input.Released("attack2"))
        {
            HandleMouseClick(cell => cell.Flag());
            return;
        }
        HandleMouseClick(cell => {
            if (PreviousHover != null)
            {
                PreviousHover.DeactivateHover();
            }
            cell.ActivateHover();
            PreviousHover = cell;
        });
    }

    private void HandleMouseClick(Action<Cell> cellAction)
    {
        var tr = Scene.Trace.Ray(Scene.Camera.ScreenPixelToRay(Mouse.Position), 1000).Run();
        if (!tr.Hit)
        {
            return;
        }
        if (tr.GameObject == null)
        {
            return;
        }
        var chunk = tr.GameObject.GetComponents<Chunk>().FirstOrDefault();
        if (chunk == null)
        {
            return;
        }
        var cell = chunk.GetCell(tr.EndPosition);
        if (cell == null)
        {
            return;
        }
        cellAction(cell);
        return;
    }
}
