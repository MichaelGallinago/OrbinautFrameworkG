using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Input;

namespace OrbinautFramework3.Objects.Player;

public partial class PlayerCpu : Player
{
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
		const byte cpuDelay = 16;
		
		if (IsHurt || Id == 0) return false;
		
		// Find a player to follow
		CpuTarget ??= Players[Id - 1];
		
		if (RecordedData.Count < cpuDelay) return false;
		
		// Read actual player input and disable AI for 10 seconds if detected it
		if (Input.Down.Abc || Input.Down.Up || Input.Down.Down || Input.Down.Left || Input.Down.Right)
		{
			CpuInputTimer = 600f;
		}

		return CpuState switch
		{
			CpuStates.RespawnInit => InitRespawnCpu(),
			CpuStates.Respawn => ProcessRespawnCpu(RecordedData[^cpuDelay]),
			CpuStates.Main => ProcessMainCpu(RecordedData[^cpuDelay]),
			CpuStates.Stuck => ProcessStuckCpu(),
			_ => false
		};
	}

	private bool InitRespawnCpu()
	{
		// ???
		if (Input.Down is { Abc: false, Start: false })
		{
			if (!FrameworkData.IsTimePeriodLooped(64f) || !CpuTarget.ObjectInteraction) return false;
		}
				
		//TODO: ZIndex;
		//depth = 75;
		Position = (Vector2I)CpuTarget.Position - new Vector2I(16, 192);
				
		ObjectInteraction = false;
		CpuState = CpuStates.Respawn;
		return false;
	}

	private bool ProcessRespawnCpu(RecordedData followData)
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
				
		float distanceX = Position.X - followData.Position.X;
		if (distanceX != 0f)
		{
			float velocityX = 1f + Math.Abs(CpuTarget.Speed.X) + Math.Min(MathF.Floor(Math.Abs(distanceX) / 16), 12);
					
			if (distanceX >= 0f)
			{
				Facing = Constants.Direction.Negative;
						
				if (velocityX >= distanceX)
				{
					velocityX = -distanceX;
					distanceX = 0f;
				}
				else
				{
					velocityX = -velocityX;
				}
			}
			else
			{
				Facing = Constants.Direction.Positive;
				distanceX = -distanceX;
						
				if (velocityX >= distanceX)
				{
					velocityX  = -distanceX;
					distanceX = 0;
				}
			}
					
			Position += new Vector2(velocityX, 0f);
		}
				
		float distanceY = Mathf.FloorToInt(followData.Position.Y - Position.Y);
		if (distanceY != 0)
		{
			Position += new Vector2(0f, Math.Sign(distanceY));
		}
				
		if (!CpuTarget.IsDead && followData.Position.Y >= 0 && distanceX == 0 && distanceY == 0)
		{
			CpuState = CpuStates.Main;
			Animation = Animations.Move;
			Speed.Vector = Vector2.Zero;
			GroundSpeed = 0f;
			GroundLockTimer = 0f;
			ObjectInteraction = true;
			ResetZIndex();
			ResetGravity();
			ResetState();
		}
		else
		{
			Input.Clear();
		}
				
		// Exit the entire player object code
		return true;
	}

	private bool ProcessMainCpu(RecordedData followData)
	{
		if (RespawnCpu()) return true;
		//TODO: check if behind player and main player tail
		ZIndex = CpuTarget.ZIndex;
		
		if (CpuInputTimer > 0f)
		{
			CpuInputTimer--;
			return false;
		}
		
		if (CarryTarget != null || Action == Actions.Carried) return false;

		if (GroundLockTimer != 0f && GroundSpeed == 0f)
		{
			CpuState = CpuStates.Stuck;
		}
		
		if (CpuTarget.Action == Actions.PeelOut)
		{
			followData.InputDown = new Buttons();
			followData.InputPress = new Buttons();
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
			if (distanceX != 0)
			{
				int maxDistanceX = SharedData.PlayerPhysics > PhysicsTypes.S3 ? 48 : 16;
				
				if (distanceX > 0)
				{
					if (distanceX > maxDistanceX)
					{
						followData.InputDown.Left = false;
						followData.InputPress.Left = false;
						followData.InputDown.Right = true;
						followData.InputPress.Right = true;
					}
						
					if (GroundSpeed != 0f && Facing == Constants.Direction.Positive)
					{
						Position += Vector2.Right;
					}
				}
				else
				{
					if (distanceX < -maxDistanceX)
					{
						followData.InputDown.Left = true;
						followData.InputPress.Left = true;
						followData.InputDown.Right = false;
						followData.InputPress.Right = false;
					}
					
					if (GroundSpeed != 0f && Facing == Constants.Direction.Negative)
					{
						Position += Vector2.Left;
					}
				}
			}
			else
			{
				Facing = followData.Facing;
			}
				
			if (!IsCpuJumping)
			{
				if (Math.Abs(distanceX) > 64 && !FrameworkData.IsTimePeriodLooped(256f))
				{
					doJump = false;
				}
				else
				{
					if (Mathf.FloorToInt(followData.Position.Y - Position.Y) > -32)
					{
						doJump = false;
					}
				}
			}
			else
			{
				followData.InputDown.Abc = true;
				
				if (IsGrounded)
				{
					IsCpuJumping = false;
				}
				else
				{
					doJump = false;
				}
			}
		}
		
		if (doJump && Animation != Animations.Duck && FrameworkData.IsTimePeriodLooped(64f))
		{
			followData.InputPress.Abc = true;
			followData.InputDown.Abc = true;
			IsCpuJumping = true;
		}
		
		Input.Set(followData.InputPress, followData.InputDown);
		return false;
	}

	private bool ProcessStuckCpu()
	{
		if (RespawnCpu()) return true;
				
		if (GroundLockTimer != 0f || CpuInputTimer != 0f || GroundSpeed != 0f) return false;
				
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

		if (++CpuTimer < 300f) return false;
		Reset();
		return true;
	}
}
