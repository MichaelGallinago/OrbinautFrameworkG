using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.InputModule;

namespace OrbinautFramework3.Objects.Player;

public partial class PlayerCpu : Player
{
	private bool _canReceiveInput;
	private ICpuTarget _leadPlayer;
	private int _delay;
	
	public override void _ExitTree()
	{
		base._ExitTree();
		
		//TODO: check respawn Player cpu
		/*
		if (Players.Count == 0 || !IsCpuRespawn) return;
		var newPlayer = new Player
		{
			Type = Type,
			Position = Players.First().Position
		};

		newPlayer._Process(Scene.Local.ProcessSpeed / Constants.BaseFramerate);
		*/
	}
	
    protected override void ProcessCpu(float processSpeed)
	{
		if (IsHurt || IsDead || Id == 0) return;
		
		_leadPlayer = Players[0];
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

	private void InitRespawnCpu()
	{
		// Take some time (up to 64 frames (on 60 fps))
		// to respawn or do not respawn at all unless holding down any button
		if (_canReceiveInput && Input.Down is { Abc: false, Start: false })
		{
			if (!Scene.Local.IsTimePeriodLooped(64f) || !_leadPlayer.IsObjectInteractionEnabled) return;
		}
		
		// Enable CPU's camera back
		//TODO: wtf index?
		//if player_view.index > 0
		//{
		Camera.IsMovementAllowed = true;
		//}
		
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
		
		DataRecord followDataRecord = _leadPlayer.GetFollowDataRecord(_delay);
		Vector2 targetPosition = followDataRecord.Position;

		if (SharedData.CpuBehaviour == CpuBehaviours.S2 && Scene.Local.IsStage)
		{
			if (Scene.Local is Stage { IsWaterEnabled: true } stage)
			{
				targetPosition.Y = Math.Min(stage.WaterLevel - 16, targetPosition.Y);
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
			float velocityX = Math.Abs(_leadPlayer.Velocity.X) + Math.Min(Math.Abs(distance.X) / 16, 12) + 1;

			if (distance.X >= 0)
			{
				if (velocityX < distance.X)
				{
					velocityX *= -1;
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
				velocityX *= -1;

				if (velocityX >= distance.X)
				{
					velocityX = distance.X;
					distance.X = 0;
				}

				Facing = Constants.Direction.Positive;
			}

			positionOffset.X = velocityX * Scene.Local.ProcessSpeed;
		}

		if (distance.Y != 0)
		{
			positionOffset.Y = Math.Sign(distance.Y) * Scene.Local.ProcessSpeed;
		}

		Position += positionOffset;

		return distance.X != 0 || distance.Y != 0;
	}

	private void PlayTailsSound()
	{
		if (!Scene.Local.IsTimePeriodLooped(16f, 8f) || !Sprite.CheckInCameras() || IsUnderwater) return;

		if (CpuState == CpuStates.Respawn)
		{
			if (SharedData.CpuBehaviour != CpuBehaviours.S3) return;
			AudioPlayer.Sound.Play(SoundStorage.Flight);
			return;
		}
		
		if (ActionValue > 0f)
		{
			AudioPlayer.Sound.Play(SoundStorage.Flight);
			return;
		}
		
		AudioPlayer.Sound.Play(SoundStorage.Flight2);
	}

	private float GetRespawnVelocityX(ref float distanceX)
	{
		if (distanceX == 0f) return 0f;
		
		float velocityX = Math.Abs(CpuTarget.Velocity.X) + Math.Min(MathF.Floor(Math.Abs(distanceX) / 16f), 12f) + 1f;
	
		Facing = distanceX >= 0f ? Constants.Direction.Negative : Constants.Direction.Positive;
		var sign = (int)Facing;
		distanceX *= sign;
		
		if (velocityX >= -distanceX)
		{
			velocityX = distanceX;
			distanceX = 0f;
		}
		else
		{
			velocityX *= sign;
		}

		return velocityX;
	}
	
	private void ProcessMainCpu()
	{
		FreezeOrFlyIfLeaderDied();
		if (CheckIfCpuOffscreen()) return;
		if (!IsObjectInteractionEnabled || CarryTarget != null || Action == Actions.Carried) return;
		
		CpuTarget ??= Players[Id - 1];
		
		DataRecord followDataRecord = CpuTarget.GetFollowDataRecord();
		
		//TODO: ZIndex
		//depth = _player1.depth + player_id;
		
		if (CpuInputTimer > 0f)
		{
			CpuInputTimer--;
			if (!Input.NoControl) return;
		}
		
		if (CarryTarget != null || Action == Actions.Carried) return;
		
		if (GroundLockTimer > 0f && GroundSpeed == 0f)
		{
			CpuState = CpuStates.Stuck;
		}
		
		if (CpuTarget.Action == Actions.PeelOut)
		{
			followDataRecord.InputDown = followDataRecord.InputPress = new Buttons();
		}
		
		if (SharedData.PlayerPhysics >= PhysicsTypes.S3)
		{
			if (Math.Abs(CpuTarget.GroundSpeed) < 4f && CpuTarget.OnObject == null)
			{
				followDataRecord.Position.X -= 32f;
			}
		}
		
		var doJump = true;
		
		// TODO: AI is pushing weirdly rn
		if (SetPushAnimationBy == null || followDataRecord.SetPushAnimationBy != null)
		{
			int distanceX = Mathf.FloorToInt(followDataRecord.Position.X - Position.X);
			PushCpu(distanceX, ref followDataRecord);
			doJump = CheckCpuJump(distanceX, followDataRecord);
		}
		
		if (doJump && Animation != Animations.Duck && Scene.Local.IsTimePeriodLooped(64f))
		{
			followDataRecord.InputPress.Abc = followDataRecord.InputDown.Abc = true;
			IsCpuJumping = true;
		}
		
		Input.Set(followDataRecord.InputPress, followDataRecord.InputDown);
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
		else // Force-disable animation
		{
			data_ani.timer = -1;
		}
					
		IsControlRoutineEnabled = false;
	}
	
	private void PushCpu(float distanceX, ref DataRecord followDataRecord)
	{
		if (distanceX == 0f)
		{
			Facing = followDataRecord.Facing;
			return;
		}
		
		int maxDistanceX = SharedData.PlayerPhysics > PhysicsTypes.S3 ? 48 : 16;

		bool isMoveToRight = distanceX > 0f;
		int sign = isMoveToRight ? 1 : -1;
		if (sign * distanceX > maxDistanceX)
		{
			followDataRecord.InputDown.Left = followDataRecord.InputPress.Left = !isMoveToRight;
			followDataRecord.InputDown.Right = followDataRecord.InputPress.Right = isMoveToRight;
		}
						
		if (GroundSpeed != 0f && Facing == (Constants.Direction)sign)
		{
			Position += Vector2.Right * sign;
		}
	}
	
	private bool CheckCpuJump(float distanceX, DataRecord followDataRecord)
	{
		if (IsCpuJumping)
		{
			followDataRecord.InputDown.Abc = true;
				
			if (!IsGrounded) return false;
			IsCpuJumping = false;

			return true;
		}
		
		if (Math.Abs(distanceX) > 64 && !Scene.Local.IsTimePeriodLooped(256f)) return false;
		return Mathf.FloorToInt(followDataRecord.Position.Y - Position.Y) <= -32;
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
		if (Sprite != null && Sprite.CheckInCamera(_leadPlayer.Camera) || Camera.Bound.Z < Position.X)
		{
			CpuTimer = 0f;
			return false;
		}
		
		CpuTimer += Scene.Local.ProcessSpeed;
		//TODO: check IsInstanceValid == instance_exists
		// Wait 300 steps unless standing on an object that got despawned
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
		//TODO: wtf index?
		//if player_view.index > 0
		//{
		Camera.IsMovementAllowed = false;
		//}
		
		ZIndex = (int)Constants.ZIndexes.AboveForeground; 
	}
}
