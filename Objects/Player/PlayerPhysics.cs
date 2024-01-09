using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Spawnable.Barrier;
using static OrbinautFramework3.Objects.Player.PlayerConstants;

namespace OrbinautFramework3.Objects.Player;

public partial class Player
{
	private void UpdatePhysics()
	{
		if (Action is Actions.ObjectControl or Actions.Transform) return;

		// Define physics for this step
		PhysicParams = PhysicParams.Get(IsUnderwater, IsSuper, Type, ItemSpeedTimer);
		
		// Abilities logic
		if (ProcessSpinDash()) return;
		if (ProcessPeelOut()) return;
		if (ProcessJump()) return;
		if (StartJump()) return;
		
		ChargeDropDash();
		ProcessFlight();
		ProcessClimb();
		ProcessGlide();
		ChargeHammerSpin();
		ProcessHammerDash();
		
		// Core player logic
		ProcessSlopeResist();
		ProcessSlopeResistRoll();
		ProcessMovementGround();
		ProcessMovementGroundRoll();
		ProcessMovementAir();
		ProcessBalance();
		ProcessCollisionGroundWalls();
		ProcessRollStart();
		ProcessLevelBound();
		ProcessPosition();
		ProcessCollisionGroundFloor();
		ProcessSlopeRepel();
		ProcessCollisionAir();
		
		// Late abilities logic
		ProcessGlideCollision();
		ProcessCarry();
	}

	private bool ProcessSpinDash()
	{
		if (!SharedData.SpinDash || !IsGrounded) return false;
	
		// Start Spin Dash (initial charge)
		if (Action == Actions.None && Sprite.AnimationType is Animations.Duck or Animations.GlideLand)
		{
			if (!InputPress.Abc || !InputDown.Down) return false;
			
			Sprite.AnimationType = Animations.SpinDash;
			Action = Actions.SpinDash;
			ActionValue = 0;
			ActionValue2 = 1;
			Speed = new Vector2();
			
			// TODO: audio & SpinDash dust 
			//instance_create(x, y + Radius.Y, obj_dust_spindash, { TargetPlayer: id });
			//audio_play_sfx(sfx_charge);
		
			// Register next charge next frame
			return false;
		}
	
		// Continue if Spin Dash is being performed
		if (Action != Actions.SpinDash) return false;
	
		if (InputDown.Down)
		{
			if (InputPress.Abc)
			{
				ActionValue = Math.Min(ActionValue + 2, 8);
				
				// TODO: audio
				/*
				ActionValue2 = audio_is_playing(sfx_charge) && ActionValue > 0
					? Math.Min(ActionValue2 + 0.1f, 1.5f)
					: 1;
				
				
				var sound = audio_play_sfx(sfx_charge);
				audio_sound_pitch(sound, ActionValue2);
				*/
				Sprite.Frame = 0;
			}
			else
			{
				ActionValue -= MathF.Floor(ActionValue / 0.125f) / 256f;
			}
		}
		else
		{
			if (!SharedData.CDCamera && Id == 0)
			{
				Camera.Main.Delay.X = 16f;
			}
		
			int baseSpeed = IsSuper ? 11 : 8;
		
			Position += new Vector2(0f, Radius.Y - RadiusSpin.Y);
			Radius = RadiusSpin;
			Sprite.AnimationType = Animations.Spin;
			IsSpinning = true;
			Action = Actions.None;
			GroundSpeed = (baseSpeed + MathF.Round(ActionValue) / 2) * (float)Facing;
				
			if (SharedData.FixDashRelease)
			{
				float radians = Mathf.DegToRad(Angle);
				Speed = GroundSpeed * new Vector2(MathF.Cos(radians), -MathF.Sin(radians));
			}
			
			//TODO: audio
			//audio_stop_sfx(sfx_charge);
			//audio_play_sfx(sfx_release);
			
			// return player control routine
			return true;
		}
		
		return false;
	}
	
	private bool ProcessPeelOut()
	{
		if (!SharedData.PeelOut || Type != Types.Sonic || Id > 0 || !IsGrounded) return false;
	
		// Start Super Peel Out
		if (Action == Actions.None && Sprite.AnimationType == Animations.LookUp && InputDown.Up && InputPress.Abc)
		{
			Sprite.AnimationType = Animations.Move;
			Action = Actions.PeelOut;
			ActionValue = 0;
			ActionValue2 = 0;
			
			//TODO: audio
			//audio_play_sfx(sfx_charge2, [1.00, 2.30]);
		}
	
		// Continue if Super Peel Out is being performed
		if (Action != Actions.PeelOut) return false;
	
		float launchSpeed = PhysicParams.AccelerationTop * (ItemSpeedTimer > 0f || IsSuper ? 1.5f : 2f);
		if (InputDown.Up)
		{
			if (ActionValue < 30f)
			{
				ActionValue++;
			}
			
			ActionValue2 = Math.Clamp(ActionValue2 + 0.390625f * (float)Facing, -launchSpeed, launchSpeed);
			GroundSpeed = ActionValue2;
		}
		else 
		{
			//TODO: audio
			//audio_stop_sfx(sfx_charge2);
			Action = Actions.None;
		
			if (!Mathf.IsEqualApprox(ActionValue, 30f))
			{
				GroundSpeed = 0f;
			}
			else
			{
				if (!SharedData.CDCamera && Id == 0)
				{
					Camera.Main.Delay.X = 16f;
				}
			
				if (SharedData.FixDashRelease)
				{
					float radians = Mathf.DegToRad(Angle);
					Speed = GroundSpeed * new Vector2(MathF.Cos(radians), -MathF.Sin(radians));
				}
			
				//TODO: audio
				//audio_play_sfx(sfx_release2);
			
				// return player control routine
				return true;
			}
		}

		return false;
	}

	private bool ProcessJump()
	{
		if (!IsJumping || IsGrounded) return false;
		
		if (!InputDown.Abc)
		{
			Speed.Y = Math.Max(Speed.Y, PhysicParams.MinimalJumpVelocity);
		}
		
		if (Speed.Y < PhysicParams.MinimalJumpVelocity || Id > 0 && CpuInputTimer == 0) return false;
		
		if (InputPress.C && SharedData.EmeraldCount == 7 && !IsSuper && RingCount >= 50)
		{
			ResetState();
			//TODO: audio
			//audio_play_sfx(sfx_transform);
			//audio_play_bgm(bgm_super);
			//TODO: instance_create obj_star_super
			//instance_create(x, y, obj_star_super, { TargetPlayer: id });
				
			ObjectInteraction = false;			
			InvincibilityFrames = 0;
			IsSuper = true;
			Action = Actions.Transform;
			ActionValue = SharedData.PlayerPhysics >= PhysicsTypes.S3 ? 26 : 36;
			Sprite.AnimationType = Animations.Transform;
			Sprite.AnimationTimer = Type == Types.Sonic ? 39 : 36;
			
			// return player control routine
			return true;
		}
		
		switch (Type)
		{
			case Types.Sonic:
				if (SharedData.DropDash && Action == Actions.None && !InputDown.Abc)
				{
					if (Barrier.Type <= Barrier.Types.Normal || IsSuper)
					{
						Action = Actions.DropDash;
						ActionValue = 0;
					}
				}
				
				// Barrier abilities
				if (!InputPress.Abc || IsSuper || Barrier.State != Barrier.States.None || ItemInvincibilityTimer != 0) break;
				
				Barrier.State = Barrier.States.Active;
				IsAirLock = false;
				
				switch (Barrier.Type)
				{
					case Barrier.Types.None:
						if (!SharedData.DoubleSpin) break;
						
						//TODO: obj_double_spin
						/*
						with obj_double_spin
						{
							if TargetPlayer == other.id
							{
								instance_destroy();
							}
						}
						*/
						
						Barrier.State = Barrier.States.DoubleSpin;
						
						//TODO: audio & obj_double_spin
						//instance_create(x, y, obj_double_spin, { TargetPlayer: id });
						//audio_play_sfx(sfx_double_spin);
						break;
					
					case Barrier.Types.Water:
						Speed = new Vector2(0, 8);
						
						Barrier.UpdateFrame(0, 1, [1, 2]);
						Barrier.UpdateDuration([6, 18]);
						Barrier.AnimationTimer = 25f;
						
						//TODO: audio
						//audio_play_sfx(sfx_barrier_water2);
						break;
					
					case Barrier.Types.Flame:
						if (!SharedData.CDCamera)
						{
							Camera.Main.Delay.X = 16f;
						}
						
						IsAirLock = true;
						Speed = new Vector2(8f * (float)Facing, 0f);
							

						// TODO: SetAnimation
						//Barrier.SetAnimation(, [2]);
						ZIndex = -1;
						
						Barrier.AnimationTimer = 24f;
						
						//TODO: audio
						//audio_play_sfx(sfx_barrier_flame2);
						break;
					
					case Barrier.Types.Thunder:
						Barrier.State = Barrier.States.Disabled;
						Speed.Y = -5.5f;
						
						for (var i = 0; i < 4; i++)
						{
							//TODO: obj_barrier_sparkle
							//instance_create(x, y, obj_barrier_sparkle, { Sparkle_ID: i });
						}
						//TODO: audio
						//audio_play_sfx(sfx_barrier_thunder2);
						break;
				}
				break;
			
			case Types.Tails:
				if (Action > 0 || !InputPress.Abc) break;
				
				IsAirLock = false;
				IsSpinning = false;
				IsJumping = false;
				Gravity	= GravityType.TailsDown;
				Action = Actions.Flight;
				ActionValue = 480;
				
				Radius = RadiusNormal;
				
				if (!IsUnderwater)
				{
					//TODO: audio
					//audio_play_sfx(sfx_flight, true);
				}
					
				InputDown.Abc = false;
				InputPress.Abc = false;
				break;
			
			case Types.Knuckles:
				if (Action > 0 || !InputPress.Abc) break;
				
				IsAirLock = false;
				IsSpinning = false;
				IsJumping = false;	
				Sprite.AnimationType = Animations.GlideAir;	
				Action = Actions.Glide;
				ActionState = (int)GlideStates.Air;
				ActionValue = Facing == Constants.Direction.Negative ? 0f : 180f;
				Radius = new Vector2I(10, 10);
				GroundSpeed = 4f;
				Speed = new Vector2(0f, Speed.Y + 2f);
				
				if (Speed.Y < 0)
				{
					Speed.Y = 0;
				}
				break;
			
			case Types.Amy:
				if (Action > 0 || !InputPress.Abc) break;
				
				if (SharedData.NoRollLock)
				{
					IsAirLock = false;	
				}
				Sprite.AnimationType = Animations.HammerSpin;
				Action = Actions.HammerSpin;
				ActionValue = 0;
				// TODO: audio
				//audio_play_sfx(sfx_hammer_spin);
				break;
		}

		return false;
	}

	private bool StartJump()
	{
		if (Action is Actions.SpinDash or Actions.PeelOut || IsForcedRoll || !IsGrounded) return false;
		
		if (!InputPress.Abc || !CheckCeilingDistance()) return false;
		
		if (!SharedData.FixJumpSize)
		{
			// Why they even do that???
			Radius = RadiusNormal;
		}
	
		if (!IsSpinning)
		{
			Position += new Vector2(0f, Radius.Y - RadiusSpin.Y);
			Radius = RadiusSpin;
		}
		else if (!SharedData.NoRollLock && SharedData.PlayerPhysics != PhysicsTypes.CD)
		{
			IsAirLock = true;
		}

		float radians = Mathf.DegToRad(Angle);
		Speed += PhysicParams.JumpVelocity * new Vector2(MathF.Sin(radians), MathF.Cos(radians));
		
		IsSpinning = true;	
		IsJumping = true;
		PushingObject = null;
		IsGrounded = false;
		OnObject = null;
		StickToConvex = false;
		GroundMode = 0;
	
		Sprite.AnimationType = Animations.Spin;
		
		//TODO: audio
		//audio_play_sfx(sfx_jump);
	
		// return player control routine
		return true;
	}

	private bool CheckCeilingDistance()
	{
		if (GroundMode == Constants.GroundMode.Ceiling) return true;
		
		return CalculateCellDistance() >= 6; // Target ceiling distance
	}

	private int CalculateCellDistance()
	{
		TileCollider.SetData((Vector2I)Position, TileLayer, TileMap, GroundMode);
		
		return GroundMode switch
		{
			Constants.GroundMode.Floor => TileCollider.FindClosestDistance(
				Radius.Shuffle(-1, -1),
				Radius.Shuffle( 1, -1),
				true, Constants.Direction.Negative),
			
			Constants.GroundMode.RightWall => TileCollider.FindClosestDistance(
				Radius.Shuffle(-1, -1, true),
				Radius.Shuffle(1, -1, true),
				false, Constants.Direction.Negative),
			
			Constants.GroundMode.LeftWall => TileCollider.FindClosestDistance(
				Radius.Shuffle(-1, 1, true),
				Radius.Shuffle(1, 1, true),
				false, Constants.Direction.Positive),
			
			Constants.GroundMode.Ceiling => throw new ArgumentOutOfRangeException(),
			
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	private void ChargeDropDash()
	{
		if (!SharedData.DropDash || Action != Actions.DropDash || IsGrounded) return;
		
		if (InputDown.Abc)
		{
			IsAirLock = false;		
			if (ActionValue < MaxDropDashCharge)
			{
				ActionValue++;
			}
			else 
			{
				if (Sprite.AnimationType != Animations.DropDash)
				{
					Sprite.AnimationType = Animations.DropDash;
					//TODO: audio
					//audio_play_sfx(sfx_charge);
				}
			}
			
			return;
		}

		if (ActionValue <= 0f) return;
		
		if (Mathf.IsEqualApprox(ActionValue, MaxDropDashCharge))
		{		
			Sprite.AnimationType = Animations.Spin;
			Action = Actions.DropDashCancel;
		}
			
		ActionValue = 0f;
	}

	private void UpdateDropDashGroundSpeed(float limitSpeed, float force)
	{
		var sign = (float)Facing;
		limitSpeed *= sign;
		force *= sign;
		
		if (Speed.X * sign >= 0f)
		{
			GroundSpeed = Mathf.Floor(GroundSpeed / 4f) + force;
			if (GroundSpeed * sign <= limitSpeed) return;
			GroundSpeed = limitSpeed;
			return;
		}

		GroundSpeed = force;
		if (Mathf.IsEqualApprox(Angle, 360f)) return;
		
		GroundSpeed += Mathf.Floor(GroundSpeed / 2f);
	}
	
	private void ProcessFlight()
	{
		if (Action != Actions.Flight) return;
	
		if (ActionValue > 0f)
		{
			if (--ActionValue == 0f)
			{
				if (!IsUnderwater)
				{
					Sprite.AnimationType = Animations.FlyTired;
					//TODO: audio
					//audio_play_sfx(sfx_flight2, true);
				}
				else
				{
					Sprite.AnimationType = Animations.SwimTired;
				}
			
				Gravity = GravityType.TailsDown;
				//TODO: audio
				//audio_stop_sfx(sfx_flight);
			}
			else
			{	
				Sprite.AnimationType = IsUnderwater ? Animations.Swim : Animations.Fly;
			
				if (!IsUnderwater || CarryTarget == null)
				{
					if (InputPress.Abc)
					{
						Gravity = GravityType.TailsUp;
					}
					else if (Speed.Y < -1f)
					{
						Gravity = GravityType.TailsDown;
					}
				
					Speed.Y = Math.Max(Speed.Y, -4f);
				}
				else
				{
					Gravity = GravityType.TailsDown;
				}
			}
		}

		if (!SharedData.FlightCancel || !InputDown.Down || !InputPress.Abc) return;
		
		Camera.Main.BufferPosition.Y += Radius.Y - RadiusSpin.Y;
		Radius = RadiusSpin;
		Sprite.AnimationType = Animations.Spin;
		IsSpinning	= true;
		Action = Actions.None;
		
		//audio_stop_sfx(sfx_flight);
		//audio_stop_sfx(sfx_flight2);
		ResetGravity();
	}
	
	private void ProcessClimb()
	{
		if (Action != Actions.Climb) return;
		
		switch ((ClimbStates)ActionState)
		{
			case ClimbStates.Normal:
				ClimbNormal();
				break;
			
			case ClimbStates.Ledge:
				switch (ActionValue++)
				{
					case 0f: // Frame 0
						Sprite.AnimationType = Animations.ClimbLedge;
						Position += new Vector2(3f * (float)Facing, -3f);
						break;
					
					case 6f: // Frame 1
						Position += new Vector2(8f * (float)Facing, -10f);
						break;
					
					case 12f: // Frame 2
						Position -= new Vector2(8f * (float)Facing, 12f);
						break;
					
					case 18f: // End
						Land();
						Sprite.AnimationType = Animations.Idle;
						Position += new Vector2(8f * (float)Facing, 4f);
						break;
				}
				break;
		}
	}

	private void ClimbNormal()
	{
		if (!Mathf.IsEqualApprox(Position.X, PreviousPosition.X))
		{
			ReleaseClimb();
			return;
		}
		
		const int stepsPerFrame = 4;
		UpdateSpeedYOnClimb(Sprite.SpriteFrames.GetFrameCount(Sprite.Animation) * stepsPerFrame);
		
		int radiusX = Radius.X;
		if (Facing == Constants.Direction.Negative)
		{
			radiusX++;
		}
		
		TileCollider.SetData((Vector2I)Position, TileLayer, TileMap);

		if (Speed.Y < 0 ? ClimbUpOntoWall(radiusX) : ReleaseClimbing(radiusX)) return;
		
		if (!InputPress.Abc)
		{
			if (Speed.Y != 0)
			{
				Sprite.Frame = Mathf.FloorToInt(ActionValue / stepsPerFrame);
			}
			return;
		}
		
		Sprite.AnimationType = Animations.Spin;
		IsSpinning = true;
		IsJumping = true;
		Action = Actions.None;
		Facing = (Constants.Direction)(-(int)Facing);
		Speed = new Vector2(3.5f * (float)Facing, PhysicParams.MinimalJumpVelocity);
			
		//TODO: audio
		//audio_play_sfx(sfx_jump);
		ResetGravity();
	}

	private bool ClimbUpOntoWall(int radiusX)
	{
		// If the wall is far away from Knuckles then he must have reached a ledge, make him climb up onto it
		var offset = new Vector2I(radiusX * (int)Facing, -Radius.Y - 1);
		int wallDistance = TileCollider.FindDistance(offset, false, Facing);
			
		if (wallDistance >= 4)
		{
			ActionState = (int)ClimbStates.Ledge;
			ActionValue = 0;
			Speed.Y = 0;
			return true;
		}

		// If Knuckles has encountered a small dip in the wall, cancel climb movement
		if (wallDistance != 0)
		{
			Speed.Y = 0;
		}

		// If Knuckles has bumped into the ceiling, cancel climb movement and push him out
		offset = new Vector2I(radiusX * (int)Facing, 1 - RadiusNormal.Y);
		int ceilDistance = TileCollider.FindDistance(offset, true, Constants.Direction.Negative);

		if (ceilDistance >= 0) return false;
		Position -= new Vector2(0f, ceilDistance);
		Speed.Y = 0;
		return false;
	}

	private bool ReleaseClimbing(int radiusX)
	{
		// If Knuckles is no longer against the wall, make him let go
		var offset = new Vector2I(radiusX * (int)Facing, Radius.Y + 1);
		int wallDistance = TileCollider.FindDistance(offset, false, Facing);
			
		if (wallDistance != 0)
		{
			ReleaseClimb();
			return true;
		}
			
		// If Knuckles has reached the floor, make him land
		offset = new Vector2I(radiusX * (int)Facing, RadiusNormal.Y);
		(int distance, float angle) = TileCollider.FindTile(offset, true, Constants.Direction.Positive);

		if (distance >= 0) return false;
		Position += new Vector2(0f, distance + RadiusNormal.Y - Radius.Y);
		Angle = angle;
				
		Land();

		Sprite.AnimationType = Animations.Idle;
		Speed.Y = 0;
				
		return true;
	}

	private void UpdateSpeedYOnClimb(int maxValue)
	{
		if (InputDown.Up)
		{
			if (++ActionValue > maxValue)
			{
				ActionValue = 0f;
			}
					
			Speed.Y = -PhysicParams.AccelerationClimb;
			return;
		}
		if (InputDown.Down)
		{
			if (--ActionValue < 0f)
			{
				ActionValue = maxValue;
			}
					
			Speed.Y = PhysicParams.AccelerationClimb;
			return;
		}

		Speed.Y = 0;
	}

	private void ReleaseClimb()
	{
		Sprite.AnimationType = Animations.GlideFall;
		Action = Actions.Glide;
		ActionState = (int)GlideStates.Fall;
		ActionValue = 1;
		Radius = RadiusNormal;
		
		ResetGravity();
	}
	
	private void ProcessGlide()
	{
		if (Action != Actions.Glide || (GlideStates)ActionState == GlideStates.Fall) return;
		
		switch ((GlideStates)ActionState)
		{
			case GlideStates.Air: GlideAir(); break;
			case GlideStates.Ground: GlideGround(); break;
			default: throw new ArgumentOutOfRangeException();
		}
	}

	private void GlideAir()
	{
		UpdateGlideSpeed();
		GlideAirTurnAround();
		UpdateGlideGravityAndHorizontalSpeed();
		UpdateGlideAirAnimationFrame();

		if (InputDown.Abc) return;
		
		Sprite.AnimationType = Animations.GlideFall;
		ActionState = (int)GlideStates.Fall;
		ActionValue = 0f;
		Radius = RadiusNormal;
		Speed.X *= 0.25f;

		ResetGravity();
	}

	private void UpdateGlideSpeed()
	{
		if (GroundSpeed < 4f)
		{
			GroundSpeed += 0.03125f;
			return;
		}

		if (ActionValue % 180f != 0) return;
		GroundSpeed = Math.Min(GroundSpeed + PhysicParams.AccelerationGlide, 24f);
	}

	private void UpdateGlideGravityAndHorizontalSpeed()
	{
		const float glideGravity = 0.125f;
		
		Speed.X = GroundSpeed * -Mathf.Cos(Mathf.DegToRad(ActionValue));
		Gravity = Speed.Y < 0.5f ? glideGravity : -glideGravity;
	}

	private void UpdateGlideAirAnimationFrame()
	{
		float angle = Math.Abs(ActionValue) % 180f;
		switch (angle)
		{
			case < 30f or > 150f:
				Sprite.Frame = 0;
				break;
			case < 60f or > 120f:
				Sprite.Frame = 1;
				break;
			default:
				Facing = angle < 90 ? Constants.Direction.Negative : Constants.Direction.Positive;
				Sprite.Frame = 2;
				break;
		}
	}

	private void GlideGround()
	{
		GlideGroundUpdateSpeedX();
				
		if (Speed.X == 0f)
		{
			Land();
			Sprite.Frame = 1;

			Sprite.AnimationType = Animations.GlideGround;
			GroundLockTimer = 16;
			GroundSpeed = 0;

			return;
		}

		if (ActionValue % 4 == 0)
		{
			//TODO: obj_dust_skid
			//instance_create(x, y + Radius.Y, obj_dust_skid);
		}
				
		if (ActionValue > 0 && ActionValue % 8 == 0)
		{
			//TODO: audio
			//audio_play_sfx(sfx_slide);
		}
					
		ActionValue++;
	}

	private void GlideGroundUpdateSpeedX()
	{
		const float slideFriction = 0.09375f;
		
		if (!InputDown.Abc)
		{
			Speed.X = 0f;
			return;
		}

		Speed.X = Speed.X switch
		{
			> 0f => Math.Max(0f, Speed.X - slideFriction),
			< 0f => Math.Min(0f, Speed.X + slideFriction),
			_ => Speed.X
		};
	}

	private void GlideAirTurnAround()
	{
		const float angleIncrement = 2.8125f;
		
		if (InputDown.Left && !Mathf.IsZeroApprox(ActionValue))
		{
			ActionValue = (ActionValue > 0f ? -ActionValue : ActionValue) + angleIncrement;
			return;
		}
		
		if (InputDown.Right && !Mathf.IsEqualApprox(ActionValue, 180f))
		{
			ActionValue = (ActionValue < 0f ? -ActionValue : ActionValue) + angleIncrement;
			return;
		}
		
		if (Mathf.IsZeroApprox(ActionValue % 180f)) return;
		ActionValue += angleIncrement;
	}
	
	private void ChargeHammerSpin()
	{
		if (Action != Actions.HammerSpin || IsGrounded) return;
		
		if (InputDown.Abc)
		{
			if (Mathf.IsEqualApprox(++ActionValue, MaxDropDashCharge))
			{
				//TODO: audio
				//audio_play_sfx(sfx_charge);
			}

			IsAirLock = false;
			return;
		}

		switch (ActionValue)
		{
			case <= 0f: return;
			
			case >= MaxDropDashCharge:
				Sprite.AnimationType = Animations.Spin;
				Action = Actions.HammerSpinCancel;
				break;
		}

		ActionValue = 0f;
	}

	private void ReleaseHammerSpin()
	{
		if (Action != Actions.HammerSpin) return;
		
		if (ActionValue < MaxDropDashCharge) return;

		Sprite.AnimationType = Animations.HammerDash;
		Action = Actions.HammerDash;
		ActionValue = 0f;
		
		//TODO: audio
		//audio_stop_sfx(sfx_charge);
		//audio_play_sfx(sfx_release);
	}
	
	private void ProcessHammerDash()
	{
		if (Action != Actions.HammerDash) return;

		if (!InputDown.Abc)
		{
			// Note that animation isn't cleared. All checks for Hammer Dash should refer to its animation
			Action = Actions.None;
			return;
		}
		
		// Air movement isn't overwritten completely, refer to ProcessMovementAir()
		if (!IsGrounded) return;
		
		float radians = Mathf.DegToRad(Angle);
		float cosine = MathF.Cos(radians);
		if (Mathf.IsEqualApprox(++ActionValue, 60f) || GroundSpeed == 0f || cosine <= 0f)
		{
			Action = Actions.None;
		}

		if (InputDown.Left && GroundSpeed > 0f || InputDown.Right && GroundSpeed < 0f)
		{
			Facing = (Constants.Direction)(-(int)Facing);
			GroundSpeed *= -1;
		}
		
		Speed = GroundSpeed * new Vector2(cosine, -MathF.Sin(radians));
	}

	private void CancelHammerDash()
	{
		Sprite.AnimationType = Animations.Move;
		Action = Actions.None;
	}

	private void ProcessSlopeResist()
	{
		if (!IsGrounded || IsSpinning || Angle is > 135f and <= 225f) return;
	
		if (Action is Actions.HammerDash or Actions.PeelOut) return;
		
		float slopeGrv = 0.125f * MathF.Sin(Mathf.DegToRad(Angle));
		if (GroundSpeed != 0f || SharedData.PlayerPhysics >= PhysicsTypes.S3 && Math.Abs(slopeGrv) > 0.05078125f)
		{
			GroundSpeed -= slopeGrv;
		}
	}

	private void ProcessSlopeResistRoll()
	{
		if (!IsGrounded || !IsSpinning || Angle is > 135f and <= 225f) return;
	
		float angleSine = MathF.Sin(Mathf.DegToRad(Angle));
		float slopeGrv = Math.Sign(GroundSpeed) != Math.Sign(angleSine) ? 0.3125f : 0.078125f;
		GroundSpeed -= slopeGrv * angleSine;
	}

	private void ProcessMovementGround()
	{
		// Control routine checks
		if (!IsGrounded || IsSpinning) return;
		
		// Action checks
		if (Action is Actions.SpinDash or Actions.PeelOut or Actions.HammerDash) return;
		
		// If Knuckles is standing up from a slide and DOWN button is pressed, cancel
		// control lock. This allows him to Spin Dash
		if (Sprite.AnimationType == Animations.GlideGround && InputDown.Down)
		{
			GroundLockTimer = 0f;
		}
		
		if (Mathf.IsZeroApprox(GroundLockTimer))
		{
			var doSkid = false;
			
			// Move left
			if (InputDown.Left)
			{	
				doSkid = MoveOnGround(Constants.Direction.Negative);
			}
			
			// Move right
			if (InputDown.Right)
			{
				doSkid = MoveOnGround(Constants.Direction.Positive);
			}
			
			UpdateMovementGroundAnimation(doSkid);
			SetPushAnimation();
		}
		
		// Apply friction
		if (InputDown is { Left: false, Right: false })
		{
			GroundSpeed = GroundSpeed switch
			{
				> 0f => Math.Max(GroundSpeed - PhysicParams.Friction, 0f),
				< 0f => Math.Min(GroundSpeed + PhysicParams.Friction, 0f),
				_ => GroundSpeed
			};
		}
		
		// Convert ground velocity into directional velocity
		float radians = Mathf.DegToRad(Angle);
		Speed = GroundSpeed * new Vector2(Mathf.Cos(radians), -Mathf.Sin(radians));
	}

	private bool MoveOnGround(Constants.Direction direction)
	{
		var sign = (float)direction;
		
		if (GroundSpeed * sign < 0f)
		{
			GroundSpeed += PhysicParams.Deceleration * sign;
			if (GroundSpeed * sign >= 0f)
			{
				GroundSpeed = 0.5f * sign;
			}
					
			return true;
		}

		if (!SharedData.NoSpeedCap || GroundSpeed * sign < PhysicParams.AccelerationTop)
		{
			GroundSpeed = direction == Constants.Direction.Positive
				? Math.Min(GroundSpeed + PhysicParams.Acceleration,  PhysicParams.AccelerationTop)
				: Math.Max(GroundSpeed - PhysicParams.Acceleration, -PhysicParams.AccelerationTop);
		}

		if (Facing == direction) return false;
		
		Sprite.AnimationType = Animations.Move;
		Facing = direction;
		PushingObject = null;
					
		Sprite.Frame = 0;

		return false;
	}

	private void UpdateMovementGroundAnimation(bool doSkid)
	{
		if (Angles.GetQuadrant(Angle) != 0f)
		{
			if (Sprite.AnimationType is Animations.Skid or Animations.Push) return;
			Sprite.AnimationType = Animations.Move;
			return;
		}
		
		if (doSkid && Math.Abs(GroundSpeed) >= 4f && Sprite.AnimationType != Animations.Skid)
		{
			Sprite.AnimationTimer = Type == Types.Sonic ? 24f : 16f;
			Sprite.AnimationType = Animations.Skid;
					
			//TODO: audio
			//audio_play_sfx(sfx_skid);
		}
				
		if (GroundSpeed != 0f)
		{
			// TODO: This
			if (Sprite.AnimationType is Animations.Skid or Animations.Push) return;
			Sprite.AnimationType = Animations.Move;
			return;
		}
		
		PushingObject = null;
		Sprite.AnimationType = InputDown.Up ? Animations.LookUp : InputDown.Down ? Animations.Duck : Animations.Idle;
	}

	private void SetPushAnimation()
	{
		if (PushingObject == null)
		{
			if (Sprite.AnimationType != Animations.Push) return;
			Sprite.AnimationType = Animations.Move;
			return;
		}
		
		if (Sprite.AnimationType != Animations.Move || !Sprite.IsFrameChanged) return;
		Sprite.AnimationType = Animations.Push;
	}

	private void ProcessMovementGroundRoll()
	{
		// Control routine checks
		if (!IsGrounded || !IsSpinning) return;

		if (GroundLockTimer == 0f)
		{
			if (InputDown.Left)
			{
				RollOnGround(Constants.Direction.Negative); // Move left
			}
			
			if (InputDown.Right)
			{
				RollOnGround(Constants.Direction.Positive); // Move right
			}
		}
	
		// Apply friction
		GroundSpeed = GroundSpeed switch
		{
			> 0 => Math.Max(GroundSpeed - PhysicParams.FrictionRoll, 0),
			< 0 => Math.Min(GroundSpeed + PhysicParams.FrictionRoll, 0),
			_ => GroundSpeed
		};

		UpdateSpinningOnGround();
	
		float radians = Mathf.DegToRad(Angle);
		Speed = GroundSpeed * new Vector2(Mathf.Cos(radians), -Mathf.Sin(radians));
		Speed.X = Math.Clamp(Speed.X, -16f, 16f);
	}

	private void RollOnGround(Constants.Direction direction)
	{
		var sign = (float)direction;
		float unsignedSpeed = sign * GroundSpeed;
		if (unsignedSpeed >= 0f || Mathf.IsZeroApprox(unsignedSpeed))
		{
			Facing = direction;
			PushingObject = null;
			return;
		}

		GroundSpeed += sign * PhysicParams.DecelerationRoll;
		if (sign * GroundSpeed < 0f) return;
		GroundSpeed = sign * 0.5f;
	}

	private void UpdateSpinningOnGround()
	{
		// Stop spinning
		if (!IsForcedRoll)
		{
			if (GroundSpeed != 0f)
			{
				if (SharedData.PlayerPhysics != PhysicsTypes.SK || Math.Abs(GroundSpeed) >= 0.5f) return;
			}
			
			Position += new Vector2(0f, Radius.Y - RadiusNormal.Y);

			Radius = RadiusNormal;
			
			IsSpinning = false;
			Sprite.AnimationType = Animations.Idle;
			return;
		}
	
		// If forced to spin, keep moving player
		if (SharedData.PlayerPhysics == PhysicsTypes.CD)
		{
			if (GroundSpeed is >= 0f and < 2f)
			{
				GroundSpeed = 2f;
			}
			return;
		}
		
		if (GroundSpeed != 0f) return;
		GroundSpeed = SharedData.PlayerPhysics == PhysicsTypes.S1 ? 2f : 4f * (float)Facing;
	}

	private void ProcessMovementAir()
	{
		// Control routine checks
		if (IsGrounded || IsDead) return;
	
		// Action checks
		if (Action is Actions.Carried or Actions.Climb or Actions.Glide
		    && (GlideStates)ActionState != GlideStates.Fall) return;
	
		// Update Angle (rotate player)
		if (!Mathf.IsEqualApprox(Angle, 360f))
		{
			if (Angle >= 180f)
			{
				Angle += 2.8125f;
			}
			else
			{
				Angle -= 2.8125f;
			}
		
			if (Angle is <= 0f or > 360f)
			{
				Angle = 360f;
			}
		}
	
		// Limit upward speed
		if (!IsJumping && Action != Actions.SpinDash && !IsForcedRoll)
		{
			if (Speed.Y < -15.75f)
			{
				Speed.Y = -15.75f;
			}
		}
	
		// Limit downward speed
		if (SharedData.PlayerPhysics == PhysicsTypes.CD && Speed.Y > 16)
		{
			Speed.Y = 16;
		}

		if (Action == Actions.HammerDash)
		{
			if (InputDown.Left)
			{
				Facing = Constants.Direction.Negative;
			}
			else if (InputDown.Right)
			{
				Facing = Constants.Direction.Positive;
			}
			
			return;
		}
	
		if (!IsAirLock)
		{
			// Move left
			if (InputDown.Left)
			{
				if (Speed.X > 0) 
				{
					Speed.X -= PhysicParams.AccelerationAir;
				}
				else if (!SharedData.NoSpeedCap || Speed.X > -PhysicParams.AccelerationTop)
				{
					Speed.X = Math.Max(Speed.X - PhysicParams.AccelerationAir, -PhysicParams.AccelerationTop);
				}
			
				Facing = Constants.Direction.Negative;
			}
		
			// Move right
			if (InputDown.Right)
			{
				if (Speed.X < 0)
				{
					Speed.X += PhysicParams.AccelerationAir;
				} 
				else if (!SharedData.NoSpeedCap || Speed.X < PhysicParams.AccelerationTop)
				{
					Speed.X = Math.Min(Speed.X + PhysicParams.AccelerationAir, PhysicParams.AccelerationTop);
				}
			
				Facing = Constants.Direction.Positive;
			}	
		}
	
		// Apply air drag
		if (!IsHurt && Speed.Y is < 0f and > -4f)
		{
			Speed.X -= Mathf.Floor(Speed.X * 8f) / 256f;
		}
	}

	private void ProcessBalance()
	{
		if (!IsGrounded || IsSpinning) return;

		if (GroundSpeed != 0 || Action is Actions.SpinDash or Actions.PeelOut) return;
		
		if (SharedData.PlayerPhysics == PhysicsTypes.SK && InputDown.Down || InputDown.Up && SharedData.PeelOut) return;
	
		if (OnObject != null)
		{
			// TODO: check IsInstanceValid == instance_exist
			if (!IsInstanceValid(OnObject) || OnObject.SolidData.NoBalance) return;
	
			const int leftEdge = 2;
			int rightEdge = OnObject.SolidData.Radius.X * 2 - leftEdge;
			int playerX = Mathf.FloorToInt(OnObject.SolidData.Radius.X - OnObject.Position.X + Position.X);
		
			if (playerX < leftEdge)
			{
				BalanceToDirection(Constants.Direction.Negative, playerX < leftEdge - 4);
			}
			else if (playerX > rightEdge)
			{
				BalanceToDirection(Constants.Direction.Positive, playerX > rightEdge + 4);
			}
		}
		
		if (Angles.GetQuadrant(Angle) > 0) return;
			
		TileCollider.SetData((Vector2I)Position + new Vector2I(0, Radius.Y), TileLayer, TileMap);
		int floorDist = TileCollider.FindDistance(new Vector2I(), true, Constants.Direction.Positive);	
		if (floorDist < 12) return;

		const Constants.Direction direction = Constants.Direction.Positive;
			
		int distanceLeft = TileCollider.FindDistance(new Vector2I(-Radius.X, 0), true, direction);
		int distanceRight = TileCollider.FindDistance(new Vector2I(Radius.X, 0), true, direction);
		
		if (distanceLeft != Constants.TileSize * 2 && distanceRight != Constants.TileSize * 2) return;

		int offsetX = distanceLeft == Constants.TileSize * 2 ? 6 : -6;
		bool isPanic = TileCollider.FindDistance(new Vector2I(offsetX, 0), true, direction) >= 12;
		BalanceToDirection(Constants.Direction.Negative, isPanic);
	}

	private void BalanceToDirection(Constants.Direction direction, bool isPanic)
	{
		switch (Type)
		{
			case Types.Amy:
			case Types.Tails:
			case Types.Sonic when IsSuper:
				Sprite.AnimationType = Animations.Balance;
				Facing = direction;
				break;
			
			case Types.Knuckles:
				Sprite.AnimationType = Facing == direction ? Animations.Balance : Animations.BalanceFlip;
				Facing = direction;
				break;
			
			case Types.Sonic:
				if (!isPanic)
				{
					Sprite.AnimationType = Facing == direction ? Animations.Balance : Animations.BalanceFlip;
				}
				else if (Facing != direction)
				{
					Sprite.AnimationType = Animations.BalanceTurn;
					Facing = direction;
				}
				else if (Sprite.AnimationType != Animations.BalanceTurn)
				{
					Sprite.AnimationType = Animations.BalancePanic;
				}
				break;
		}
	}

	private void ProcessCollisionGroundWalls()
	{
		// Control routine checks
		if (!IsGrounded) return;
		
		if (SharedData.PlayerPhysics < PhysicsTypes.SK)
		{
			if (Angle is > 90f and <= 270f) return;
		}
		else if (Angle is >= 90f and <= 270f && Angle % 90f != 0f)
		{
			return;
		}

		int castDirection = Angle switch
		{
			>= 45f and <= 128f => 1,
			> 128f and < 225f => 2,
			>= 225f and < 315f => 3,
			_ => 0
		};

		int wallRadius = RadiusNormal.X + 1;
		int offsetY = 8 * (Mathf.IsEqualApprox(Angle, 360f) ? 1 : 0);

		int sign;
		Constants.Direction firstDirection, secondDirection;
		switch (GroundSpeed)
		{
			case < 0f:
				sign = (int)Constants.Direction.Positive;
				firstDirection = Constants.Direction.Negative;
				secondDirection = Constants.Direction.Positive;
				break;
			
			case > 0f:
				sign = (int)Constants.Direction.Negative;
				firstDirection = Constants.Direction.Positive;
				secondDirection = Constants.Direction.Negative;
				wallRadius *= sign;
				break;
			
			default:
				return;
		}
		
		TileCollider.SetData((Vector2I)(Position + Speed), TileLayer, TileMap, GroundMode);
		
		int wallDist = castDirection switch
		{
			0 => TileCollider.FindDistance(new Vector2I(-wallRadius, offsetY), false, firstDirection),
			1 => TileCollider.FindDistance(new Vector2I(0, wallRadius), true, secondDirection),
			2 => TileCollider.FindDistance(new Vector2I(wallRadius, 0), false, secondDirection),
			3 => TileCollider.FindDistance(new Vector2I(0, -wallRadius), true, firstDirection),
			_ => throw new ArgumentOutOfRangeException()
		};
		
		if (wallDist >= 0) return;
		
		wallDist *= sign;
		
		switch (Angles.GetQuadrant(Angle))
		{
			case 0:
				Speed.X -= wallDist;
				GroundSpeed = 0f;
					
				if (Facing == firstDirection && !IsSpinning)
				{
					PushingObject = this;
				}
				break;
				
			case 1:
				Speed.Y += wallDist;
				break;
				
			case 2:
				Speed.X += wallDist;
				GroundSpeed = 0;
					
				if (Facing == firstDirection && !IsSpinning)
				{
					PushingObject = this;
				}
				break;
				
			case 3:
				Speed.Y -= wallDist;
				break;
		}
	}

	private void ProcessRollStart()
	{
		if (!IsGrounded || IsSpinning || Action is Actions.SpinDash or Actions.HammerDash) return;
		
		if (!IsForcedRoll && (InputDown.Left || InputDown.Right)) return;
	
		var allowSpin = false;
		if (InputDown.Down)
		{
			if (SharedData.PlayerPhysics == PhysicsTypes.SK)
			{
				if (Math.Abs(GroundSpeed) >= 1f)
				{
					allowSpin = true;
				}
				else
				{
					Sprite.AnimationType = Animations.Duck;
				}
			}
			else if (Math.Abs(GroundSpeed) >= 0.5f)
			{
				allowSpin = true;
			}
		}

		if (!allowSpin && !IsForcedRoll) return;
		Position += new Vector2(0f,  Radius.Y - RadiusSpin.Y);
		Radius.Y = RadiusSpin.Y;
		Radius.X = RadiusSpin.X;
		IsSpinning = true;
		Sprite.AnimationType = Animations.Spin;
			
		//TODO: audio
		//audio_play_sfx(sfx_roll);
	}

	private void ProcessLevelBound()
	{
		if (IsDead) return;
	
		Camera camera = Camera.Main;
		
		if (camera == null) return;
	
		// Note that position here is checked including subpixel
		if (Position.X + Speed.X < camera.Limit.X + 16f)
		{
			GroundSpeed = 0;
			Speed.X = 0;
			Position = new Vector2(camera.Limit.X + 16f, Position.Y);
		}
	
		float rightBound = camera.Limit.Z - 24f;
		//TODO: replace instance_exists
		/*if (instance_exists(obj_signpost))
		{
			// TODO: There should be a better way?
			rightBound += 64;
		}*/
	
		if (Position.X + Speed.X > rightBound)
		{
			GroundSpeed = 0;
			Speed.X = 0;
			Position = new Vector2(rightBound, Position.Y);
		}
	
		switch (Action)
		{
			case Actions.Flight or Actions.Climb:
			{
				if (Position.Y + Speed.Y < camera.Limit.Y + 16f)
				{ 	
					if (Action == Actions.Flight)
					{
						Gravity	= GravityType.TailsDown;
					}
			
					Speed.Y = 0f;
					Position = new Vector2(Position.X, camera.Limit.Y + 16f);
				}

				break;
			}
			case Actions.Glide when Position.Y < camera.Limit.Y + 10f:
				Speed.X = 0f;
				break;
		}
	
		if (AirTimer > 0f && Position.Y > Math.Max(camera.Limit.W, camera.Bound.Z))
		{
			Kill();
		}
	}

	private void ProcessPosition()
	{
		if (Action == Actions.Carried) return;
	
		if (StickToConvex)
		{
			Speed = Speed.Clamp(new Vector2(-16f, -16f), new Vector2(16, 16));
		}

		Position += Speed;
	
		if (!IsGrounded && Action != Actions.Carried)
		{
			Speed.Y += Gravity;
		}
	}

	private void ProcessCollisionGroundFloor()
	{
		// Control routine checks
		if (!IsGrounded || OnObject != null) return;

		GroundMode = Angle switch
		{
			<= 45 or >= 315 => Constants.GroundMode.Floor,
			> 45 and < 135 => Constants.GroundMode.RightWall,
			>= 135 and <= 225 => Constants.GroundMode.Ceiling,
			_ => Constants.GroundMode.LeftWall
		};

		const int minTolerance = 4;
		const int maxTolerance = 14;
		
		TileCollider.SetData((Vector2I)Position, TileLayer, TileMap, GroundMode);

		(int distance, float angle) = GroundMode switch
		{
			Constants.GroundMode.Floor => TileCollider.FindClosestTile(
				new Vector2I(-Radius.X, Radius.Y),
				new Vector2I(Radius.X, Radius.Y), 
				true, Constants.Direction.Positive),
			
			Constants.GroundMode.RightWall => TileCollider.FindClosestTile(
				new Vector2I(Radius.Y, Radius.X),
				new Vector2I(Radius.Y, -Radius.X), 
				false, Constants.Direction.Positive),
			
			Constants.GroundMode.Ceiling => TileCollider.FindClosestTile(
				new Vector2I(Radius.X, -Radius.Y),
				new Vector2I(-Radius.X, -Radius.Y), 
				true, Constants.Direction.Negative),
			
			Constants.GroundMode.LeftWall => TileCollider.FindClosestTile(
				new Vector2I(-Radius.Y, -Radius.X), 
				new Vector2I(-Radius.Y, Radius.X), 
				false, Constants.Direction.Negative),
			
			_ => throw new ArgumentOutOfRangeException()
		};
		
		if (!StickToConvex)
		{
			float toleranceCheckSpeed = GroundMode switch
			{
				Constants.GroundMode.Floor => Speed.X,
				Constants.GroundMode.RightWall => Speed.Y,
				Constants.GroundMode.Ceiling => Speed.X,
				Constants.GroundMode.LeftWall => Speed.Y,
				_ => throw new ArgumentOutOfRangeException()
			};
			
			float tolerance = SharedData.PlayerPhysics < PhysicsTypes.S2 ? 
				maxTolerance : Math.Min(minTolerance + Math.Abs(MathF.Floor(toleranceCheckSpeed)), maxTolerance);
			
			if (distance > tolerance)
			{
				PushingObject = null;
				IsGrounded = false;
						
				Sprite.Frame = 0;
				return;
			}
		}

		if (distance < -maxTolerance) return;
		
		Position += GroundMode switch
		{
			Constants.GroundMode.Floor => new Vector2(0f, distance),
			Constants.GroundMode.RightWall => new Vector2(distance, 0f),
			Constants.GroundMode.Ceiling => new Vector2(0f, -distance),
			Constants.GroundMode.LeftWall => new Vector2(-distance, 0f),
			_ => throw new ArgumentOutOfRangeException()
		};

		Angle = SharedData.PlayerPhysics >= PhysicsTypes.S2 ? SnapFloorAngle(angle) : angle;
	}

	private float SnapFloorAngle(float floorAngle)
	{
		float difference = Math.Abs(Angle % 180f - floorAngle % 180f);
		
		if (difference is <= 45f or >= 135f) return floorAngle;
		
		floorAngle = MathF.Round(Angle / 90f) % 4f * 90f;
		if (floorAngle == 0f)
		{
			floorAngle = 360f;
		}

		return floorAngle;
	}

	private void ProcessSlopeRepel()
	{
		if (!IsGrounded || StickToConvex || Action == Actions.HammerDash) return;
	
		if (GroundLockTimer > 0f)
		{
			GroundLockTimer--;
		}
		else if (Math.Abs(GroundSpeed) < 2.5f)
		{
			if (SharedData.PlayerPhysics < PhysicsTypes.S3)
			{
				if (Angles.GetQuadrant(Angle) == 0) return;
				
				GroundSpeed = 0f;	
				GroundLockTimer = 30f;
				IsGrounded = false;
			}
			else if (Angle is > 33.75f and <= 326.25f)
			{
				if (Angle is > 67.5f and <= 292.5f)
				{
					IsGrounded = false;
				}
				else
				{
					GroundSpeed += Angle < 180f ? -0.5f : 0.5f;
				}
		
				GroundLockTimer = 30;
			}
		}
	}

	private void ProcessCollisionAir()
	{
		// Control routine checks
		if (IsGrounded || IsDead) return;
		
		// Action checks
		if (Action is Actions.Glide or Actions.Climb) return;
		
		int wallRadius = RadiusNormal.X + 1;
		byte moveQuadrant = Angles.GetQuadrant(Angles.GetVector256(Speed));
		
		TileCollider.SetData((Vector2I)Position, TileLayer, TileMap);
		
		// Perform left wall collision if not moving mostly right
		if (moveQuadrant != 1)
		{
			int wallDistance = TileCollider.FindDistance(
				new Vector2I(-wallRadius, 0), false, Constants.Direction.Negative);
			
			if (wallDistance < 0f)
			{
				Position -= new Vector2(wallDistance, 0f);
				TileCollider.Position = (Vector2I)Position;
				Speed.X = 0;
				
				if (moveQuadrant == 3)
				{
					GroundSpeed = Speed.Y;
					return;
				}
			}
		}
		
		// Perform right wall collision if not moving mostly left
		if (moveQuadrant != 3)
		{
			int wallDistance = TileCollider.FindDistance(
				new Vector2I(wallRadius, 0), false, Constants.Direction.Positive);
			
			if (wallDistance < 0f)
			{
				Position += new Vector2(wallDistance, 0f);
				TileCollider.Position = (Vector2I)Position;
				Speed.X = 0f;
				
				if (moveQuadrant == 1)
				{
					GroundSpeed = Speed.Y;
					return;
				}
			}
		}
		
		// Perform ceiling collision if not moving mostly down
		if (moveQuadrant != 0)
		{
			(int roofDistance, float roofAngle) = TileCollider.FindClosestTile(
				Radius.Shuffle(-1, -1), 
				Radius.Shuffle(1, -1),
				true, Constants.Direction.Negative);
			
			if (moveQuadrant == 3 && SharedData.PlayerPhysics >= PhysicsTypes.S3 && roofDistance <= -14f)
			{
				// Perform right wall collision if moving mostly left and too far into the ceiling
				int wallDist = TileCollider.FindDistance(
					new Vector2I(wallRadius, 0), false, Constants.Direction.Positive);
				
				if (wallDist < 0)
				{
					Position += new Vector2(wallDist, 0f);
					Speed.X = 0;
					
					return;
				}
			}
			else if (roofDistance < 0)
			{
				Position -= new Vector2(0f, roofDistance);
				if (moveQuadrant == 2 && Angles.GetQuadrant(roofAngle) % 2 > 0 && Action != Actions.Flight)
				{
					Angle = roofAngle;
					GroundSpeed = roofAngle < 180 ? -Speed.Y : Speed.Y;
					Speed.Y = 0;
					
					Land();
				}
				else
				{
					if (Speed.Y < 0f)
					{
						Speed.Y = 0f;
					}
						
					if (Action == Actions.Flight)
					{
						Gravity	= GravityType.TailsDown;
					}
				}
				
				return;
			}
		}
		
		// Perform floor collision if not moving mostly up
		int distance;
		float angle;

		if (moveQuadrant == 0)
		{
			(int distanceL, float angleL) = TileCollider.FindTile(
				Radius.Shuffle(-1, 1), 
				true, Constants.Direction.Positive);
			
			(int distanceR, float angleR) = TileCollider.FindTile(
				Radius.Shuffle(1, 1), 
				true, Constants.Direction.Positive);

			if (distanceL > distanceR)
			{
				distance = distanceR;
				angle = angleR;
			}
			else
			{
				distance = distanceL;
				angle = angleL;
			}
					
			float minClip = -(Speed.Y + 8f);		
			if (distance >= 0 || minClip >= distanceL && minClip >= distanceR) return;
					
			if (Angles.GetQuadrant(angle) > 0)
			{
				if (Speed.Y > 15.75f)
				{
					Speed.Y = 15.75f;
				}
						
				GroundSpeed = angle < 180f ? -Speed.Y : Speed.Y;
				Speed.X = 0f;
			}
			else if (angle is > 22.5f and <= 337.5f)
			{
				GroundSpeed = angle < 180f ? -Speed.Y : Speed.Y;
				GroundSpeed /= 2f;
			}
			else 
			{
				GroundSpeed = Speed.X;
				Speed.Y = 0f;
			}
		}
		else if (Speed.Y >= 0)
		{
			(distance, angle) = TileCollider.FindClosestTile(
				Radius.Shuffle(-1, 1), 
				Radius.Shuffle(1, 1),
				true, Constants.Direction.Positive);
			
			if (distance >= 0) return;
				
			GroundSpeed = Speed.X;
			Speed.Y = 0;
		}
		else
		{
			return;
		}

		Position += new Vector2(0f, distance);
		Angle = angle;
			
		Land();
	}

	private void ProcessGlideCollision()
	{
		// This script is a modified copy of scr_player_collision_air()
		
		if (Action != Actions.Glide) return;
		
		int wallRadius = RadiusNormal.X + 1;
		byte moveQuad = Angles.GetQuadrant(Angles.GetVector256(Speed));
		
		var collisionFlagWall = false;
		var collisionFlagFloor = false;
		var climbY = (int)Position.Y;
		
		TileCollider.SetData((Vector2I)Position, TileLayer, TileMap);
		
		// Perform left wall collision if not moving mostly right
		if (moveQuad != 1)
		{
			int wallDistance = TileCollider.FindDistance(
				new Vector2I(-wallRadius, 0), false, Constants.Direction.Negative);

			if (wallDistance < 0)
			{
				Position -= new Vector2(wallDistance, 0f);
				TileCollider.Position = (Vector2I)Position;
				Speed.X = 0;
				collisionFlagWall = true;
			}
		}
		
		// Perform right wall collision if not moving mostly left
		if (moveQuad != 3)
		{
			int wallDistance = TileCollider.FindDistance(
				new Vector2I(wallRadius, 0), false, Constants.Direction.Positive);
			
			if (wallDistance < 0)
			{
				Position += new Vector2(wallDistance, 0f);
				TileCollider.Position = (Vector2I)Position;
				Speed.X = 0;
				collisionFlagWall = true;
			}
		}
		
		// Perform ceiling collision if not moving mostly down
		if (moveQuad != 0)
		{
			int roofDistance = TileCollider.FindClosestDistance(
				Radius.Shuffle(-1, -1),
				Radius.Shuffle(1, -1),
				true, Constants.Direction.Negative);

			if (moveQuad == 3 && roofDistance <= -14 && SharedData.PlayerPhysics >= PhysicsTypes.S3)
			{
				// Perform right wall collision instead if moving mostly left and too far into the ceiling
				int findDistance = TileCollider.FindDistance(
					new Vector2I(wallRadius, 0), false, Constants.Direction.Positive);
				
				if (findDistance < 0)
				{
					Position += new Vector2(findDistance, 0f);
					TileCollider.Position = (Vector2I)Position;
					Speed.X = 0;
					collisionFlagWall = true;
				}
			}
			else if (roofDistance < 0)
			{
				Position -= new Vector2(0f, roofDistance);
				TileCollider.Position = (Vector2I)Position;
				if (Speed.Y < 0 || moveQuad == 2)
				{
					Speed.Y = 0;
				}
			}
		}
		
		// Perform floor collision if not moving mostly up
		if (moveQuad != 2)
		{
			(int floorDistance, float floorAngle) = TileCollider.FindClosestTile(
				Radius.Shuffle(-1, 1), 
				Radius.Shuffle(1, 1),
				true, Constants.Direction.Positive);
		
			if ((GlideStates)ActionState == GlideStates.Ground)
			{
				if (floorDistance > 14f)
				{
					ReleaseGlide();
				}
				else
				{
					Position += new Vector2(0f, floorDistance);
					Angle = floorAngle;
				}
				
				return;
			}

			if (floorDistance < 0f)
			{
				Position += new Vector2(0f, floorDistance);
				TileCollider.Position = (Vector2I)Position;
				Angle = floorAngle;
				Speed.Y = 0;
				collisionFlagFloor = true;
			}
		}
		
		// Land logic
		if (collisionFlagFloor)
		{
			switch ((GlideStates)ActionState)
			{
				case GlideStates.Air when Angles.GetQuadrant(Angle) == 0:
					Sprite.AnimationType = Animations.GlideGround;
					ActionState = (int)GlideStates.Ground;
					ActionValue = 0;
					Gravity = 0;
					break;
				
				case GlideStates.Air:
					GroundSpeed = Angle < 180 ? Speed.X : -Speed.X;
					Land();
					break;
				
				case GlideStates.Fall:
					Land();
					//TODO: audio
					//audio_play_sfx(sfx_land);
				
					if (Angles.GetQuadrant(Angle) != 0)
					{
						GroundSpeed = Speed.X;
						break;
					}
					
					Sprite.AnimationType = Animations.GlideLand;
					GroundLockTimer = 16;
					GroundSpeed = 0;
					Speed.X = 0;
					break;
				
				case GlideStates.Ground:
					break;
			}
		}
		
		// Wall attach logic
		else if (collisionFlagWall)
		{
			if ((GlideStates)ActionState != GlideStates.Air) return;

			// Cast a horizontal sensor just above Knuckles. If the distance returned is not 0, he is either inside the ceiling or above the floor edge
			TileCollider.Position.Y = climbY - Radius.Y;
			int wallDistance = TileCollider.FindDistance(
				new Vector2I(wallRadius * (int)Facing, 0), false, Facing);
			
			if (wallDistance != 0)
			{
				// Cast a vertical sensor now. If the distance returned is negative, Knuckles is inside
				// the ceiling, else he is above the edge
				
				// Note: _find_mode is set to 2. LBR tiles are not ignored in this case
				TileCollider.GroundMode = Constants.GroundMode.Ceiling;
				int floorDistance = TileCollider.FindDistance(
					new Vector2I((wallRadius + 1) * (int)Facing, -1), 
					true, Constants.Direction.Positive);
				
				if (floorDistance is < 0 or >= 12)
				{
					ReleaseGlide();
					return;
				}
				
				// Adjust Knuckles' Y position to place him just below the edge
				Position += new Vector2(0f, floorDistance);
			}
			
			if (Facing == Constants.Direction.Negative)
			{
				Position += new Vector2(1f, 0f);
			}
			
			Sprite.AnimationType = Animations.ClimbWall;
			Action = Actions.Climb;
			ActionState = (int)ClimbStates.Normal;
			ActionValue = 0;
			GroundSpeed = 0;
			Speed.Y = 0;
			Gravity	= 0;
			
			//TODO: audio
			//audio_play_sfx(sfx_grab);
		}
	}

	private void ReleaseGlide()
	{
		Sprite.AnimationType = Animations.GlideFall;
		ActionState = (int)GlideStates.Fall;
		ActionValue = 0;
		Radius.X = RadiusNormal.X;
		Radius.Y = RadiusNormal.Y;
		
		ResetGravity();
	}

	private void ProcessCarry()
	{
		if (Type != Types.Tails || CarryTimer > 0 && --CarryTimer != 0) return;
	
		if (CarryTarget == null)
		{
			if (Action != Actions.Flight) return;
		
			// Try to grab another player
			foreach (Player player in Players)
			{
				if (player == this) continue;

				if (player.Action is Actions.SpinDash or Actions.Carried) continue;
			
				float distanceX = Mathf.Floor(player.Position.X - Position.X);
				if (distanceX is < -16f or >= 16f) continue;
			
				float distanceY = Mathf.Floor(player.Position.Y - Position.Y);
				if (distanceY is < 32f or >= 48f) continue;
				
				player.ResetState();
				//TODO: audio
				//audio_play_sfx(sfx_grab);
			
				player.Sprite.AnimationType = Animations.Grab;
				player.Action = Actions.Carried;
				CarryTarget = player;

				player.AttachToPlayer(this);
			}
		}
		else
		{
			CarryTarget.OnPlayerAttached(this);
		}
	}

	private void OnPlayerAttached(Player carrier)
	{
		Vector2 previousPosition = carrier.CarryTargetPosition;
				
		if (InputPress.Abc)
		{
			carrier.CarryTarget = null;
			carrier.CarryTimer = 18f;
				
			IsSpinning = true;
			IsJumping = true;
			Action = Actions.None;
			Sprite.AnimationType = Animations.Spin;
			Radius.X = RadiusSpin.X;
			Radius.Y = RadiusSpin.Y;
			Speed.X = 0f;
			Speed.Y = PhysicParams.MinimalJumpVelocity;
					
			if (InputDown.Left)
			{
				Speed.X = -2;
			}
			else if (InputDown.Right)
			{
				Speed.X = 2;
			}
					
			//TODO: audio
			//audio_play_sfx(sfx_jump);
			
		}
		else if (carrier.Action != Actions.Flight 
		    || !Mathf.IsEqualApprox(Position.X, previousPosition.X) 
		    || !Mathf.IsEqualApprox(Position.Y, previousPosition.Y))
		{
			carrier.CarryTarget = null;
			carrier.CarryTimer = 60;
			Action = Actions.None;
		}
		else
		{
			AttachToPlayer(carrier);
		}
	}

	private void AttachToPlayer(Player carrier)
	{
		Facing = carrier.Facing;
		Speed.X = carrier.Speed.X;
		Speed.Y = carrier.Speed.Y;
		Position = carrier.Position + new Vector2(0f, 28f);
		Scale = new Vector2(Math.Abs(Scale.X) * (float)carrier.Facing, Scale.Y);
		
		carrier.CarryTargetPosition = Position;
	}
}
