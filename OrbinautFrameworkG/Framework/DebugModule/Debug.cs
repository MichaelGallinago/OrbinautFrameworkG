using Godot;
using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework.StaticStorages;

namespace OrbinautFrameworkG.Framework.DebugModule;

public partial class Debug : Node
{
	[Export] public DebugOverlay Overlay { get; private set; }
	
	[Export] private PackedScene _startup;
	[Export] private PackedScene _devMenu;
	
#if DEBUG
	private enum DebugKeys
	{
		Collision = (int)Key.Key1,
		GameSpeed = (int)Key.Key2,
		RestartRoom = (int)Key.Key9,
		RestartGame = (int)Key.Key0,
		DevMenu = (int)Key.Escape
	}
	
	private const int DebugFrameLimit = 2;
	
	public static Debug Instance { get; private set; }
	
	public Debug()
	{
		if (Instance == null)
		{
			Instance = this;
			return;
		}
		
		QueueFree();
	}

	public override void _Input(InputEvent input)
	{
		if (input is not InputEventKey { Pressed: false } keyEvent) return;

		switch ((DebugKeys)keyEvent.Keycode)
		{
			case DebugKeys.Collision: Overlay.ChangeSensorType(); break;
			case DebugKeys.GameSpeed: OnGameSpeedPressed(); break;
			case DebugKeys.RestartRoom: OnRestartRoomPressed(); break;
			case DebugKeys.RestartGame: OnRestartGamePressed(); break;
			case DebugKeys.DevMenu: OnDevMenuPressed(); break;
		}
	}
	
	private static void OnGameSpeedPressed()
	{
		Engine.MaxFps = Engine.MaxFps == DebugFrameLimit ? Settings.TargetFps : DebugFrameLimit;
	}
	
	private void OnRestartRoomPressed()
	{
		AudioPlayer.StopAll();
		GetTree().ReloadCurrentScene();
	}
	
	private void OnRestartGamePressed()
	{
		AudioPlayer.StopAll();
		GetTree().ChangeSceneToPacked(_startup);
	}

	private void OnDevMenuPressed()
	{
		AudioPlayer.StopAll();
		GetTree().ChangeSceneToPacked(_devMenu);
	}
#else
	public Debug() => QueueFree();
#endif
}
