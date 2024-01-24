using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Spawnable.Barrier;
using static OrbinautFramework3.Objects.Player.PlayerConstants;

namespace OrbinautFramework3.Objects.Player;

public abstract partial class PhysicalPlayerWithAbilities : ObjectInteractivePlayer, ICarrier, ICarried
{
	protected int ClimbAnimationFrameNumber;
	
	protected PhysicalPlayerWithAbilities()
	{
		LandHandler += ReleaseDropDash;
		LandHandler += ReleaseHammerSpin;
	}

	public override void _Process(double delta)
	{
		if (Action is Actions.ObjectControl or Actions.Transform) return;

		UpdatePhysicParams();
		
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
		
		ProcessGlideCollision();
		Carry();
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
			Velocity.Vector = new Vector2(0f, PhysicParams.MinimalJumpVelocity);
					
			if (Input.Down.Left)
			{
				Velocity.X = -2f;
			}
			else if (Input.Down.Right)
			{
				Velocity.X = 2f;
			}
					
			//TODO: audio
			//audio_play_sfx(sfx_jump);
			
		}
		else if (carrier.Action != Actions.Flight || !Position.IsEqualApprox(previousPosition))
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
	
	private bool ProcessSpinDash()
	{
		if (!SharedData.SpinDash || !IsGrounded) return false;
	
		// Start Spin Dash (initial charge)
		if (Action == Actions.None && Animation is Animations.Duck or Animations.GlideLand)
		{
			if (!Input.Press.Abc || !Input.Down.Down) return false;
			
			Animation = Animations.SpinDash;
			Action = Actions.SpinDash;
			ActionValue = 0f;
			ActionValue2 = 1f;
			Velocity.Vector = Vector2.Zero;
			
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
				ActionValue = Math.Min(ActionValue + 2f, 8f);
				
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
				ActionValue -= MathF.Floor(ActionValue * 8f) / 256f * FrameworkData.ProcessSpeed;
			}

			return false;
		}

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
		GroundSpeed = (baseSpeed + MathF.Round(ActionValue) / 2f) * (float)Facing;
			
		//TODO: audio
		//audio_stop_sfx(sfx_charge);
		//audio_play_sfx(sfx_release);

		if (!SharedData.FixDashRelease) return true;
			
		Velocity.SetDirectionalValue(GroundSpeed, Angle);
		return true;
	}
	
	private bool ProcessPeelOut()
	{
		if (!SharedData.PeelOut || Type != Types.Sonic || Id > 0 || !IsGrounded) return false;
	
		// Start Super Peel Out
		if (Action == Actions.None && Animation == Animations.LookUp && Input.Down.Up && Input.Press.Abc)
		{
			Animation = Animations.Move;
			Action = Actions.PeelOut;
			ActionValue = 0f;
			ActionValue2 = 0f;
			
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
				ActionValue += FrameworkData.ProcessSpeed;
			}

			float acceleration = 0.390625f * (float)Facing * FrameworkData.ProcessSpeed;
			ActionValue2 = Math.Clamp(ActionValue2 + acceleration, -launchSpeed, launchSpeed);
			GroundSpeed = ActionValue2;
			return false;
		}

		//TODO: audio
		//audio_stop_sfx(sfx_charge2);
		Action = Actions.None;
		
		if (ActionValue < 30f)
		{
			GroundSpeed = 0f;
			return false;
		}
			
		if (!SharedData.CDCamera && Id == 0)
		{
			Camera.Main.Delay.X = 16f;
		}

		//TODO: audio
		//audio_play_sfx(sfx_release2);
			
		if (!SharedData.FixDashRelease) return true;
			
		Velocity.SetDirectionalValue(GroundSpeed, Angle);
		return true;
	}

	private bool ProcessJump()
	{
		if (!IsJumping || IsGrounded) return false;
		
		if (!Input.Down.Abc)
		{
			Velocity.MaxY(PhysicParams.MinimalJumpVelocity);
		}
		
		if (Velocity.Y < PhysicParams.MinimalJumpVelocity || Id > 0 && CpuInputTimer == 0) return false;
		
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
			ActionValue = SharedData.PlayerPhysics >= PhysicsTypes.S3 ? 26f : 36f;
			Animation = Animations.Transform;
			
			// return player control routine
			return true;
		}
		
		switch (Type)
		{
			case Types.Sonic: JumpSonic(); break;
			case Types.Tails: JumpTails(); break;
			case Types.Knuckles: JumpKnuckles(); break;
			case Types.Amy: JumpAmy(); break;
		}
		
		return false;
	}

	private void JumpSonic()
	{
		if (SharedData.DropDash && Action == Actions.None && !Input.Down.Abc)
		{
			if (Barrier.Type <= Barrier.Types.Normal || IsSuper)
			{
				Action = Actions.DropDash;
				ActionValue = 0f;
			}
		}
		
		// Barrier abilities
		if (!Input.Press.Abc || IsSuper || Barrier.State != Barrier.States.None || ItemInvincibilityTimer != 0) return;
		
		Barrier.State = Barrier.States.Active;
		IsAirLock = false;
		
		switch (Barrier.Type)
		{
			case Barrier.Types.None: JumpDoubleSpin(); break;
			case Barrier.Types.Water: JumpWaterBarrier(); break;
			case Barrier.Types.Flame: JumpFlameBarrier(); break;
			case Barrier.Types.Thunder: JumpThunderBarrier(); break;
		}
	}

	private void JumpDoubleSpin()
	{
		if (!SharedData.DoubleSpin) return;
				
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
	}

	private void JumpWaterBarrier()
	{
		Velocity.Vector = new Vector2(0f, 8f);
				
		Barrier.UpdateFrame(0, 1, [1, 2]);
		Barrier.UpdateDuration([6, 18]);
		Barrier.AnimationTimer = 25f;
				
		//TODO: audio
		//audio_play_sfx(sfx_barrier_water2);
	}
	
	private void JumpFlameBarrier()
	{
		if (!SharedData.CDCamera)
		{
			Camera.Main.Delay.X = 16f;
		}
				
		IsAirLock = true;
		Velocity.Vector = new Vector2(8f * (float)Facing, 0f);
					

		// TODO: SetAnimation
		//Barrier.SetAnimation(, [2]);
		ZIndex = -1;
				
		Barrier.AnimationTimer = 24f;
				
		//TODO: audio
		//audio_play_sfx(sfx_barrier_flame2);
	}

	private void JumpThunderBarrier()
	{
		Barrier.State = Barrier.States.Disabled;
		Velocity.Y = -5.5f;
				
		for (var i = 0; i < 4; i++)
		{
			//TODO: obj_barrier_sparkle
			//instance_create(x, y, obj_barrier_sparkle, { Sparkle_ID: i });
		}
		//TODO: audio
		//audio_play_sfx(sfx_barrier_thunder2);
	}

	private void JumpTails()
	{
		if (Action > 0 || !Input.Press.Abc) return;
				
		IsAirLock = false;
		IsSpinning = false;
		IsJumping = false;
		Gravity	= GravityType.TailsDown;
		Action = Actions.Flight;
		ActionValue = 480f;
				
		Radius = RadiusNormal;
				
		if (!IsUnderwater)
		{
			//TODO: audio
			//audio_play_sfx(sfx_flight, true);
		}

		Input.Down = Input.Down with { Abc = false };
		Input.Press = Input.Press with { Abc = false };
	}

	private void JumpKnuckles()
	{
		if (Action > 0 || !Input.Press.Abc) return;
				
		IsAirLock = false;
		IsSpinning = false;
		IsJumping = false;	
		Animation = Animations.GlideAir;	
		Action = Actions.Glide;
		ActionState = (int)GlideStates.Air;
		ActionValue = Facing == Constants.Direction.Negative ? 0f : 180f;
		Radius = new Vector2I(10, 10);
		GroundSpeed = 4f;
		Velocity.X = 0f;
		Velocity.Y += 2f; 
				
		if (Velocity.Y < 0f)
		{
			Velocity.Y = 0f;
		}
	}

	private void JumpAmy()
	{
		if (Action > 0 || !Input.Press.Abc) return;
				
		if (SharedData.NoRollLock)
		{
			IsAirLock = false;	
		}
		Animation = Animations.HammerSpin;
		Action = Actions.HammerSpin;
		ActionValue = 0f;
		// TODO: audio
		//audio_play_sfx(sfx_hammer_spin);
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
		Velocity.Vector += PhysicParams.JumpVelocity * new Vector2(MathF.Sin(radians), MathF.Cos(radians));
		
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
		if (IsGrounded || CancelDropDash()) return;
		
		if (Input.Down.Abc)
		{
			IsAirLock = false;		
			if (ActionValue < MaxDropDashCharge)
			{
				ActionValue += FrameworkData.ProcessSpeed;
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

		switch (ActionValue)
		{
			case <= 0f:
				return;
			
			case >= MaxDropDashCharge:
				Animation = Animations.Spin;
				Action = Actions.DropDashCancel;
				break;
		}

		ActionValue = 0f;
	}
	
	private void ReleaseDropDash()
	{
		if (CancelDropDash()) return;
		
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

	private bool CancelDropDash()
	{
		if (!SharedData.DropDash || Action != Actions.DropDash) return true;

		if (Barrier.Type <= Barrier.Types.Normal || IsSuper) return false;
		
		Animation = Animations.Spin;
		Action = Actions.None;
		return true;
	}

	private void UpdateDropDashGroundSpeed(float limitSpeed, float force)
	{
		var sign = (float)Facing;
		limitSpeed *= sign;
		force *= sign;
		
		if (Velocity.X * sign >= 0f)
		{
			GroundSpeed = MathF.Floor(GroundSpeed / 4f) + force;
			if (sign * GroundSpeed <= limitSpeed) return;
			GroundSpeed = limitSpeed;
			return;
		}

		GroundSpeed = force;
		if (Mathf.IsEqualApprox(Angle, 360f)) return;
		
		GroundSpeed += MathF.Floor(GroundSpeed / 2f);
	}
	
	private void ProcessFlight()
	{
		if (Action != Actions.Flight) return;
	
		if (ActionValue > 0f)
		{
			ActionValue -= FrameworkData.ProcessSpeed;
			if (ActionValue <= 0f)
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
					else if (Velocity.Y < -1f)
					{
						Gravity = GravityType.TailsDown;
					}

					Velocity.MaxY(-4f);
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
				//TODO: floating point && delta && skill issue
				ActionValue += FrameworkData.ProcessSpeed;
				switch (ActionValue)
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
		UpdateVerticalSpeedOnClimb(ClimbAnimationFrameNumber * stepsPerFrame);
		
		int radiusX = Radius.X;
		if (Facing == Constants.Direction.Negative)
		{
			radiusX++;
		}
		
		TileCollider.SetData((Vector2I)Position, TileLayer, TileMap);

		if (Velocity.Y < 0 ? ClimbUpOntoWall(radiusX) : ReleaseClimbing(radiusX)) return;
		
		if (!Input.Press.Abc)
		{
			if (Velocity.Y != 0)
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
		Velocity.Vector = new Vector2(3.5f * (float)Facing, PhysicParams.MinimalJumpVelocity);
			
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
			ActionValue = 0f;
			Velocity.Y = 0f;
			return true;
		}

		// If Knuckles has encountered a small dip in the wall, cancel climb movement
		if (wallDistance != 0)
		{
			Velocity.Y = 0f;
		}

		// If Knuckles has bumped into the ceiling, cancel climb movement and push him out
		offset = new Vector2I(radiusX * (int)Facing, 1 - RadiusNormal.Y);
		int ceilDistance = TileCollider.FindDistance(offset, true, Constants.Direction.Negative);

		if (ceilDistance >= 0) return false;
		Position -= new Vector2(0f, ceilDistance);
		Velocity.Y = 0f;
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
		Velocity.Y = 0f;
				
		return true;
	}

	private void UpdateVerticalSpeedOnClimb(int maxValue)
	{
		if (Input.Down.Up)
		{
			ActionValue += FrameworkData.ProcessSpeed;
			if (ActionValue > maxValue)
			{
				ActionValue = 0f;
			}

			Velocity.Y = -PhysicParams.AccelerationClimb;
			return;
		}
		
		if (Input.Down.Down)
		{
			ActionValue -= FrameworkData.ProcessSpeed;
			if (ActionValue < 0f)
			{
				ActionValue = maxValue;
			}

			Velocity.Y = PhysicParams.AccelerationClimb;
			return;
		}

		Velocity.Y = 0f;
	}

	private void ReleaseClimb()
	{
		Animation = Animations.GlideFall;
		Action = Actions.Glide;
		ActionState = (int)GlideStates.Fall;
		ActionValue = 1f;
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
		Velocity.X *= 0.25f;

		ResetGravity();
	}

	private void UpdateGlideSpeed()
	{
		const float glideAcceleration = 0.03125f;
		
		if (GroundSpeed < 4f)
		{
			GroundSpeed.Acceleration = glideAcceleration;
			return;
		}

		if (ActionValue % 180f != 0f) return;
		GroundSpeed.Acceleration = PhysicParams.AccelerationGlide;
		GroundSpeed.Min(24f);
	}

	private void UpdateGlideGravityAndHorizontalSpeed()
	{
		const float glideGravity = 0.125f;

		Velocity.X = GroundSpeed * -MathF.Cos(Mathf.DegToRad(ActionValue));
		Gravity = Velocity.Y < 0.5f ? glideGravity : -glideGravity;
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
				
		if (Velocity.X == 0f)
		{
			Land();
			OverrideAnimationFrame = 1;

			Animation = Animations.GlideGround;
			GroundLockTimer = 16f;
			GroundSpeed = 0f;

			return;
		}

		if (ActionValue % 4f < FrameworkData.ProcessSpeed)
		{
			//TODO: obj_dust_skid
			//instance_create(x, y + Radius.Y, obj_dust_skid);
		}
				
		if (ActionValue > 0f && ActionValue % 8f < FrameworkData.ProcessSpeed)
		{
			//TODO: audio
			//audio_play_sfx(sfx_slide);
		}
					
		ActionValue += FrameworkData.ProcessSpeed;
	}

	private void GlideGroundUpdateSpeedX()
	{
		const float slideFriction = -0.09375f;
		
		if (!Input.Down.Abc)
		{
			Velocity.X = 0f;
			return;
		}
		
		float speedX = Velocity.X;
		Velocity.AccelerationX = Math.Sign(Velocity.X) * slideFriction;
		switch (speedX)
		{
			case > 0f: Velocity.MaxX(0f); break;
			case < 0f: Velocity.MinX(0f); break;
		}
	}

	private void GlideAirTurnAround()
	{
		float speed = Angles.ByteAngleStep * FrameworkData.ProcessSpeed;
		if (Input.Down.Left && !Mathf.IsZeroApprox(ActionValue))
		{
			ActionValue = (ActionValue > 0f ? -ActionValue : ActionValue) + speed;
			return;
		}
		
		if (Input.Down.Right && !Mathf.IsEqualApprox(ActionValue, 180f))
		{
			ActionValue = (ActionValue < 0f ? -ActionValue : ActionValue) + speed;
			return;
		}
		
		if (Mathf.IsZeroApprox(ActionValue % 180f)) return;
		ActionValue += speed;
	}
	
	private void ChargeHammerSpin()
	{
		if (Action != Actions.HammerSpin || IsGrounded) return;
		
		if (Input.Down.Abc)
		{
			ActionValue += FrameworkData.ProcessSpeed;
			if (ActionValue >= MaxDropDashCharge)
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
		
		ActionValue += FrameworkData.ProcessSpeed;
		if (ActionValue >= 60f || GroundSpeed == 0f || PushingObject != null || 
		    MathF.Cos(Mathf.DegToRad(Angle)) <= 0f)
		{
			Action = Actions.None;
		}

		if (Input.Press.Left && GroundSpeed > 0f || Input.Press.Right && GroundSpeed < 0f)
		{
			Facing = (Constants.Direction)(-(int)Facing);
			GroundSpeed *= -1f;
		}
		
		Velocity.SetDirectionalValue(GroundSpeed, Angle);
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
		byte moveQuad = Angles.GetQuadrant(Angles.GetVector256(Velocity));
		
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
				Velocity.X = 0f;
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
				Velocity.X = 0f;
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
					Velocity.X = 0f;
					collisionFlagWall = true;
				}
			}
			else if (roofDistance < 0)
			{
				Position -= new Vector2(0f, roofDistance);
				TileCollider.Position = (Vector2I)Position;
				if (Velocity.Y < 0f || moveQuad == 2)
				{
					Velocity.Y = 0f;
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
				Velocity.Y = 0f;
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
					ActionValue = 0f;
					Gravity = 0f;
					break;
				
				case GlideStates.Air:
					GroundSpeed = Angle < 180 ? Velocity.X : -Velocity.X;
					Land();
					break;
				
				case GlideStates.Fall:
					Land();
					//TODO: audio
					//audio_play_sfx(sfx_land);
				
					if (Angles.GetQuadrant(Angle) != 0)
					{
						GroundSpeed = Velocity.X;
						break;
					}
					
					Animation = Animations.GlideLand;
					GroundLockTimer = 16f;
					GroundSpeed = 0f;
					Velocity.X = 0f;
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
			ActionValue = 0f;
			GroundSpeed = 0f;
			Velocity.Y = 0f;
			Gravity	= 0f;
			
			//TODO: audio
			//audio_play_sfx(sfx_grab);
		}
	}

	private void ReleaseGlide()
	{
		Animation = Animations.GlideFall;
		ActionState = (int)GlideStates.Fall;
		ActionValue = 0f;
		Radius = RadiusNormal;
		
		ResetGravity();
	}

	private void Carry()
	{
		if (Type != Types.Tails) return;

		if (CarryTimer > 0f)
		{
			CarryTimer -= FrameworkData.ProcessSpeed;
			if (CarryTimer > 0f) return;
		}
	
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
	
	private void AttachToPlayer(ICarrier carrier)
	{
		Facing = carrier.Facing;
		Velocity.Vector = carrier.Velocity.Vector;
		Position = carrier.Position + new Vector2(0f, 28f);
		Scale = new Vector2(Math.Abs(Scale.X) * (float)carrier.Facing, Scale.Y);
		
		carrier.CarryTargetPosition = Position;
	}
}
