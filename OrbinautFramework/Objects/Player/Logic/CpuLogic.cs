using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.InputModule;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.Logic;

public class CpuLogic(PlayerData data, IPlayerLogic logic)
{
	public enum States : byte
	{
		RespawnInit, Respawn, Main, Fly, Stuck
	}
	
	public const int DelayStep = 16;
	private const int JumpFrequency = 64;
	
	private int _delay;
	private Buttons _inputDown;
	private Buttons _inputPress;
	private bool _canReceiveInput;
	private IPlayer _leadPlayer;
	
    public void Process()
    {
		_leadPlayer = Scene.Instance.Players.FirstOrDefault();
		_delay = DelayStep * data.Id;
		
		// Read actual player input and enable manual control for 10 seconds if detected it
		_canReceiveInput = data.Id < Constants.MaxInputDevices;
		
		if (_canReceiveInput && 
		    data.Input.Down is not { Abc: false, Up: false, Down: false, Left: false, Right: false })
		{
			data.Cpu.InputTimer = 600f;
		}
		
		switch (data.Cpu.State)
		{
			case States.RespawnInit: InitRespawn(); break;
			case States.Respawn: ProcessRespawn(); break;
			case States.Main: ProcessMain(); break;
			case States.Stuck: ProcessStuck(); break;
		}
	}

	private void InitRespawn()
	{
		// Take some time (up to 64 frames (on 60 fps))
		// to respawn or do not respawn at all unless holding down any button
		if (_canReceiveInput && data.Input.Down is { Abc: false, Start: false })
		{
			if (!Scene.Instance.IsTimePeriodLooped(64f)) return;
		}
		
		data.Node.Position = _leadPlayer.Position - new Vector2(0f, SharedData.ViewSize.Y - 32);
		
		data.Cpu.State = States.Respawn;
	}

	private void ProcessRespawn()
	{
		if (CheckRespawn()) return;

		SetRespawnAnimation();
		
#if S3_CPU
		if (data.Node.Type == PlayerNode.Types.Tails)
		{
			PlayRespawnFlyingSound();
		}
#endif
		
		Vector2I targetPosition = _leadPlayer.Recorder.Data[_delay].Position;

#if S2_CPU
		if (Stage.Local != null && Stage.Local.IsWaterEnabled)
		{
			targetPosition.Y = Math.Min(Stage.Local.WaterLevel - 16, targetPosition.Y);
		}
#endif
		
		if (MoveToLeadPlayer(targetPosition)) return;
#if S3_CPU
		if (_leadPlayer.Data.State == PlayerStates.Death) return;
#endif
		if (!_leadPlayer.Recorder.Data[_delay].IsGrounded) return;
		
		data.Cpu.State = States.Main;
		data.Sprite.Animation = Animations.Move;
		data.State = PlayerStates.Control;
	}

	private void SetRespawnAnimation()
	{
		data.Sprite.Animation = data.Node.Type switch
		{
			PlayerNode.Types.Sonic or PlayerNode.Types.Amy => Animations.Spin, 
			PlayerNode.Types.Tails => data.Water.IsUnderwater ? Animations.Swim : Animations.Fly,
			PlayerNode.Types.Knuckles => Animations.GlideAir,
			_ => throw new ArgumentOutOfRangeException()
		};
	}
	
#if S3_CPU
	private void PlayRespawnFlyingSound()
	{
		if (!Scene.Instance.IsTimePeriodLooped(16f, 8f) || !data.Sprite.CheckInCameras()) return;
		if (data.Water.IsUnderwater) return;
		
		AudioPlayer.Sound.Play(SoundStorage.Flight);
	}
#endif

	private bool MoveToLeadPlayer(Vector2I targetPosition)
	{
		var distance = new Vector2I(
			(int)data.Node.Position.X - targetPosition.X, 
			targetPosition.Y - (int)data.Node.Position.Y);

		Vector2 positionOffset = Vector2.Zero;
		if (distance.X != 0)
		{
			float velocityX = Math.Abs(_leadPlayer.Data.Movement.Velocity.X);
			velocityX += Math.Min(Math.Abs(distance.X) / 16, 12) + 1f;
			velocityX *= Scene.Instance.Speed;
			
			//TODO: check this
			if (distance.X >= 0)
			{
				if (velocityX < distance.X)
				{
					velocityX = -velocityX;
				}
				else
				{
					velocityX = -distance.X;
					distance.X = 0;
				}

				data.Visual.Facing = Constants.Direction.Negative;
			}
			else
			{
				velocityX = -velocityX;

				if (velocityX >= distance.X)
				{
					velocityX = distance.X;
					distance.X = 0;
				}

				data.Visual.Facing = Constants.Direction.Positive;
			}

			positionOffset.X = velocityX;
		}

		if (distance.Y != 0)
		{
			positionOffset.Y = Math.Sign(distance.Y) * Scene.Instance.Speed;
		}

		data.Node.Position += positionOffset;
		
		return distance.X != 0 || distance.Y != 0;
	}
	
	private void ProcessMain()
	{
		//TODO: check that ZIndex works
		data.Node.ZIndex = _leadPlayer.Data.Node.ZIndex + data.Id;
		
		FreezeOrFlyIfLeaderDied();
		
		if (CheckRespawn()) return; // Exit if respawned
		
		if (data.State == PlayerStates.NoControl) return; 
		if (data.Carry.Target != null || logic.Action == ActionFsm.States.Carried) return;
		
		data.Cpu.Target ??= _leadPlayer; // Follow lead player
		
		// Exit if CPU logic is disabled
		if (data.Cpu.InputTimer > 0f)
		{
			data.Cpu.InputTimer -= Scene.Instance.Speed;
			if (!data.Input.NoControl) return;
		}
		
		if (data.Movement.GroundLockTimer > 0f && data.Movement.GroundSpeed == 0f)
		{
			data.Cpu.State = States.Stuck;
		}

		TryJump();
		
		data.Input.Set(_inputPress, _inputDown);
	}

	private void TryJump()
	{
		IPlayer target = data.Cpu.Target;
		(Vector2I targetPosition, _inputPress, _inputDown, Constants.Direction direction, object isTargetPush, _) = 
			target.Recorder.Data[_delay];
		
#if S3_CPU
		if (Math.Abs(target.Data.Movement.GroundSpeed) < 4f && target.Data.Collision.OnObject == null)
		{
			targetPosition.X -= 32;
		}
#endif
		
		// Jump if we are not pushing anything or if the followed player was pushing something a few frames ago
		if (data.Visual.SetPushBy != null && isTargetPush == null)
		{
			Jump();
			return;
		}
		
		int distanceX = targetPosition.X - (int)data.Node.Position.X;
		Push(distanceX, direction);
		if (CheckJump(distanceX, targetPosition.Y))
		{
			Jump();
		}
	}

	// Copy (and modify) inputs
	private void Jump()
	{
		if (data.Sprite.Animation == Animations.Duck) return;
		if (data.Cpu.Target.Data.Sprite.Animation == Animations.Wait) return;
		if (!Scene.Instance.IsTimePeriodLooped(JumpFrequency)) return;
		
		_inputPress.Abc = _inputDown.Abc = true;
		data.Cpu.IsJumping = true;
	}

	private void FreezeOrFlyIfLeaderDied()
	{
		// Freeze or start flying (if we're Tails) if lead player has died
		if (_leadPlayer.Data.State != PlayerStates.Death) return;
		
		logic.ResetData();
		logic.Action = ActionFsm.States.Default;
		
		data.Cpu.State = States.Respawn;
		data.State = PlayerStates.NoControl;
	}
	
	private void Push(int distanceX, Constants.Direction facing)
	{
		if (distanceX == 0)
		{
			data.Visual.Facing = facing;
			return;
		}
		
#if S3_CPU
		const int maxDistanceX = 48;
#else
		const float maxDistanceX = 16f;
#endif

		bool isMoveToRight = distanceX > 0;
		int sign = isMoveToRight ? 1 : -1;
		if (sign * distanceX > maxDistanceX)
		{
			_inputDown.Left = _inputPress.Left = !isMoveToRight;
			_inputDown.Right = _inputPress.Right = isMoveToRight;
		}
						
		if (data.Movement.GroundSpeed != 0f && (int)data.Visual.Facing == sign)
		{
			data.Node.Position += Vector2.Right * sign;
		}
	}
	
	private bool CheckJump(int distanceX, int targetPositionY)
	{
		if (data.Cpu.IsJumping)
		{
			_inputDown.Abc = true;
				 
			if (!data.Movement.IsGrounded) return false;
			
			data.Cpu.IsJumping = false;
			return true;
		}

		const float jumpPeriod = JumpFrequency * 4f;
		if (distanceX >= 64 && !Scene.Instance.IsTimePeriodLooped(jumpPeriod)) return false;
		return targetPositionY - (int)data.Node.Position.Y <= -32;
	}
	
	private void ProcessStuck()
	{
		if (CheckRespawn()) return;
		
		if (data.Movement.GroundLockTimer > 0f || data.Cpu.InputTimer > 0f || data.Movement.GroundSpeed != 0f) return;
		
		if (data.Sprite.Animation == Animations.Idle)
		{
			data.Visual.Facing = (int)data.Cpu.Target.Position.X >= (int)data.Node.Position.X ? 
				Constants.Direction.Positive : Constants.Direction.Negative;
		}
		
		if (!Scene.Instance.IsTimePeriodLooped(128f))
		{
			data.Input.Down = data.Input.Down with { Down = true };
			if (!Scene.Instance.IsTimePeriodLooped(32f)) return;
			data.Input.Press = data.Input.Press with { Abc = true };
			
			return;
		}
		
		data.Input.Down = data.Input.Down with { Down = false };
		data.Input.Press = data.Input.Press with { Abc = false };
		data.Cpu.State = States.Main;
	}
	
	private bool CheckRespawn()
	{
		bool isBehindLeader = _leadPlayer.Data.Node.IsCameraTarget(out ICamera camera) && 
		                      camera.TargetBoundary.Z <= data.Node.Position.X;
		
		if (isBehindLeader || data.Sprite != null && data.Sprite.CheckInCameras())
		{
			data.Cpu.RespawnTimer = 0f;
			return false;
		}
		
		data.Cpu.RespawnTimer += Scene.Instance.Speed;
		if (data.Cpu.RespawnTimer < 300f)
		{
			if (!data.Collision.OnObject.IsInstanceValid()) return false;
		}
		
		logic.Respawn();
		return true;
	}
}
