public sealed class MusicManager : Component
{
	public static MusicManager Instance { get; set; } = null; // Singleton instance
	[Property] public SoundEvent BackgroundMusic { get; set; } // Background music event

	public SoundHandle DJ = null; // Sound handle for the music

	protected override void OnAwake()
	{
		base.OnAwake();
		Instance = this; // Set the singleton instance
	}

	protected override void OnStart()
	{
		base.OnStart();
		if (BackgroundMusic is not null)
		{
			DJ = Sound.Play(BackgroundMusic); // Play the background music
		}
	}

	// Called when the scene is unloaded
	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (DJ is not null && DJ.IsPlaying)
			StopMusic(); // Stop the music if it's playing
	}

	public void StopMusic()
	{
		if (BackgroundMusic is not null)
			DJ.Stop(0.5f); // Stop the music with a fade out
		DJ = null; // Clear the handle
	}

	public void PlayMusic()
	{
		if (DJ is not null && DJ.IsPlaying)
			StopMusic();
		if (BackgroundMusic is not null)
			DJ = Sound.Play(BackgroundMusic); // Play the new music
	}

	// TODO: Add methods to control the music, like pause, stop, resume, change song, etc.
}
