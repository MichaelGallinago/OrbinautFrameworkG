using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Spawnable.Barrier;
using static OrbinautFramework3.Objects.Player.PlayerConstants;

namespace OrbinautFramework3.Objects.Player;

public abstract partial class PhysicalPlayerWithAbilities : BasicPhysicalPlayer, ICarrier, ICarried
{
	protected PhysicalPlayerWithAbilities()
	{
		LandHandler += ReleaseDropDash;
		LandHandler += ReleaseHammerSpin;
	}
	
	protected void UpdatePhysics()
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

		ProcessCorePhysics();
		
		// Late abilities logic
		ProcessGlideCollision();
		Carry();
	}

	private bool ProcessSpinDash()
	{
		if (!SharedData.SpinDash || !IsGrounded) return false;
	
		// Start Spin Dash (initial charge)
		if (Action == Actions.None && Animation is Animations.Duck or Animations.GlideLand)
		{
			if (!Input.Press.Abc || !Input.Down.Down) return false;
			
			Animation = Animations.SpinDash;
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
	
		if (Input.Down.Down)
		{
			if (Input.Press.Abc)
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
				OverrideAnimationFrame = 0;
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
			Animation = Animations.Spin;
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
		if (Action == Actions.None && Animation == Animations.LookUp && Input.Down.Up && Input.Press.Abc)
		{
			Animation = Animations.Move;
			Action = Actions.PeelOut;
			ActionValue = 0;
			ActionValue2 = 0;
			
			//TODO: audio
			//audio_play_sfx(sfx_charge2, [1.00, 2.30]);
		}
	
		// Continue if Super Peel Out is being performed
		if (Action != Actions.PeelOut) return false;
	
		float launchSpeed = PhysicParams.AccelerationTop * (ItemSpeedTimer > 0f || IsSuper ? 1.5f : 2f);
		if (Input.Down.Up)
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
		
		if (!Input.Down.Abc)
		{
			Speed = Speed with { Y = Math.Max(Speed.Y, PhysicParams.MinimalJumpVelocity) };
		}
		
		if (Speed.Y < PhysicParams.MinimalJumpVelocity || Id > 0 && CpuInputTimer == 0) return false;
		
		if (Input.Press.C && SharedData.EmeraldCount == 7 && !IsSuper && RingCount >= 50)
		{
			ResetState();
			//TODO: audio
			//audio_play_sfx(sfx_transform);
			//audio_play_bgm(bgm_super);
			//TODO: instance_create obj_star_super
			//instance_create(x, y, obj_star_super, { TargetPlayer: id });
				
			ObjectInteraction = false;			
			InvincibilityTimer = 0;
			IsSuper = true;
			Action = Actions.Transform;
			ActionValue = SharedData.PlayerPhysics >= PhysicsTypes.S3 ? 26 : 36;
			Animation = Animations.Transform;
			
			// return player control routine
			return true;
		}
		
		switch (Type)
		{
			case Types.Sonic:
				if (SharedData.DropDash && Action == Actions.None && !Input.Down.Abc)
				{
					if (Barrier.Type <= Barrier.Types.Normal || IsSuper)
					{
						Action = Actions.DropDash;
						ActionValue = 0;
					}
				}
				
				// Barrier abilities
				if (!Input.Press.Abc || IsSuper || Barrier.State != Barrier.States.None || ItemInvincibilityTimer != 0) break;
				
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
						Speed = Speed with { Y = -5.5f };
						
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
				if (Action > 0 || !Input.Press.Abc) break;
				
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

				Input.Down = Input.Down with { Abc = false };
				Input.Press = Input.Press with { Abc = false };
				break;
			
			case Types.Knuckles:
				if (Action > 0 || !Input.Press.Abc) break;
				
				IsAirLock = false;
				IsSpinning = false;
				IsJumping = false;	
				Animation = Animations.GlideAir;	
				Action = Actions.Glide;
				ActionState = (int)GlideStates.Air;
				ActionValue = Facing == Constants.Direction.Negative ? 0f : 180f;
				Radius = new Vector2I(10, 10);
				GroundSpeed = 4f;
				Speed = new Vector2(0f, Speed.Y + 2f);
				
				if (Speed.Y < 0)
				{
					Speed = Speed with { Y = 0 };
				}
				break;
			
			case Types.Amy:
				if (Action > 0 || !Input.Press.Abc) break;
				
				if (SharedData.NoRollLock)
				{
					IsAirLock = false;	
				}
				Animation = Animations.HammerSpin;
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
		
		if (!Input.Press.Abc || !CheckCeilingDistance()) return false;
		
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
	
		Animation = Animations.Spin;
		
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
		
		if (Input.Down.Abc)
		{
			IsAirLock = false;		
			if (ActionValue < MaxDropDashCharge)
			{
				ActionValue++;
			}
			else 
			{
				if (Animation != Animations.DropDash)
				{
					Animation = Animations.DropDash;
					//TODO: audio
					//audio_play_sfx(sfx_charge);
				}
			}
			
			return;
		}

		if (ActionValue <= 0f) return;
		
		if (Mathf.IsEqualApprox(ActionValue, MaxDropDashCharge))
		{		
			Animation = Animations.Spin;
			Action = Actions.DropDashCancel;
		}
			
		ActionValue = 0f;
	}
	
	private void ReleaseDropDash()
	{
		if (!SharedData.DropDash || Action != Actions.DropDash) return;

		if (ActionValue < MaxDropDashCharge) return;
		
		Position += new Vector2(0f, Radius.Y - RadiusSpin.Y);
		Radius = RadiusSpin;

		if (IsSuper)
		{
			UpdateDropDashGroundSpeed(13f, 12f);
			Camera.Main?.SetShakeTimer(6);
		}
		else
		{
			UpdateDropDashGroundSpeed(12f, 8f);
		}
		
		Animation = Animations.Spin;
		IsSpinning = true;
		
		if (!SharedData.CDCamera && Camera.Main != null)
		{
			Camera.Main.Delay.X = 8;
		}
			
		//TODO: audio & obj_dust_dropdash
		//instance_create(x, y + Radius.Y, obj_dust_dropdash, { image_xscale: Facing });
		//audio_stop_sfx(sfx_charge);
		//audio_play_sfx(sfx_release);
	}

	private void UpdateDropDashGroundSpeed(float limitSpeed, float force)
	{
		var sign = (float)Facing;
		limitSpeed *= sign;
		force *= sign;
		
		if (Speed.X * sign >= 0f)
		{
			GroundSpeed = Mathf.Floor(GroundSpeed / 4f) + force;
			if (sign * GroundSpeed <= limitSpeed) return;
			GroundSpeed = limitSpeed;
			return;
		}

		GroundSpeed = force;
		if (Mathf.IsEqualApprox(Angle, 360f)) return;
		
		GroundSpeed += Mathf.Floor(GroundSpeed / 2f);
	}
	
	private void SetDropDashGroundSpeed(float force, float limitSpeed, Constants.Direction facing)
	{
		var sign = (float)facing;
		limitSpeed *= sign;
		force *= sign;
		
		if (sign * Speed.X >= 0)
		{
			GroundSpeed = MathF.Floor(GroundSpeed / 4f) + force;
			if (sign * GroundSpeed <= limitSpeed) return;
			GroundSpeed = limitSpeed;
			return;
		}
		
		GroundSpeed = (Mathf.IsEqualApprox(Angle, 360f) ? 0f : MathF.Floor(GroundSpeed / 2f)) + force;
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
					Animation = Animations.FlyTired;
					//TODO: audio
					//audio_play_sfx(sfx_flight2, true);
				}
				else
				{
					Animation = Animations.SwimTired;
				}
			
				Gravity = GravityType.TailsDown;
				//TODO: audio
				//audio_stop_sfx(sfx_flight);
			}
			else
			{	
				Animation = IsUnderwater ? Animations.Swim : Animations.Fly;
			
				if (!IsUnderwater || CarryTarget == null)
				{
					if (Input.Press.Abc)
					{
						Gravity = GravityType.TailsUp;
					}
					else if (Speed.Y < -1f)
					{
						Gravity = GravityType.TailsDown;
					}

					Speed = Speed with { Y = Math.Max(Speed.Y, -4f) };
				}
				else
				{
					Gravity = GravityType.TailsDown;
				}
			}
		}

		if (!SharedData.FlightCancel || !Input.Down.Down || !Input.Press.Abc) return;
		
		Camera.Main.BufferPosition.Y += Radius.Y - RadiusSpin.Y;
		Radius = RadiusSpin;
		Animation = Animations.Spin;
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
						Animation = Animations.ClimbLedge;
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
						Animation = Animations.Idle;
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
		UpdateVerticalSpeedOnClimb(Sprite.SpriteFrames.GetFrameCount(Sprite.Animation) * stepsPerFrame);
		
		int radiusX = Radius.X;
		if (Facing == Constants.Direction.Negative)
		{
			radiusX++;
		}
		
		TileCollider.SetData((Vector2I)Position, TileLayer, TileMap);

		if (Speed.Y < 0 ? ClimbUpOntoWall(radiusX) : ReleaseClimbing(radiusX)) return;
		
		if (!Input.Press.Abc)
		{
			if (Speed.Y != 0)
			{
				OverrideAnimationFrame = Mathf.FloorToInt(ActionValue / stepsPerFrame);
			}
			return;
		}
		
		Animation = Animations.Spin;
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
			Speed = Speed with { Y = 0 };
			return true;
		}

		// If Knuckles has encountered a small dip in the wall, cancel climb movement
		if (wallDistance != 0)
		{
			Speed = Speed with { Y = 0 };
		}

		// If Knuckles has bumped into the ceiling, cancel climb movement and push him out
		offset = new Vector2I(radiusX * (int)Facing, 1 - RadiusNormal.Y);
		int ceilDistance = TileCollider.FindDistance(offset, true, Constants.Direction.Negative);

		if (ceilDistance >= 0) return false;
		Position -= new Vector2(0f, ceilDistance);
		Speed = Speed with { Y = 0 };
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

		Animation = Animations.Idle;
		Speed = Speed with { Y = 0 };
				
		return true;
	}

	private void UpdateVerticalSpeedOnClimb(int maxValue)
	{
		if (Input.Down.Up)
		{
			if (++ActionValue > maxValue)
			{
				ActionValue = 0f;
			}

			Speed = Speed with { Y = -PhysicParams.AccelerationClimb };
			return;
		}
		if (Input.Down.Down)
		{
			if (--ActionValue < 0f)
			{
				ActionValue = maxValue;
			}

			Speed = Speed with { Y = PhysicParams.AccelerationClimb };
			return;
		}

		Speed = Speed with { Y = 0 };
	}

	private void ReleaseClimb()
	{
		Animation = Animations.GlideFall;
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

		if (Input.Down.Abc) return;
		
		Animation = Animations.GlideFall;
		ActionState = (int)GlideStates.Fall;
		ActionValue = 0f;
		Radius = RadiusNormal;
		Speed = Speed with { X = Speed.X * 0.25f };

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

		Speed = Speed with { X = GroundSpeed * -Mathf.Cos(Mathf.DegToRad(ActionValue)) };
		Gravity = Speed.Y < 0.5f ? glideGravity : -glideGravity;
	}

	private void UpdateGlideAirAnimationFrame()
	{
		float angle = Math.Abs(ActionValue) % 180f;
		switch (angle)
		{
			case < 30f or > 150f:
				OverrideAnimationFrame = 0;
				break;
			case < 60f or > 120f:
				OverrideAnimationFrame = 1;
				break;
			default:
				Facing = angle < 90 ? Constants.Direction.Negative : Constants.Direction.Positive;
				OverrideAnimationFrame = 2;
				break;
		}
	}

	private void GlideGround()
	{
		GlideGroundUpdateSpeedX();
				
		if (Speed.X == 0f)
		{
			Land();
			OverrideAnimationFrame = 1;

			Animation = Animations.GlideGround;
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
		
		if (!Input.Down.Abc)
		{
			Speed = Speed with { X = 0f };
			return;
		}

		Speed = Speed with
		{
			X = Speed.X switch
			{
				> 0f => Math.Max(0f, Speed.X - slideFriction),
				< 0f => Math.Min(0f, Speed.X + slideFriction),
				_ => Speed.X
			}
		};
	}

	private void GlideAirTurnAround()
	{
		const float angleIncrement = 2.8125f;
		
		if (Input.Down.Left && !Mathf.IsZeroApprox(ActionValue))
		{
			ActionValue = (ActionValue > 0f ? -ActionValue : ActionValue) + angleIncrement;
			return;
		}
		
		if (Input.Down.Right && !Mathf.IsEqualApprox(ActionValue, 180f))
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
		
		if (Input.Down.Abc)
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
				Animation = Animations.Spin;
				Action = Actions.HammerSpinCancel;
				break;
		}

		ActionValue = 0f;
	}

	private void ReleaseHammerSpin()
	{
		if (Action != Actions.HammerSpin) return;
		
		if (ActionValue < MaxDropDashCharge) return;

		Animation = Animations.HammerDash;
		Action = Actions.HammerDash;
		ActionValue = 0f;
		
		//TODO: audio
		//audio_stop_sfx(sfx_charge);
		//audio_play_sfx(sfx_release);
	}
	
	private void ProcessHammerDash()
	{
		if (Action != Actions.HammerDash) return;

		if (!Input.Down.Abc)
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

		if (Input.Down.Left && GroundSpeed > 0f || Input.Down.Right && GroundSpeed < 0f)
		{
			Facing = (Constants.Direction)(-(int)Facing);
			GroundSpeed *= -1;
		}
		
		Speed = GroundSpeed * new Vector2(cosine, -MathF.Sin(radians));
	}

	private void CancelHammerDash()
	{
		Animation = Animations.Move;
		Action = Actions.None;
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
				Speed = Speed with { X = 0 };
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
				Speed = Speed with { X = 0 };
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
					Speed = Speed with { X = 0 };
					collisionFlagWall = true;
				}
			}
			else if (roofDistance < 0)
			{
				Position -= new Vector2(0f, roofDistance);
				TileCollider.Position = (Vector2I)Position;
				if (Speed.Y < 0 || moveQuad == 2)
				{
					Speed = Speed with { Y = 0 };
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
				Speed = Speed with { Y = 0 };
				collisionFlagFloor = true;
			}
		}
		
		// Land logic
		if (collisionFlagFloor)
		{
			switch ((GlideStates)ActionState)
			{
				case GlideStates.Air when Angles.GetQuadrant(Angle) == 0:
					Animation = Animations.GlideGround;
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
					
					Animation = Animations.GlideLand;
					GroundLockTimer = 16;
					GroundSpeed = 0;
					Speed = Speed with { X = 0 };
					break;
				
				case GlideStates.Ground:
					break;
			}
		}
		
		// Wall attach logic
		else if (collisionFlagWall)
		{
			if ((GlideStates)ActionState != GlideStates.Air) return;

			// Cast a horizontal sensor just above Knuckles.
			// If the distance returned is not 0, he is either inside the ceiling or above the floor edge
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
			
			Animation = Animations.ClimbWall;
			Action = Actions.Climb;
			ActionState = (int)ClimbStates.Normal;
			ActionValue = 0;
			GroundSpeed = 0;
			Speed = Speed with { Y = 0 };
			Gravity	= 0;
			
			//TODO: audio
			//audio_play_sfx(sfx_grab);
		}
	}

	private void ReleaseGlide()
	{
		Animation = Animations.GlideFall;
		ActionState = (int)GlideStates.Fall;
		ActionValue = 0;
		Radius.X = RadiusNormal.X;
		Radius.Y = RadiusNormal.Y;
		
		ResetGravity();
	}

	private void Carry()
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
				
				if (MathF.Floor(player.Position.X - Position.X) is < -16f or >= 16f) continue;
				if (MathF.Floor(player.Position.Y - Position.Y) is < 32f or >= 48f) continue;
				
				player.ResetState();
				//TODO: audio
				//audio_play_sfx(sfx_grab);
			
				player.Animation = Animations.Grab;
				player.Action = Actions.Carried;
				CarryTarget = player;

				player.AttachToPlayer(this);
			}
		}
		else
		{
			CarryTarget.OnAttached(this);
		}
	}
	
	public void OnAttached(ICarrier carrier)
	{
		Vector2 previousPosition = carrier.CarryTargetPosition;
				
		if (Input.Press.Abc)
		{
			carrier.CarryTarget = null;
			carrier.CarryTimer = 18f;
				
			IsSpinning = true;
			IsJumping = true;
			Action = Actions.None;
			Animation = Animations.Spin;
			Radius = RadiusSpin;
			Speed = new Vector2(0f, PhysicParams.MinimalJumpVelocity);
					
			if (Input.Down.Left)
			{
				Speed = Speed with { X = -2 };
			}
			else if (Input.Down.Right)
			{
				Speed = Speed with { X = 2 };
			}
					
			//TODO: audio
			//audio_play_sfx(sfx_jump);
			
		}
		else if (carrier.Action != Actions.Flight 
		    || !Mathf.IsEqualApprox(Position.X, previousPosition.X) 
		    || !Mathf.IsEqualApprox(Position.Y, previousPosition.Y))
		{
			carrier.CarryTarget = null;
			carrier.CarryTimer = 60f;
			Action = Actions.None;
		}
		else
		{
			AttachToPlayer(carrier);
		}
	}
	
	private void AttachToPlayer(ICarrier carrier)
	{
		Facing = carrier.Facing;
		Speed = carrier.Speed;
		Position = carrier.Position + new Vector2(0f, 28f);
		Scale = new Vector2(Math.Abs(Scale.X) * (float)carrier.Facing, Scale.Y);
		
		carrier.CarryTargetPosition = Position;
	}
}
