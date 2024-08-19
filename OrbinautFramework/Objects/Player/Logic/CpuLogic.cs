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
	
	public enum Behaviours : byte
	{
		S2, S3
	}
	
	public const int DelayStep = 16;
	private const int JumpFrequency = 64;
	
	private int _delay;
	private Buttons _inputDown;
	private Buttons _inputPress;
	private bool _canReceiveInput;
	private ICpuTarget _leadPlayer;
	
    public void Process()
    {
		if (data.Damage.IsHurt || data.Death.IsDead || data.Id == 0) return;
		
		_leadPlayer = Scene.Instance.Players.First();
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
			if (!Scene.Instance.IsTimePeriodLooped(64f) || !_leadPlayer.IsObjectInteractionEnabled) return;
		}
		
		data.Node.Position = _leadPlayer.Position - new Vector2(0f, SharedData.ViewSize.Y - 32);
		
		data.Cpu.State = States.Respawn;
	}

	private void ProcessRespawn()
	{
		if (CheckRespawn()) return;

		SetRespawnAnimation();
		if (data.Node.Type == PlayerNode.Types.Tails)
		{
			PlayRespawnFlyingSound();
		}
		
		DataRecord followDataRecord = _leadPlayer.RecordedData[_delay];
		Vector2 targetPosition = followDataRecord.Position;

		if (SharedData.Behaviour == Behaviours.S2)
		{
			if (Stage.Local != null && Stage.Local.IsWaterEnabled)
			{
				targetPosition.Y = Math.Min(Stage.Local.WaterLevel - 16, targetPosition.Y);
			}
		}
		
		if (MoveToLeadPlayer(targetPosition)) return;

		if (SharedData.Behaviour == Behaviours.S3 && _leadPlayer.IsDead) return;

		if (!data.Movement.IsGrounded || !Mathf.IsEqualApprox(targetPosition.Y, followDataRecord.Position.Y)) return;
		
		data.Cpu.State = States.Main;
		data.Visual.Animation = Animations.Move;
		data.Collision.IsObjectInteractionEnabled = true;
		data.Movement.IsControlRoutineEnabled = true;
	}

	private void SetRespawnAnimation()
	{
		data.Visual.Animation = data.Node.Type switch
		{
			PlayerNode.Types.Sonic or PlayerNode.Types.Amy => Animations.Spin, 
			PlayerNode.Types.Tails => data.Water.IsUnderwater ? Animations.Swim : Animations.Fly,
			PlayerNode.Types.Knuckles => Animations.GlideAir,
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	private void PlayRespawnFlyingSound()
	{
		if (SharedData.Behaviour != Behaviours.S3) return;
		if (!Scene.Instance.IsTimePeriodLooped(16f, 8f) || !data.Node.SpriteNode.CheckInCameras()) return;
		if (data.Water.IsUnderwater) return;
		
		AudioPlayer.Sound.Play(SoundStorage.Flight);
	}

	private bool MoveToLeadPlayer(Vector2 targetPosition)
	{
		var distance = new Vector2I(
			Mathf.FloorToInt(data.Node.Position.X - targetPosition.X),
			Mathf.FloorToInt(targetPosition.Y - data.Node.Position.Y));

		Vector2 positionOffset = Vector2.Zero;
		if (distance.X != 0)
		{
			float velocityX = Math.Abs(_leadPlayer.Velocity.X) + Math.Min(Math.Abs(distance.X) / 16, 12) + 1f;
			velocityX *= Scene.Instance.ProcessSpeed;
			
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
			positionOffset.Y = Math.Sign(distance.Y) * Scene.Instance.ProcessSpeed;
		}

		data.Node.Position += positionOffset;
		
		return distance.X != 0 || distance.Y != 0;
	}
	
	private void ProcessMain()
	{
		FreezeOrFlyIfLeaderDied();
		
		if (CheckRespawn()) return; // Exit if respawned
		
		if (!data.Collision.IsObjectInteractionEnabled || 
		    data.Carry.Target != null || logic.Action == ActionFsm.States.Carried) return;
		
		data.Cpu.Target ??= _leadPlayer; // Follow lead player
		
		// Exit if CPU logic is disabled
		if (data.Cpu.InputTimer > 0f)
		{
			data.Cpu.InputTimer -= Scene.Instance.ProcessSpeed;
			if (!data.Input.NoControl) return;
		}
		
		//TODO: check that ZIndex works
		data.Node.ZIndex = _leadPlayer.ZIndex + data.Id;
		
		if (data.Movement.GroundLockTimer > 0f && data.Movement.GroundSpeed == 0f)
		{
			data.Cpu.State = States.Stuck;
		}

		TryJump();
		
		data.Input.Set(_inputPress, _inputDown);
	}

	private void TryJump()
	{
		(Vector2 targetPosition, _inputPress, _inputDown, Constants.Direction direction, object isTargetPush) = 
			data.Cpu.Target.RecordedData[_delay];

		if (SharedData.Behaviour == Behaviours.S3 &&
		    Math.Abs(data.Cpu.Target.GroundSpeed) < 4f && data.Cpu.Target.OnObject == null)
		{
			targetPosition.X -= 32f;
		}
		
		// Copy (and modify) inputs if we are not pushing anything or if the followed player
		// was pushing something a few frames ago
		if (data.Visual.SetPushBy != null && isTargetPush == null)
		{
			Jump();
			return;
		}
		
		int distanceX = Mathf.FloorToInt(targetPosition.X - data.Node.Position.X);
		Push(distanceX, direction);
		if (CheckJump(distanceX, targetPosition.Y))
		{
			Jump();
		}
	}

	private void Jump()
	{
		if (data.Visual.Animation == Animations.Duck || data.Cpu.Target.Animation == Animations.Wait) return;
		if (!Scene.Instance.IsTimePeriodLooped(JumpFrequency)) return;
		
		_inputPress.Abc = _inputDown.Abc = true;
		data.Cpu.IsJumping = true;
	}

	private void FreezeOrFlyIfLeaderDied()
	{
		// Freeze or start flying (if we're Tails) if lead player has died
		if (!_leadPlayer.IsDead) return;
		
		logic.ResetState();
		logic.Action = ActionFsm.States.Default;
		
		data.Cpu.State = States.Respawn;
		data.Movement.IsControlRoutineEnabled = false;
		data.Collision.IsObjectInteractionEnabled = false;
	}
	
	private void Push(float distanceX, Constants.Direction facing)
	{
		if (distanceX == 0f)
		{
			data.Visual.Facing = facing;
			return;
		}
		
		int maxDistanceX = SharedData.PhysicsType == PhysicsCore.Types.S3 ? 48 : 16;

		bool isMoveToRight = distanceX > 0f;
		int sign = isMoveToRight ? 1 : -1;
		if (sign * distanceX > maxDistanceX)
		{
			_inputDown.Left = _inputPress.Left = !isMoveToRight;
			_inputDown.Right = _inputPress.Right = isMoveToRight;
		}
						
		if (data.Movement.GroundSpeed != 0f && data.Movement.IsControlRoutineEnabled && (int)data.Visual.Facing == sign)
		{
			data.Node.Position += Vector2.Right * sign;
		}
	}
	
	private bool CheckJump(float distanceX, float targetPositionY)
	{
		if (data.Cpu.IsJumping)
		{
			_inputDown.Abc = true;
				 
			if (!data.Movement.IsGrounded) return false;
			
			data.Cpu.IsJumping = false;
			return true;
		}

		const float jumpPeriod = JumpFrequency * 4f;
		if (distanceX >= 64f && !Scene.Instance.IsTimePeriodLooped(jumpPeriod)) return false;
		return targetPositionY - data.Node.Position.Y <= -32f;
	}
	
	private void ProcessStuck()
	{
		if (CheckRespawn()) return;
		
		if (data.Movement.GroundLockTimer > 0f || data.Cpu.InputTimer > 0f || data.Movement.GroundSpeed != 0f) return;
		
		if (data.Visual.Animation == Animations.Idle)
		{
			data.Visual.Facing = data.Cpu.Target.Position.X >= data.Node.Position.X ? 
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
		bool isBehindLeader = _leadPlayer.IsCameraTarget(out ICamera camera) && 
		                      camera.TargetBoundary.Z <= data.Node.Position.X;
		
		if (isBehindLeader || data.Node.SpriteNode != null && data.Node.SpriteNode.CheckInCameras())
		{
			data.Cpu.RespawnTimer = 0f;
			return false;
		}
		
		data.Cpu.RespawnTimer += Scene.Instance.ProcessSpeed;
		if (data.Cpu.RespawnTimer < 300f)
		{
			if (!data.Collision.OnObject.IsInstanceValid()) return false;
		}
		
		Respawn();
		return true;
	}

	private void Respawn()
	{
		data.Cpu.Init();
		
		if (data.IsCameraTarget(out ICamera camera))
		{
			camera.IsMovementAllowed = true;
			data.Damage.InvincibilityTimer = 60f;
			return;
		}
		
		data.Node.Position = new Vector2(sbyte.MaxValue, 0);
		data.Node.ZIndex = (int)Constants.ZIndexes.AboveForeground;
		
		data.Cpu.State = States.RespawnInit;
		data.Movement.IsControlRoutineEnabled = false;
		data.Collision.IsObjectInteractionEnabled = false;
		data.Movement.IsGrounded = false;
	}
}
