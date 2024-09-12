#if DEBUG
using Godot;

namespace OrbinautFramework3.Framework;

public partial class Debug : Node
{
	private const int DebugFrameLimit = 2;
	private const string StartupPath = "res://Scenes/Screens/Startup/startup.tscn";
	private const string DevMenuPath = "res://Scenes/Screens/DevMenu/dev_menu.tscn";
	
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
		Engine.MaxFps = Engine.MaxFps == DebugFrameLimit ? SharedData.TargetFps : DebugFrameLimit;
	}
	
	private static void OnRestartRoomPressed() => Scene.Instance.Tree.ReloadCurrentScene();
	private static void OnRestartGamePressed() => Scene.Instance.Tree.ChangeSceneToFile(StartupPath);
	private static void OnDevMenuPressed() => Scene.Instance.Tree.ChangeSceneToFile(DevMenuPath);
}
#endif
