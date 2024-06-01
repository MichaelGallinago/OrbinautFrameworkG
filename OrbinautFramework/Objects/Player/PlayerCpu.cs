using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.InputModule;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.View;

namespace OrbinautFramework3.Objects.Player;

public partial class PlayerCpu : Player
{
	private bool _canReceiveInput;
	private ICpuTarget _leadPlayer;
	private Buttons _cpuInputDown;
	private Buttons _cpuInputPress;
	private int _delay;
	
    protected override void ProcessCpu()
    {
		if (IsHurt || IsDead || Id == 0) return;
		
		_leadPlayer = Scene.Local.Players.First();
		_delay = CpuDelayStep * Id;
		
		// Read actual player input and enable manual control for 10 seconds if detected it
		_canReceiveInput = Id < Constants.MaxInputDevices;
		
		if (_canReceiveInput && Input.Down is not { Abc: false, Up: false, Down: false, Left: false, Right: false })
		{
			CpuInputTimer = 600f;
		}
		
		switch (CpuState)
		{
			case CpuStates.RespawnInit: InitRespawnCpu(); break;
			case CpuStates.Respawn: ProcessRespawnCpu(); break;
			case CpuStates.Main: ProcessMainCpu(); break;
			case CpuStates.Stuck: ProcessStuckCpu(); break;
		}
	}

	protected override void SetNextStateOnDeath()
	{
		// If CPU, respawn
		if (Id != 0)
		{
			Respawn();
			return;
		}
		base.SetNextStateOnDeath();
	}

	private void InitRespawnCpu()
	{
		// Take some time (up to 64 frames (on 60 fps))
		// to respawn or do not respawn at all unless holding down any button
		if (_canReceiveInput && Input.Down is { Abc: false, Start: false })
		{
			if (!Scene.Local.IsTimePeriodLooped(64f) || !_leadPlayer.IsObjectInteractionEnabled) return;
		}
		
		// Enable CPU's camera back
		if (IsCameraTarget(out ICamera camera))
		{
			camera.IsMovementAllowed = true;
		}
		
		Position = _leadPlayer.Position - new Vector2(0f, SharedData.ViewSize.Y - 32);
		
		CpuState = CpuStates.Respawn;
	}

	private void ProcessRespawnCpu()
	{
		if (CheckIfCpuOffscreen()) return;
		
		// Force animation & play sound
		switch (Type)
		{
			case Types.Sonic or Types.Amy: 
				Animation = Animations.Spin; 
				break;
			
			case Types.Tails: 
				Animation = IsUnderwater ? Animations.Swim : Animations.Fly;
				PlayTailsSound();
				break;
			
			case Types.Knuckles:
				Animation = Animations.GlideAir;
				break;
		}
		
		DataRecord followDataRecord = _leadPlayer.RecordedData[_delay];
		Vector2 targetPosition = followDataRecord.Position;

		if (SharedData.CpuBehaviour == CpuBehaviours.S2)
		{
			if (Stage.Local != null && Stage.Local.IsWaterEnabled)
			{
				targetPosition.Y = Math.Min(Stage.Local.WaterLevel - 16, targetPosition.Y);
			}
		}
		
		if (MoveToLeadPlayer(targetPosition)) return;

		if (SharedData.CpuBehaviour == CpuBehaviours.S3 && _leadPlayer.IsDead) return;

		if (!IsGrounded || !Mathf.IsEqualApprox(targetPosition.Y, followDataRecord.Position.Y)) return;
		
		CpuState = CpuStates.Main;
		Animation = Animations.Move;
		IsObjectInteractionEnabled = true;
		IsControlRoutineEnabled = true;
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
			velocityX *= Scene.Local.ProcessSpeed;

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
			positionOffset.Y = Math.Sign(distance.Y) * Scene.Local.ProcessSpeed;
		}

		Position += positionOffset;
		
		return distance.X != 0 || distance.Y != 0;
	}
	
	private void ProcessMainCpu()
	{
		FreezeOrFlyIfLeaderDied();
		
		if (CheckIfCpuOffscreen()) return; // Exit if respawned
		
		if (!IsObjectInteractionEnabled || CarryTarget != null || Action == Actions.Carried) return;
		
		CpuTarget ??= _leadPlayer; // Follow lead player
		
		// Do not run CPU follow logic while under manual control and input is allowed
		if (CpuInputTimer > 0f)
		{
			CpuInputTimer -= Scene.Local.ProcessSpeed;
			if (!Input.NoControl) return;
		}
		
		//TODO: check that ZIndex works
		ZIndex = _leadPlayer.ZIndex + Id;
		
		if (GroundLockTimer > 0f && GroundSpeed == 0f)
		{
			CpuState = CpuStates.Stuck;
		}
		
		(Vector2 targetPosition,_cpuInputPress, _cpuInputDown, 
			Constants.Direction direction, BaseObject setPushAnimationBy) = CpuTarget.RecordedData[_delay];

		if (SharedData.CpuBehaviour == CpuBehaviours.S3 &&
		    Math.Abs(CpuTarget.GroundSpeed) < 4f && CpuTarget.OnObject == null)
		{
			targetPosition.X -= 32f;
		}

		var doJump = true;
		if (SetPushAnimationBy == null || setPushAnimationBy != null)
		{
			int distanceX = Mathf.FloorToInt(targetPosition.X - Position.X);
			PushCpu(distanceX, direction);
			doJump = CheckCpuJump(distanceX, targetPosition.Y);
		}
		
		if (doJump && Animation != Animations.Duck && Scene.Local.IsTimePeriodLooped(64f))
		{
			_cpuInputPress.Abc = _cpuInputDown.Abc = true;
			IsCpuJumping = true;
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
			CpuState = CpuStates.Respawn;
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
		
		int maxDistanceX = SharedData.PlayerPhysics == PhysicsTypes.S3 ? 48 : 16;

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
		if (IsCpuJumping)
		{
			_cpuInputDown.Abc = true;
				 
			if (!IsGrounded) return false;
			
			IsCpuJumping = false;
			return true;
		}
		
		if (distanceX >= 64f && !Scene.Local.IsTimePeriodLooped(256f)) return false;
		return targetPositionY - Position.Y <= -32;
	}
	
	private void ProcessStuckCpu()
	{
		if (CheckIfCpuOffscreen()) return;
		
		if (GroundLockTimer > 0f || CpuInputTimer > 0f || GroundSpeed != 0f) return;
		
		if (Animation == Animations.Idle)
		{
			Facing = CpuTarget.Position.X >= Position.X ? Constants.Direction.Positive : Constants.Direction.Negative;
		}
		
		if (!Scene.Local.IsTimePeriodLooped(128f))
		{
			Input.Down = Input.Down with { Down = true };
			if (!Scene.Local.IsTimePeriodLooped(32f)) return;
			Input.Press = Input.Press with { Abc = true };
			
			return;
		}
		
		Input.Down = Input.Down with { Down = false };
		Input.Press = Input.Press with { Abc = false };
		CpuState = CpuStates.Main;
	}
	
	private bool CheckIfCpuOffscreen()
	{
		bool isLeadPlayerCameraTarget = _leadPlayer.IsCameraTarget(out ICamera camera);
		
		if (CpuInputTimer > 0 || Sprite != null && (isLeadPlayerCameraTarget ? 
		    Sprite.CheckInCamera(camera) || camera.TargetBoundary.Z <= Position.X : Sprite.CheckInCameras()))
		{
			CpuTimer = 0f;
			return false;
		}
		
		CpuTimer += Scene.Local.ProcessSpeed;
		//TODO: check IsInstanceValid == instance_exists
		// Wait 300 steps unless standing on an object that got respawned
		if (CpuTimer < 300f && (OnObject == null || IsInstanceValid(OnObject))) return false;
		Respawn();
		return true;
	}

	private void Respawn()
	{
		Init();
		
		Position = new Vector2(sbyte.MaxValue, 0);
		
		CpuState = CpuStates.RespawnInit;
		IsControlRoutineEnabled = false;
		IsObjectInteractionEnabled = false;
		IsGrounded = false;
		
		// Since we're teleporting CPU to the top left corner, temporary disable their camera
		if (IsCameraTarget(out ICamera camera))
		{
			camera.IsMovementAllowed = false;
		}
		
		ZIndex = (int)Constants.ZIndexes.AboveForeground; 
	}
}
