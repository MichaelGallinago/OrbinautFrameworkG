using System;
using System.Linq;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Objects.Player;

namespace OrbinautFramework3.Framework;

public partial class Debug : Node
{
	private const int DebugFrameLimit = 2;
	private const string StartupPath = "res://Screens/Startup/startup.tscn";
	private const string DevMenuPath = "res://Screens/Startup/startup.tscn"; // TODO: replace
	
	private enum DebugKeys
	{
		Swap = (int)Key.Key1,
		Collision = (int)Key.Key2,
		GameSpeed = (int)Key.Key3,
		Resolution = (int)Key.Key8,
		RestartRoom = (int)Key.Key9,
		RestartGame = (int)Key.Key0,
		DevMenu = (int)Key.Escape
	}
	
	private record struct ResetData(
		Vector2 Position,
		Vector2 Velocity,
		float GroundSpeed,
		Constants.Direction Facing,
		uint ScoreCount,
		uint RingCount,
		uint LifeCount,
		bool IsGrounded
	);
	
	private static readonly int PlayerTypesNumber = Enum.GetValuesAsUnderlyingType<Types>().Length;
	
	public override void _Input(InputEvent input)
	{
		if (!SharedData.DevMode) return;
		
		if (input is not InputEventKey { Pressed: false } keyEvent) return;

		switch ((DebugKeys)keyEvent.Keycode)
		{
			case DebugKeys.Swap: OnSwapPressed(); break;
			case DebugKeys.Collision: OnCollisionPressed(); break;
			case DebugKeys.GameSpeed: OnGameSpeedPressed(); break;
			case DebugKeys.RestartRoom: OnRestartRoomPressed(); break;
			case DebugKeys.RestartGame: OnRestartGamePressed(); break;
			case DebugKeys.Resolution: OnResolutionPressed(); break;
			case DebugKeys.DevMenu: OnDevMenuPressed(); break;
		}
	}
	
	private static void OnSwapPressed()
	{
		Player player = PlayerData.Players.First();
		
		AudioPlayer.Sound.Play(SoundStorage.Beep);
		player.ResetState(); // We call this to stop action-specific sounds

		var resetData = new ResetData(player.Position, player.Velocity, player.GroundSpeed, player.Facing, 
			player.ScoreCount, player.RingCount, player.LifeCount, player.IsGrounded);

		if ((int)++player.Type >= PlayerTypesNumber)
		{
			player.Type = (Types)1;
		}
		
		player.Reset();
		player.Animation = Objects.Player.Animations.Move;
		
		player.Position = resetData.Position;
		player.Velocity.Vector = resetData.Velocity;
		player.GroundSpeed.Value = resetData.GroundSpeed;
		player.Facing = resetData.Facing;
		player.ScoreCount = resetData.ScoreCount;
		player.RingCount = resetData.RingCount;
		player.LifeCount = resetData.LifeCount;
		player.IsGrounded = resetData.IsGrounded;
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
	
	private static void OnResolutionPressed()
	{
		Window window = FrameworkData.CurrentScene.Tree.Root;
		window.Size = window.ContentScaleSize = SharedData.ViewSize = SharedData.ViewSize.X switch
		{
			400 => new Vector2I(424, 240),
			424 => new Vector2I(320, 224),
			_ => new Vector2I(400, 224)
		};
	}
	
	private static void OnRestartRoomPressed() => FrameworkData.CurrentScene.Tree.ReloadCurrentScene();
	private static void OnRestartGamePressed() => FrameworkData.CurrentScene.Tree.ChangeSceneToFile(StartupPath);
	private static void OnDevMenuPressed() => FrameworkData.CurrentScene.Tree.ChangeSceneToFile(DevMenuPath);
}
