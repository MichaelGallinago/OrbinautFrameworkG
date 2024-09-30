using Godot;
using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework.StaticStorages;

namespace OrbinautFrameworkG.Framework.SceneModule;

public partial class Debug : Node
{
	[Export] private PackedScene _startup;
	[Export] private PackedScene _devMenu;
	
#if DEBUG
	private const int DebugFrameLimit = 2;
	
	private enum DebugKeys
	{
		Collision = (int)Key.Key1,
		GameSpeed = (int)Key.Key2,
		RestartRoom = (int)Key.Key9,
		RestartGame = (int)Key.Key0,
		DevMenu = (int)Key.Escape
	}

	public override void _Input(InputEvent input)
	{
		if (input is not InputEventKey { Pressed: false } keyEvent) return;

		switch ((DebugKeys)keyEvent.Keycode)
		{
			case DebugKeys.Collision: OnCollisionPressed(); break;
			case DebugKeys.GameSpeed: OnGameSpeedPressed(); break;
			case DebugKeys.RestartRoom: OnRestartRoomPressed(); break;
			case DebugKeys.RestartGame: OnRestartGamePressed(); break;
			case DebugKeys.DevMenu: OnDevMenuPressed(); break;
		}
	}

	private static void OnCollisionPressed()
	{
		if (++SharedData.SensorDebugType <= SharedData.SensorDebugTypes.SolidBox) return;
		SharedData.SensorDebugType = SharedData.SensorDebugTypes.None;
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
