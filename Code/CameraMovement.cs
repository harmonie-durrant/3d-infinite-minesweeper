using Sandbox;

public sealed class CameraMovement : Component
{
	[Property] public float MoveSpeed { get; set; } = 100f;
	[Property] public ChunkRenderer _renderer { get; set; }
	public bool IsGameOver { get; set; } = false;

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
		if (!Mouse.Visible)
		{
			Mouse.Visible = true;
			Mouse.CursorType = "default";
		}
		if (Input.Down("Forward"))
		{
			GameObject.WorldPosition += Vector3.Up * Time.Delta * MoveSpeed;
			//TODO: Limit movement to the explored grid
			_renderer.UpdateChunks();
		}
		if (Input.Down("Backward"))
		{
			GameObject.WorldPosition += Vector3.Down * Time.Delta * MoveSpeed;
			//TODO: Limit movement to the explored grid
			_renderer.UpdateChunks();
		}
		if (Input.Down("Left"))
		{
			GameObject.WorldPosition += Vector3.Backward * Time.Delta * MoveSpeed;
			//TODO: Limit movement to the explored grid
			_renderer.UpdateChunks();
		}
		if (Input.Down("Right"))
		{
			GameObject.WorldPosition += Vector3.Forward * Time.Delta * MoveSpeed;
			//TODO: Limit movement to the explored grid
			_renderer.UpdateChunks();
		}
		if (Input.Released("attack1"))
		{
			var tr = Scene.Trace.Ray( Scene.Camera.ScreenPixelToRay( Mouse.Position ), 1000 ).Run();
			if (tr.Hit)
			{
				if (tr.GameObject == null)
				{
					Log.Info("No GameObject found");
					return;
				}
				var chunk = tr.GameObject.GetComponents<Chunk>().FirstOrDefault();
				if (chunk == null)
				{
					Log.Info("Chunk not found");
					return;
				}
				var cell = chunk.GetCell(tr.EndPosition);
				if (cell == null)
				{
					Log.Info("No cell found to show");
					return;
				}
				cell.Show();
				return;
			}
			Log.Info("No Hit" + tr.EndPosition);
		}
		if (Input.Released("attack2"))
		{
			var tr = Scene.Trace.Ray( Scene.Camera.ScreenPixelToRay( Mouse.Position ), 1000 ).Run();
			if (tr.Hit)
			{
				if (tr.GameObject == null)
				{
					Log.Info("No GameObject found");
					return;
				}
				var chunk = tr.GameObject.GetComponents<Chunk>().FirstOrDefault();
				if (chunk == null)
				{
					Log.Info("Chunk not found");
					return;
				}
				var cell = chunk.GetCell(tr.EndPosition);
				if (cell == null)
				{
					Log.Info("No cell found to flag");
					return;
				}
				cell.Flag();
				return;
			}
			Log.Info("No Hit" + tr.EndPosition);
		}
	}
}
