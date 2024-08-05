using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.InputModule;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.View;

namespace OrbinautFramework3.Objects.Player.Modules;

public class CpuModule
{
	public const int DelayStep = 16;
	
	public enum States : byte
	{
		RespawnInit, Respawn, Main, Fly, Stuck
	}
	
	public enum Behaviours : byte
	{
		S2, S3
	}
	
    public States State { get; set; } = States.Main;
    public float RespawnTimer { get; set; }
    public float InputTimer { get; set; }
    public bool IsJumping { get; set; }
    public bool IsRespawn { get; set; }
    public ICpuTarget Target { get; set; }
    
    private bool _canReceiveInput;
	private ICpuTarget _leadPlayer;
	private Buttons _cpuInputDown;
	private Buttons _cpuInputPress;
	private int _delay;
	
    public void Process()
    {
		if (IsHurt || IsDead || Id == 0) return;
		
		_leadPlayer = Scene.Instance.Players.First();
		_delay = DelayStep * Id;
		
		// Read actual player input and enable manual control for 10 seconds if detected it
		_canReceiveInput = Id < Constants.MaxInputDevices;
		
		if (_canReceiveInput && Input.Down is not { Abc: false, Up: false, Down: false, Left: false, Right: false })
		{
			InputTimer = 600f;
		}
		
		switch (State)
		{
			case States.RespawnInit: InitRespawnCpu(); break;
			case States.Respawn: ProcessRespawnCpu(); break;
			case States.Main: ProcessMainCpu(); break;
			case States.Stuck: ProcessStuckCpu(); break;
		}
	}

	private void InitRespawnCpu()
	{
		// Take some time (up to 64 frames (on 60 fps))
		// to respawn or do not respawn at all unless holding down any button
		if (_canReceiveInput && Input.Down is { Abc: false, Start: false })
		{
			if (!Scene.Instance.IsTimePeriodLooped(64f) || !_leadPlayer.IsObjectInteractionEnabled) return;
		}
		
		Position = _leadPlayer.Position - new Vector2(0f, SharedData.ViewSize.Y - 32);
		
		State = States.Respawn;
	}

	private void ProcessRespawnCpu()
	{
		if (CheckCpuRespawn()) return;
		
		// Force animation & play sound
		switch (Type)
		{
			case Types.Sonic or Types.Amy: 
				Animation = Animations.Spin; 
				break;
			
			case Types.Tails: 
				Animation = IsUnderwater ? Animations.Swim : Animations.Fly;
				PlayRespawnFlyingSound();
				break;
			
			case Types.Knuckles:
				Animation = Animations.GlideAir;
				break;
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

		if (!IsGrounded || !Mathf.IsEqualApprox(targetPosition.Y, followDataRecord.Position.Y)) return;
		
		State = States.Main;
		Animation = Animations.Move;
		IsObjectInteractionEnabled = true;
		IsControlRoutineEnabled = true;
	}

	private void PlayRespawnFlyingSound()
	{
		if (!Scene.Instance.IsTimePeriodLooped(16f, 8f) || !Sprite.CheckInCameras() || IsUnderwater) return;
		if (SharedData.Behaviour != Behaviours.S3) return;
		AudioPlayer.Sound.Play(SoundStorage.Flight);
	}

	private bool MoveToLeadPlayer(Vector2 targetPosition)
	{
		var distance = new Vector2I(
			Mathf.FloorToInt(Position.X - targetPosition.X),
			Mathf.FloorToInt(targetPosition.Y - Position.Y));

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

				Facing = Constants.Direction.Negative;
			}
			else
			{
				velocityX = -velocityX;

				if (velocityX >= distance.X)
				{
					velocityX = distance.X;
					distance.X = 0;
				}

				Facing = Constants.Direction.Positive;
			}

			positionOffset.X = velocityX;
		}

		if (distance.Y != 0)
		{
			positionOffset.Y = Math.Sign(distance.Y) * Scene.Instance.ProcessSpeed;
		}

		Position += positionOffset;
		
		return distance.X != 0 || distance.Y != 0;
	}
	
	private void ProcessMainCpu()
	{
		FreezeOrFlyIfLeaderDied();
		
		if (CheckCpuRespawn()) return; // Exit if respawned
		
		if (!IsObjectInteractionEnabled || CarryTarget != null || Action == Actions.Carried) return;
		
		CpuTarget ??= _leadPlayer; // Follow lead player
		
		// Do not run CPU follow logic while under manual control and input is allowed
		if (InputTimer > 0f)
		{
			InputTimer -= Scene.Instance.ProcessSpeed;
			if (!Input.NoControl) return;
		}
		
		//TODO: check that ZIndex works
		ZIndex = _leadPlayer.ZIndex + Id;
		
		if (GroundLockTimer > 0f && GroundSpeed == 0f)
		{
			State = States.Stuck;
		}
		
		(Vector2 targetPosition, _cpuInputPress, _cpuInputDown, 
			Constants.Direction direction, OrbinautData setPushAnimationBy) = CpuTarget.RecordedData[_delay];

		if (SharedData.Behaviour == Behaviours.S3 &&
		    Math.Abs(CpuTarget.GroundSpeed) < 4f && CpuTarget.OnObject == null)
		{
			targetPosition.X -= 32f;
		}

		// Copy and modify inputs if we are not pushing anything or
		// if the followed player was pushing something a few frames ago
		bool doJump = Animation != Animations.Duck && CpuTarget.Animation != Animations.Wait;
		if (SetPushAnimationBy == null || setPushAnimationBy != null)
		{
			int distanceX = Mathf.FloorToInt(targetPosition.X - Position.X);
			PushCpu(distanceX, direction);
			doJump = CheckCpuJump(distanceX, targetPosition.Y);
		}
		
		// Jump
		if (doJump && Scene.Instance.IsTimePeriodLooped(64f))
		{
			_cpuInputPress.Abc = _cpuInputDown.Abc = true;
			IsJumping = true;
		}
		
		Input.Set(_cpuInputPress, _cpuInputDown);
	}

	private void FreezeOrFlyIfLeaderDied()
	{
		// Freeze or start flying (if we're Tails) if lead player has died
		if (!_leadPlayer.IsDead) return;
		
		if (Type == Types.Tails)
		{
			Animation = Animations.Fly;
			State = States.Respawn;
			ResetState();
		}
		else
		{
			//TODO: instance_deactivate_object(id);
		}
					
		IsControlRoutineEnabled = false;
	}
	
	private void PushCpu(float distanceX, Constants.Direction facing)
	{
		if (distanceX == 0f)
		{
			Facing = facing;
			return;
		}
		
		int maxDistanceX = SharedData.PhysicsType == PhysicsTypes.S3 ? 48 : 16;

		bool isMoveToRight = distanceX > 0f;
		int sign = isMoveToRight ? 1 : -1;
		if (sign * distanceX > maxDistanceX)
		{
			_cpuInputDown.Left = _cpuInputPress.Left = !isMoveToRight;
			_cpuInputDown.Right = _cpuInputPress.Right = isMoveToRight;
		}
						
		if (GroundSpeed != 0f && IsControlRoutineEnabled && (int)Facing == sign)
		{
			Position += Vector2.Right * sign;
		}
	}
	
	private bool CheckCpuJump(float distanceX, float targetPositionY)
	{
		if (IsJumping)
		{
			_cpuInputDown.Abc = true;
				 
			if (!IsGrounded) return false;
			
			IsJumping = false;
			return true;
		}
		
		if (distanceX >= 64f && !Scene.Instance.IsTimePeriodLooped(256f)) return false;
		return targetPositionY - Position.Y <= -32;
	}
	
	private void ProcessStuckCpu()
	{
		if (CheckCpuRespawn()) return;
		
		if (GroundLockTimer > 0f || InputTimer > 0f || GroundSpeed != 0f) return;
		
		if (Animation == Animations.Idle)
		{
			Facing = CpuTarget.Position.X >= Position.X ? Constants.Direction.Positive : Constants.Direction.Negative;
		}
		
		if (!Scene.Instance.IsTimePeriodLooped(128f))
		{
			Input.Down = Input.Down with { Down = true };
			if (!Scene.Instance.IsTimePeriodLooped(32f)) return;
			Input.Press = Input.Press with { Abc = true };
			
			return;
		}
		
		Input.Down = Input.Down with { Down = false };
		Input.Press = Input.Press with { Abc = false };
		State = States.Main;
	}
	
	private bool CheckCpuRespawn()
	{
		bool isBehindLeader = _leadPlayer.IsCameraTarget(out ICamera camera) && camera.TargetBoundary.Z <= Position.X;
		if (isBehindLeader || Sprite != null && Sprite.CheckInCameras())
		{
			CpuRespawnTimer = 0f;
			return false;
		}
		
		//TODO: check IsInstanceValid == instance_exists
		CpuRespawnTimer += Scene.Instance.ProcessSpeed;
		if (CpuRespawnTimer < 300f && (OnObject == null || IsInstanceValid(OnObject))) return false;
		Respawn();
		return true;
	}

	private void Respawn()
	{
		Init();
		
		if (IsCameraTarget(out ICamera camera))
		{
			camera.IsMovementAllowed = true;
			InvincibilityTimer = 60f;
			return;
		}
		
		Position = new Vector2(sbyte.MaxValue, 0);
		ZIndex = (int)Constants.ZIndexes.AboveForeground;
		
		CpuState = States.RespawnInit;
		IsControlRoutineEnabled = false;
		IsObjectInteractionEnabled = false;
		IsGrounded = false;
	}
}
