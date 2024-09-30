using System;
using Godot;
using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework.StaticStorages;

namespace OrbinautFrameworkG.Framework.SceneModule;

public partial class Debug : Node2D
{
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
	
	private static SensorTypes _sensorType = SensorTypes.None;
	public static event Action<SensorTypes> SensorDebugToggled;
	public static SensorTypes SensorType 
	{ 
		get => _sensorType;
		set
		{
			if ((value == SensorTypes.None) ^ (_sensorType == SensorTypes.None)) return;
			SensorDebugToggled?.Invoke(value);
			_sensorType = value;
		}
	}

	public enum SensorTypes : byte { None, Collision, HitBox, SolidBox }
	
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
			case DebugKeys.Collision: OnCollisionPressed(); break;
			case DebugKeys.GameSpeed: OnGameSpeedPressed(); break;
			case DebugKeys.RestartRoom: OnRestartRoomPressed(); break;
			case DebugKeys.RestartGame: OnRestartGamePressed(); break;
			case DebugKeys.DevMenu: OnDevMenuPressed(); break;
		}
	}

	private static void OnCollisionPressed()
	{
		if (++SensorType <= SensorTypes.SolidBox) return;
		SensorType = SensorTypes.None;
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
	
	public override void _Draw()
	{
		base._Draw();
		
	}
#else
	public Debug() => QueueFree();
#endif
}
