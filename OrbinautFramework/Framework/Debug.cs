using Godot;
using JetBrains.Annotations;
using OrbinautFramework3.Scenes;

namespace OrbinautFramework3.Framework;

public partial class Debug : Node
{
	private const int DebugFrameLimit = 2;
	
	[UsedImplicitly] private IScene _scene;
	
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
		if (!SharedData.DevMode) return;
		
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
	
	private void OnRestartRoomPressed() => _scene.Reload();
	private void OnRestartGamePressed() => _scene.Change(Scenes.Scenes.Startup);
	private void OnDevMenuPressed() => _scene.Change(Scenes.Scenes.DevMenu);
}
