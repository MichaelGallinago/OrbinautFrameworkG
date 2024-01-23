using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Input;

namespace OrbinautFramework3.Objects.Player;

public partial class PlayerCpu : Player
{
	private bool _canReceiveInput;
	private ICpuTarget _mainPlayer;
	
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
	
    protected override bool ProcessCpu(float processSpeed)
	{
		if (IsHurt || Id == 0) return false;
		
		// Always have a reference to player 1
		_mainPlayer = Players[0];
		
		// Read actual player input and enable manual control for 10 seconds if detected it
		_canReceiveInput = Id < Constants.MaxInputDevices;
		
		//if (RecordedData.Count < cpuDelay) return false;
		
		// Read actual player input and enable manual control for 10 seconds if detected it
		if (_canReceiveInput && Input.Down is not { Abc: false, Up: false, Down: false, Left: false, Right: false })
		{
			CpuInputTimer = 600f;
		}

		return CpuState switch
		{
			CpuStates.RespawnInit => InitRespawnCpu(),
			CpuStates.Respawn => ProcessRespawnCpu(),
			CpuStates.Main => ProcessMainCpu(),
			CpuStates.Stuck => ProcessStuckCpu(),
			_ => false
		};
	}

	private bool InitRespawnCpu()
	{
		// This forces player to respawn instantly if they're holding any button
		if (_canReceiveInput)
		{
			if (Input.Down is { Abc: false, Start: false })
			{
				if (!FrameworkData.IsTimePeriodLooped(64f) || !CpuTarget.ObjectInteraction) return false;
			}
		}
		
		Position = _mainPlayer.Position - new Vector2(0f, SharedData.ViewSize.Y - 32);
		
		ZIndex = (int)Constants.ZIndexes.AboveForeground;
		CpuState = CpuStates.Respawn;
		ObjectInteraction = false;
		return false;
	}

	private bool ProcessRespawnCpu()
	{
		if (!RespawnCpu())
		{
			if (Type == Types.Tails)
			{
				Animation = Animations.Fly;
			}
					
			OnObject = null;
			IsGrounded = false;
					
			// Run animation script since we exit the entire player object code later
			Sprite.Animate(this);
		}
		
		RecordedData followData = _mainPlayer.FollowData;

		Vector2 distance = Position - followData.Position;
		distance.Y *= -1f;
		distance = distance.Floor();
		
		Position += new Vector2(GetRespawnVelocityX(ref distance.X), Math.Sign(distance.Y));

		if (_mainPlayer.IsDead || followData.Position.Y < 0f || distance == Vector2.Zero) return true;
		
		CpuState = CpuStates.Main;
		Animation = Animations.Move;
		Speed.Vector = Vector2.Zero;
		GroundSpeed = 0f;
		GroundLockTimer = 0f;
		ObjectInteraction = true;
		
		ResetGravity();
		ResetState();
		
		return true;
	}

	private float GetRespawnVelocityX(ref float distanceX)
	{
		if (distanceX == 0f) return 0f;
		
		float velocityX = Math.Abs(CpuTarget.Speed.X) + Math.Min(MathF.Floor(Math.Abs(distanceX) / 16f), 12f) + 1f;
	
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
		if (RespawnCpu()) return true;

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
		if (PushingObject == null || followData.PushingObject != null)
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
		if (RespawnCpu()) return true;
		
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

	private bool RespawnCpu()
	{
		if (Sprite != null && Sprite.CheckInView())
		{
			CpuTimer = 0f;
			return false;
		}

		CpuTimer += FrameworkData.ProcessSpeed;
		if (CpuTimer < 300f) return false;
		Reset();
		return true;
	}
}
