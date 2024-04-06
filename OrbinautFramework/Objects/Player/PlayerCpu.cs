using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.InputModule;
using Camera = OrbinautFramework3.Framework.View.Camera;

namespace OrbinautFramework3.Objects.Player;

public partial class PlayerCpu : Player
{
	private const int DelayStep = 16;
	
	private bool _canReceiveInput;
	private ICpuTarget _leadPlayer;
	private int _delay;
	
	public override void _ExitTree()
	{
		base._ExitTree();
		
		if (Players.Count == 0 || !IsCpuRespawn) return;
		//TODO: check respawn Player cpu
		/*
		var newPlayer = new Player
		{
			Type = Type,
			Position = Players.First().Position
		};

		newPlayer._Process(FrameworkData.ProcessSpeed / Constants.BaseFramerate);
		*/
	}
	
    protected override void ProcessCpu(float processSpeed)
	{
		if (IsHurt || IsDead || Id == 0) return;
		
		_leadPlayer = Players[0];
		_delay = DelayStep * Id;
		
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

	private bool InitRespawnCpu()
	{
		// This forces player to respawn instantly if they're holding any button
		if (_canReceiveInput && Input.Down is { Abc: false, Start: false })
		{
			if (!FrameworkData.IsTimePeriodLooped(64f) || !CpuTarget.ObjectInteraction) return false;
		}
		
		Position = _leadPlayer.Position - new Vector2(0f, SharedData.ViewSize.Y - 32);
		
		CpuState = CpuStates.Respawn;
		return false;
	}

	private bool ProcessRespawnCpu()
	{
		if (CheckIfCpuOffscreen()) return false;
		
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
		
		RecordedData followData = _leadPlayer.FollowData;

		Vector2 distance = Position - followData.Position;
		distance.Y *= -1f;
		distance = distance.Floor();
		
		Position += new Vector2(GetRespawnVelocityX(ref distance.X), Math.Sign(distance.Y));

		if (_leadPlayer.IsDead || followData.Position.Y < 0f || distance == Vector2.Zero) return true;
		
		CpuState = CpuStates.Main;
		Animation = Animations.Move;
		Velocity.Vector = Vector2.Zero;
		GroundSpeed.Value = 0f;
		GroundLockTimer = 0f;
		ObjectInteraction = true;
		
		return true;
	}

	private void PlayTailsSound()
	{
		if (!FrameworkData.IsTimePeriodLooped(16f, 8f) || !Sprite.CheckInCamera() || IsUnderwater) return;

		if (CpuState == CpuStates.Respawn)
		{
			if (SharedData.CpuBehaviour != CpuBehaviours.S3) return;
			AudioPlayer.Sound.Play(SoundStorage.Flight);
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
	
	private bool ProcessMainCpu()
	{
		if (CheckIfCpuOffscreen()) return true;

		CpuTarget ??= Players[Id - 1];
		
		RecordedData followData = CpuTarget.FollowData;
		
		//TODO: ZIndex
		//depth = _player1.depth + player_id;
		
		if (CpuInputTimer > 0f)
		{
			CpuInputTimer--;
			if (!Input.NoControl) return false;
		}
		
		if (CarryTarget != null || Action == Actions.Carried) return false;
		
		if (GroundLockTimer > 0f && GroundSpeed == 0f)
		{
			CpuState = CpuStates.Stuck;
		}
		
		if (CpuTarget.Action == Actions.PeelOut)
		{
			followData.InputDown = followData.InputPress = new Buttons();
		}
		
		if (SharedData.PlayerPhysics >= PhysicsTypes.S3)
		{
			if (Math.Abs(CpuTarget.GroundSpeed) < 4f && CpuTarget.OnObject == null)
			{
				followData.Position.X -= 32f;
			}
		}
		
		var doJump = true;
		
		// TODO: AI is pushing weirdly rn
		if (SetPushAnimationBy == null || followData.SetPushAnimationBy != null)
		{
			int distanceX = Mathf.FloorToInt(followData.Position.X - Position.X);
			PushCpu(distanceX, ref followData);
			doJump = CheckCpuJump(distanceX, ref followData);
		}
		
		if (doJump && Animation != Animations.Duck && FrameworkData.IsTimePeriodLooped(64f))
		{
			followData.InputPress.Abc = followData.InputDown.Abc = true;
			IsCpuJumping = true;
		}
		
		Input.Set(followData.InputPress, followData.InputDown);
		return false;
	}
	
	private void PushCpu(float distanceX, ref RecordedData followData)
	{
		if (distanceX == 0f)
		{
			Facing = followData.Facing;
			return;
		}
		
		int maxDistanceX = SharedData.PlayerPhysics > PhysicsTypes.S3 ? 48 : 16;

		bool isMoveToRight = distanceX > 0f;
		int sign = isMoveToRight ? 1 : -1;
		if (sign * distanceX > maxDistanceX)
		{
			followData.InputDown.Left = followData.InputPress.Left = !isMoveToRight;
			followData.InputDown.Right = followData.InputPress.Right = isMoveToRight;
		}
						
		if (GroundSpeed != 0f && Facing == (Constants.Direction)sign)
		{
			Position += Vector2.Right * sign;
		}
	}
	
	private bool CheckCpuJump(float distanceX, ref RecordedData followData)
	{
		if (IsCpuJumping)
		{
			followData.InputDown.Abc = true;
				
			if (!IsGrounded) return false;
			IsCpuJumping = false;

			return true;
		}
		
		if (Math.Abs(distanceX) > 64 && !FrameworkData.IsTimePeriodLooped(256f)) return false;
		return Mathf.FloorToInt(followData.Position.Y - Position.Y) <= -32;
	}
	
	private bool ProcessStuckCpu()
	{
		if (CheckIfCpuOffscreen()) return true;
		
		if (GroundLockTimer > 0f || CpuInputTimer > 0f || GroundSpeed != 0f) return false;
		
		if (Animation == Animations.Idle)
		{
			Facing = MathF.Floor(CpuTarget.Position.X - Position.X) > 0f ? 
				Constants.Direction.Positive : Constants.Direction.Negative;
		}
		
		if (!FrameworkData.IsTimePeriodLooped(128f))
		{
			Input.Down = Input.Down with { Down = true };
			if (!FrameworkData.IsTimePeriodLooped(32f)) return false;
			Input.Press = Input.Press with { Abc = true };
			
			return false;
		}
		
		Input.Down = Input.Down with { Down = false };
		Input.Press = Input.Press with { Abc = false };
		CpuState = CpuStates.Main;
		
		return false;
	}
	
	private bool CheckIfCpuOffscreen()
	{
		//TODO: check "camera_get(0).bound_right - x < 0" == "Camera.Main.Bounds.Z < Position.X"
		if (Sprite != null && Sprite.CheckInCamera() || Camera.Main.Bounds.Z < Position.X)
		{
			CpuTimer = 0f;
			return false;
		}
		
		CpuTimer += FrameworkData.ProcessSpeed;
		//TODO: check IsInstanceValid == instance_exists
		if (CpuTimer < 300f && (OnObject == null || IsInstanceValid(OnObject))) return false;
		Respawn();
		return true;
	}

	private void Respawn()
	{
		Reset();
		
		Position = new Vector2(sbyte.MaxValue, 0);
		
		CpuState = CpuStates.RespawnInit;
		IsRunControlRoutine = false;
		ObjectInteraction = false;
		IsGrounded = false;
		
		ZIndex = (int)Constants.ZIndexes.AboveForeground; 
	}
}
