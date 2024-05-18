using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Spawnable.Shield;
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
		UpdatePhysicParameters();
		
		if (ProcessSpinDash()) return;
		if (ProcessDash()) return;
		if (ProcessJump()) return;
		if (StartJump()) return;
		
		// Abilities logic
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
			Velocity.Vector = new Vector2(0f, PhysicParams.MinimalJumpSpeed);
					
			if (Input.Down.Left)
			{
				Velocity.X = -2f;
			}
			else if (Input.Down.Right)
			{
				Velocity.X = 2f;
			}
			
			AudioPlayer.Sound.Play(SoundStorage.Jump);
			return;
		}
		
		if (Action != Actions.Carried || carrier.Action != Actions.Flight || !Position.IsEqualApprox(previousPosition))
		{
			carrier.CarryTarget = null;
			carrier.CarryTimer = 60f;
			Action = Actions.None;
			return;
		}
		
		AttachToPlayer(carrier);
	}
	
	private bool ProcessSpinDash()
	{
		if (!SharedData.SpinDash || !IsGrounded) return false;
		
		if (StartSpinDash()) return false;
		
		// Continue if Spin Dash is being performed
		if (Action != Actions.SpinDash) return false;
		
		if (ChargeSpinDash()) return false;
		
		SetCameraDelayX(16f);
		
		Position += new Vector2(0f, Radius.Y - RadiusSpin.Y);
		Animation = Animations.Spin;
		Action = Actions.None;
		Radius = RadiusSpin;
		IsSpinning = true;
		
		GroundSpeed.Value = ((SuperTimer > 0 ? 11 : 8) + MathF.Round(ActionValue) / 2f) * (float)Facing;
		
		AudioPlayer.Sound.Stop(SoundStorage.Charge);
		AudioPlayer.Sound.Play(SoundStorage.Release);
		
		if (!SharedData.FixDashRelease) return true;
		Velocity.SetDirectionalValue(GroundSpeed, Angle);
		
		return true;
	}

	private bool StartSpinDash()
	{
		if (Action != Actions.None || Animation is not (Animations.Duck or Animations.GlideLand)) return false;
		if (!Input.Press.Abc || !Input.Down.Down) return true;
		
		Animation = Animations.SpinDash;
		Action = Actions.SpinDash;
		ActionValue = 0f;
		ActionValue2 = 1f;
		Velocity.Vector = Vector2.Zero;
			
		// TODO: SpinDash dust 
		//instance_create(x, y + Radius.Y, obj_dust_spindash, { TargetPlayer: id });
		AudioPlayer.Sound.Play(SoundStorage.Charge);
		
		return true;
	}

	private bool ChargeSpinDash()
	{
		if (!Input.Down.Down) return false;
		
		if (!Input.Press.Abc)
		{
			//TODO: check math with ProcessSpeed
			ActionValue -= MathF.Floor(ActionValue * 8f) / 256f * Scene.Local.ProcessSpeed;
			return true;
		}
		
		ActionValue = Math.Min(ActionValue + 2f, 8f);
		
		ActionValue2 = AudioPlayer.Sound.IsPlaying(SoundStorage.Charge) && ActionValue > 0f ? 
			Math.Min(ActionValue2 + 0.1f, 1.5f) : 1f;
				
		AudioPlayer.Sound.PlayPitched(SoundStorage.Charge, ActionValue2);
		OverrideAnimationFrame = 0;
		
		return true;
	}
	
	private bool ProcessDash()
	{
		if (!SharedData.Dash || Type != Types.Sonic || Id > 0 && CpuInputTimer <= 0f) return false;
		
		StartDash();
		
		if (Action == Actions.Dash && IsGrounded) return !ChargeDash() && ReleaseDash();
		
		if (Action != Actions.Dash)
		{
			AudioPlayer.Sound.Stop(SoundStorage.Charge2);
		}
		return false;
	}

	private void StartDash()
	{
		if (Action != Actions.None || Animation != Animations.LookUp || !Input.Down.Up || !Input.Press.Abc) return;
		
		Animation = Animations.Move;
		Action = Actions.Dash;
		ActionValue = 0f;
		ActionValue2 = 0f;
			
		AudioPlayer.Sound.Play(SoundStorage.Charge2);
	}

	private bool ChargeDash()
	{
		if (!Input.Down.Up) return false;
		
		if (ActionValue < 30f)
		{
			ActionValue += Scene.Local.ProcessSpeed;
		}

		float acceleration = 0.390625f * (float)Facing * Scene.Local.ProcessSpeed;
		float launchSpeed = PhysicParams.AccelerationTop * (ItemSpeedTimer > 0f || SuperTimer > 0f ? 1.5f : 2f);
		ActionValue2 = Math.Clamp(ActionValue2 + acceleration, -launchSpeed, launchSpeed);
		GroundSpeed.Value = ActionValue2;
		return true;
	}

	private bool ReleaseDash()
	{
		AudioPlayer.Sound.Stop(SoundStorage.Charge2);
		Action = Actions.None;
		
		if (ActionValue < 30f)
		{
			GroundSpeed.Value = 0f;
			return false;
		}

		SetCameraDelayX(16f);
		
		AudioPlayer.Sound.Play(SoundStorage.Release2);	
		
		if (!SharedData.FixDashRelease) return true;
		Velocity.SetDirectionalValue(GroundSpeed, Angle);
		return true;
	}

	private bool ProcessJump()
	{
		if (!IsJumping || IsGrounded) return false;
		
		if (!Input.Down.Abc)
		{
			Velocity.MaxY(PhysicParams.MinimalJumpSpeed);
		}
		
		if (Velocity.Y < PhysicParams.MinimalJumpSpeed || Id > 0 && CpuInputTimer == 0) return false;

		if (TransformInJump()) return true;
		
		switch (Type)
		{
			case Types.Sonic: JumpSonic(); break;
			case Types.Tails: JumpTails(); break;
			case Types.Knuckles: JumpKnuckles(); break;
			case Types.Amy: JumpAmy(); break;
		}
		
		return false;
	}

	private bool TransformInJump()
	{
		if (!Input.Press.C || SuperTimer > 0f || SharedData.EmeraldCount != 7 || SharedData.PlayerRings < 50)
		{
			return false;
		}
		
		ResetState();
		AudioPlayer.Sound.Play(SoundStorage.Transform);
		AudioPlayer.Music.Play(MusicStorage.Super);
		//TODO: instance_create obj_star_super
		//instance_create(x, y, obj_star_super, { TargetPlayer: id });

		IsControlRoutineEnabled = false;
		IsObjectInteractionEnabled = false;			
		InvincibilityTimer = 0;
		SuperTimer = 1f;
		Action = Actions.Transform;
		Animation = Animations.Transform;
		ActionValue = SharedData.PlayerPhysics >= PhysicsTypes.S3 ? 26f : 36f;
			
		// return player control routine
		return true;
	}

	private void JumpSonic()
	{
		if (SharedData.DropDash && Action == Actions.None && !Input.Down.Abc)
		{
			if (Shield.Type <= ShieldContainer.Types.Normal || SuperTimer > 0f || ItemInvincibilityTimer > 0f)
			{
				Action = Actions.DropDash;
				ActionValue = 0f;
			}
		}
		
		// Barrier abilities
		if (!Input.Press.Abc || SuperTimer > 0f || 
		    Shield.State != ShieldContainer.States.None || ItemInvincibilityTimer != 0) return;
		
		Shield.State = ShieldContainer.States.Active;
		IsAirLock = false;
		
		switch (Shield.Type)
		{
			case ShieldContainer.Types.None: JumpDoubleSpin(); break;
			case ShieldContainer.Types.Bubble: JumpWaterBarrier(); break;
			case ShieldContainer.Types.Fire: JumpFlameBarrier(); break;
			case ShieldContainer.Types.Lightning: JumpThunderBarrier(); break;
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
		
		Shield.State = ShieldContainer.States.DoubleSpin;
				
		//TODO: obj_double_spin
		//instance_create(x, y, obj_double_spin, { TargetPlayer: id });
		AudioPlayer.Sound.Play(SoundStorage.DoubleSpin);
	}

	private void JumpWaterBarrier()
	{
		Velocity.Vector = new Vector2(0f, 8f);
		//TODO: update shield animation
		Shield.AnimationType = ShieldContainer.AnimationTypes.BubbleBounce;
		AudioPlayer.Sound.Play(SoundStorage.ShieldBubble2);
	}
	
	private void JumpFlameBarrier()
	{
		SetCameraDelayX(16f);
				
		IsAirLock = true;
		Velocity.Vector = new Vector2(8f * (float)Facing, 0f);
		
		//TODO: update shield animation
		if (Shield.AnimationType == ShieldContainer.AnimationTypes.FireDash)
		{
			Shield.Frame = 0;
		}
		else
		{
			Shield.AnimationType = ShieldContainer.AnimationTypes.FireDash;
		}
		
		//TODO: check ZIndex
		ZIndex = -1;
		
		AudioPlayer.Sound.Play(SoundStorage.ShieldFire2);
	}

	private void JumpThunderBarrier()
	{
		Shield.State = ShieldContainer.States.Disabled;
		Velocity.Y = -5.5f;
				
		for (var i = 0; i < 4; i++)
		{
			//TODO: obj_barrier_sparkle
			//instance_create(x, y, obj_barrier_sparkle, { Sparkle_ID: i });
		}
		
		AudioPlayer.Sound.Play(SoundStorage.ShieldLightning2);
	}

	private void JumpTails()
	{
		if (Action != Actions.None || !Input.Press.Abc) return;
		
		IsAirLock = false;
		IsSpinning = false;
		IsJumping = false;
		Gravity	= GravityType.TailsDown;
		Action = Actions.Flight;
		ActionValue = 480f;
		ActionValue2 = 0f;
		Radius = RadiusNormal;
				
		if (!IsUnderwater)
		{
			AudioPlayer.Sound.Play(SoundStorage.Flight);
		}

		Input.Down = Input.Down with { Abc = false };
		Input.Press = Input.Press with { Abc = false };
	}

	private void JumpKnuckles()
	{
		if (Action != Actions.None || !Input.Press.Abc) return;
		
		IsAirLock = false;
		IsSpinning = false;
		IsJumping = false;	
		Animation = Animations.GlideAir;
		Action = Actions.Glide;
		ActionState = (int)GlideStates.Air;
		ActionValue = Facing == Constants.Direction.Negative ? 0f : 180f;
		Radius = new Vector2I(10, 10);
		GroundSpeed.Value = 4f;
		Velocity.X = 0f;
		Velocity.Y += 2f; 
				
		if (Velocity.Y < 0f)
		{
			Velocity.Y = 0f;
		}
	}

	private void JumpAmy()
	{
		if (Action != Actions.None || !Input.Press.Abc) return;
		
		if (SharedData.NoRollLock)
		{
			IsAirLock = false;
		}
		
		Animation = Animations.HammerSpin;
		Action = Actions.HammerSpin;
		ActionValue = 0f;
		
		AudioPlayer.Sound.Play(SoundStorage.Hammer);
	}

	private bool StartJump()
	{
		if (Action is Actions.SpinDash or Actions.Dash || IsForcedSpin || !IsGrounded) return false;
		
		if (!Input.Press.Abc || !CheckCeilingDistance()) return false;
		
		//TODO: check that Triangly fix "!global.player_physics != PHYSICS_CD"
		if (!SharedData.FixJumpSize && SharedData.PlayerPhysics != PhysicsTypes.CD)
		{
			// Why do they even do that?
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
		Velocity.Vector += PhysicParams.JumpSpeed * new Vector2(MathF.Sin(radians), MathF.Cos(radians));
		
		IsSpinning = true;
		IsJumping = true;
		IsGrounded = false;
		OnObject = null;
		SetPushAnimationBy = null;
		IsStickToConvex = false;
		Animation = Animations.Spin;
		
		AudioPlayer.Sound.Play(SoundStorage.Jump);
	
		// Exit control routine
		return true;
	}

	private bool CheckCeilingDistance()
	{
		const int maxCeilingDistance = 6; 
		
		TileCollider.SetData((Vector2I)Position, TileLayer, TileBehaviour);
		
		int distance = TileBehaviour switch
		{
			Constants.TileBehaviours.Floor => TileCollider.FindClosestDistance(
				-Radius.X, -Radius.Y, Radius.X, -Radius.Y,
				true, Constants.Direction.Negative),
			
			Constants.TileBehaviours.RightWall => TileCollider.FindClosestDistance(
				-Radius.Y, -Radius.X, Radius.Y, -Radius.X,
				false, Constants.Direction.Negative),
			
			Constants.TileBehaviours.LeftWall => TileCollider.FindClosestDistance(
				-Radius.Y, Radius.X, Radius.Y, Radius.X,
				false, Constants.Direction.Positive),
			
			Constants.TileBehaviours.Ceiling => maxCeilingDistance,
			
			_ => throw new ArgumentOutOfRangeException()
		};
		
		return distance >= maxCeilingDistance;
	}

	private void ChargeDropDash()
	{
		if (IsGrounded || CancelDropDash()) return;
		
		if (Input.Down.Abc)
		{
			IsAirLock = false;		
			ActionValue += Scene.Local.ProcessSpeed;
			
			if (ActionValue < MaxDropDashCharge || Animation == Animations.DropDash) return;
			
			AudioPlayer.Sound.Play(SoundStorage.Charge3);
			Animation = Animations.DropDash;
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
		
		if (SuperTimer > 0f)
		{
			UpdateDropDashGroundSpeed(13f, 12f);
			if (IsCameraTarget(out ICamera camera))
			{
				camera.SetShakeTimer(6f);
			}
		}
		else
		{
			UpdateDropDashGroundSpeed(12f, 8f);
		}
		
		Animation = Animations.Spin;
		IsSpinning = true;
		
		SetCameraDelayX(8f);
			
		//TODO: obj_dust_dropdash
		//instance_create(x, y + Radius.Y, obj_dust_dropdash, { image_xscale: Facing });
		AudioPlayer.Sound.Stop(SoundStorage.Charge3);
		AudioPlayer.Sound.Play(SoundStorage.Release);
	}

	private bool CancelDropDash()
	{
		if (!SharedData.DropDash || Action != Actions.DropDash) return true;
		
		if (Shield.Type <= ShieldContainer.Types.Normal || SuperTimer > 0f || ItemInvincibilityTimer > 0f) return false;
		
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
			GroundSpeed.Value = MathF.Floor(GroundSpeed / 4f) + force;
			if (sign * GroundSpeed <= limitSpeed) return;
			GroundSpeed.Value = limitSpeed;
			return;
		}
		
		GroundSpeed.Value = force;
		if (Mathf.IsEqualApprox(Angle, 360f)) return;
		
		GroundSpeed.Value += MathF.Floor(GroundSpeed / 2f);
	}
	
	private void ProcessFlight()
	{
		if (Action != Actions.Flight) return;

		if (ActionValue > 0f)
		{
			ActionValue -= Scene.Local.ProcessSpeed;
		}

		if (!FlyUp())
		{
			FlyDown();
		}

		PlayTailsSound();

		if (IsUnderwater)
		{
			Animation = ActionValue > 0f ? Animations.Fly : Animations.FlyTired;
		}
		else
		{
			Animation = ActionValue > 0f ? Animations.Swim : Animations.SwimTired;
		}
	}
	
	protected void PlayTailsSound()
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

	private bool FlyUp()
	{
		if (ActionValue2 <= 0f) return false;

		if (Velocity.Y < -1f)
		{
			ActionValue2 = 0f;
			return true;
		}
		
		Gravity = GravityType.TailsUp;
				
		ActionValue2 += Scene.Local.ProcessSpeed;
		if (ActionValue2 >= 31f)
		{
			ActionValue2 = 0f;
		}

		return true;
	}

	private void FlyDown()
	{
		if (Input.Press.Abc && ActionValue > 0f && (!IsUnderwater || CarryTarget == null))
		{
			//TODO: check that this works
			ActionValue2 = 1f;
		}
			
		Gravity = GravityType.TailsDown;
	}

	private void ProcessClimb()
	{
		if (Action != Actions.Climb) return;
		
		switch ((ClimbStates)ActionState)
		{
			case ClimbStates.Normal: ClimbNormal(); break;
			case ClimbStates.Ledge: ClimbLedge(); break;
		}
	}

	private void ClimbNormal()
	{
		if (!Mathf.IsEqualApprox(Position.X, PreviousPosition.X) || Velocity.X != 0f)
		{
			ReleaseClimb();
			return;
		}
		
		const int stepsPerClimbFrame = 4;
		UpdateVerticalSpeedOnClimb(ClimbAnimationFrameNumber * stepsPerClimbFrame);
		
		int radiusX = Radius.X;
		if (Facing == Constants.Direction.Negative)
		{
			radiusX++;
		}
		
		TileCollider.SetData((Vector2I)Position, TileLayer);

		if (Velocity.Y < 0 ? ClimbUpOntoWall(radiusX) : ReleaseClimbing(radiusX)) return;
		
		if (!Input.Press.Abc)
		{
			// Update animation frame if still climbing
			if (Velocity.Y != 0)
			{
				OverrideAnimationFrame = Mathf.FloorToInt(ActionValue / stepsPerClimbFrame);
			}
			return;
		}
		
		Animation = Animations.Spin;
		IsSpinning = true;
		IsJumping = true;
		Action = Actions.None;
		Facing = (Constants.Direction)(-(int)Facing);
		Velocity.Vector = new Vector2(3.5f * (float)Facing, PhysicParams.MinimalJumpSpeed);
			
		AudioPlayer.Sound.Play(SoundStorage.Jump);
		ResetGravity();
	}

	private bool ClimbUpOntoWall(int radiusX)
	{
		// If the wall is far away from Knuckles then he must have reached a ledge, make him climb up onto it
		int wallDistance = TileCollider.FindDistance(radiusX * (int)Facing, -Radius.Y - 1, false, Facing);
		
		if (wallDistance >= 4)
		{
			ActionState = (int)ClimbStates.Ledge;
			ActionValue = 0f;
			Velocity.Y = 0f;
			Gravity = 0f;
			return true;
		}

		// If Knuckles has encountered a small dip in the wall, cancel climb movement
		if (wallDistance != 0)
		{
			Velocity.Y = 0f;
		}

		// If Knuckles has bumped into the ceiling, cancel climb movement and push him out
		int ceilDistance = TileCollider.FindDistance(
			radiusX * (int)Facing, 1 - RadiusNormal.Y, true, Constants.Direction.Negative);

		if (ceilDistance >= 0) return false;
		Position -= new Vector2(0f, ceilDistance);
		Velocity.Y = 0f;
		return false;
	}

	private bool ReleaseClimbing(int radiusX)
	{
		// If Knuckles is no longer against the wall, make him let go
		if (TileCollider.FindDistance(radiusX * (int)Facing, Radius.Y + 1, false, Facing) == 0)
		{
			return LandAfterClimbing(radiusX);
		}
		
		ReleaseClimb();
		return true;
	}

	private bool LandAfterClimbing(int radiusX)
	{
		(int distance, float angle) = TileCollider.FindTile(
			radiusX * (int)Facing, RadiusNormal.Y, true, Constants.Direction.Positive);

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
			ActionValue += Scene.Local.ProcessSpeed;
			if (ActionValue > maxValue)
			{
				ActionValue = 0f;
			}

			Velocity.Y = -PhysicParams.AccelerationClimb;
			return;
		}
		
		if (Input.Down.Down)
		{
			ActionValue -= Scene.Local.ProcessSpeed;
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
	
	private enum ClimbLedgeStates : byte
	{
		None, Frame0, Frame1, Frame2, End
	}

	private void ClimbLedge()
	{
		//TODO: check this
		
		ClimbLedgeStates previousState = GetClimbLedgeState(ActionValue);
		ActionValue += Scene.Local.ProcessSpeed;
		ClimbLedgeStates state = GetClimbLedgeState(ActionValue);
		if (state == previousState) return;
		
		switch (state)
		{
			case ClimbLedgeStates.Frame0:
				Animation = Animations.ClimbLedge;
				Position += new Vector2(3f * (float)Facing, -3f);
				break;
					
			case ClimbLedgeStates.Frame1:
				Position += new Vector2(8f * (float)Facing, -10f);
				break;
					
			case ClimbLedgeStates.Frame2:
				Position -= new Vector2(8f * (float)Facing, 12f);
				break;
					
			case ClimbLedgeStates.End:
				Land();
				Animation = Animations.Idle;
				Position += new Vector2(8f * (float)Facing, 4f);

				// Subtract that 1px that was applied when we attached to the wall
				if (Facing == Constants.Direction.Negative)
				{
					Position += Vector2.Left;
				}
				break;
		}
	}
	
	private static ClimbLedgeStates GetClimbLedgeState(float value) => value switch
	{
		<= 0f => ClimbLedgeStates.None,
		<= 6f => ClimbLedgeStates.Frame0,
		<= 12f => ClimbLedgeStates.Frame1,
		<= 18f => ClimbLedgeStates.Frame2,
		_ => ClimbLedgeStates.End
	};
	
	private void ProcessGlide()
	{
		if (Action != Actions.Glide || ActionState == (int)GlideStates.Fall) return;
		
		switch ((GlideStates)ActionState)
		{
			case GlideStates.Air: GlideAir(); break;
			case GlideStates.Ground: GlideGround(); break;
		}
	}

	private void GlideAir()
	{
		UpdateGlideSpeed();
		GlideAirTurnAround();
		UpdateGlideGravityAndHorizontalSpeed();
		UpdateGlideAirAnimationFrame();
		
		if (Input.Down.Abc) return;

		ReleaseGlide();
		Velocity.X *= 0.25f;
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
		
		// Stop sliding
		if (Velocity.X == 0f)
		{
			Land();
			OverrideAnimationFrame = 1;
			
			Animation = Animations.GlideGround;
			GroundLockTimer = 16f;
			GroundSpeed.Value = 0f;
			
			return;
		}

		// Spawn dust particles
		if (ActionValue % 4f < Scene.Local.ProcessSpeed)
		{
			//TODO: obj_dust_skid
			//instance_create(x, y + Radius.Y, obj_dust_skid);
		}
				
		if (ActionValue > 0f && ActionValue % 8f < Scene.Local.ProcessSpeed)
		{
			AudioPlayer.Sound.Play(SoundStorage.Slide);
		}
					
		ActionValue += Scene.Local.ProcessSpeed;
	}

	private void GlideGroundUpdateSpeedX()
	{
		if (!Input.Down.Abc)
		{
			Velocity.X = 0f;
			return;
		}
		
		const float slideFriction = -0.09375f;
		
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
		float speed = Angles.ByteAngleStep * Scene.Local.ProcessSpeed;
		if (Input.Down.Left && !Mathf.IsZeroApprox(ActionValue))
		{
			if (ActionValue > 0f)
			{
				ActionValue = -ActionValue;
			}
			
			ActionValue += speed;
			
			if (ActionValue < 0f)
			{
				ActionValue = 0f;
			}
			return;
		}
		
		if (Input.Down.Right && !Mathf.IsEqualApprox(ActionValue, 180f))
		{
			if (ActionValue < 0f)
			{
				ActionValue = -ActionValue;
			}
			
			ActionValue += speed;

			if (ActionValue > 180f)
			{
				ActionValue = 180f;
			}
			return;
		}
		
		if (Mathf.IsZeroApprox(ActionValue % 180f)) return;
		ActionValue += speed;
	}
	
	private void ChargeHammerSpin()
	{
		if (Action != Actions.HammerSpin || IsGrounded) return;
		
		// Charge
		if (Input.Down.Abc)
		{
			ActionValue += Scene.Local.ProcessSpeed;
			if (ActionValue >= MaxDropDashCharge)
			{
				AudioPlayer.Sound.Play(SoundStorage.Charge3);
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
		GroundSpeed.Value = 6f * (float)Facing;
		
		if (SuperTimer > 0f && IsCameraTarget(out ICamera camera))
		{
			camera.SetShakeTimer(6f);
		}
		
		AudioPlayer.Sound.Stop(SoundStorage.Charge3);
		AudioPlayer.Sound.Play(SoundStorage.Release);
	}
	
	private void ProcessHammerDash()
	{
		if (Action != Actions.HammerDash) return;

		// Note that ACTION_HAMMERDASH is used for movement logic only so the respective
		// animation isn't cleared alongside the action flag. All checks for Hammer Dash should refer to its animation
		
		if (!Input.Down.Abc)
		{
			Action = Actions.None;
			return;
		}
		
		ActionValue += Scene.Local.ProcessSpeed;
		if (ActionValue >= 60f)
		{
			Action = Actions.None;
			return;
		}
		
		// Air movement isn't overwritten completely, refer to ProcessMovementAir()
		if (!IsGrounded) return;
		
		if (GroundSpeed == 0f || SetPushAnimationBy != null || MathF.Cos(Mathf.DegToRad(Angle)) <= 0f)
		{
			Action = Actions.None;
		}

		// Turn around
		if (Input.Press.Left && GroundSpeed > 0f || Input.Press.Right && GroundSpeed < 0f)
		{
			Facing = (Constants.Direction)(-(int)Facing);
			GroundSpeed.Value *= -1f;
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
		if (Action != Actions.Glide) return;
		
		var climbY = (int)Position.Y;
		var collisionFlagWall = false;
		int wallRadius = RadiusNormal.X + 1;
		Angles.Quadrant moveQuadrant = Angles.GetQuadrant(Angles.GetVector256(Velocity));

		TileCollider.SetData((Vector2I)Position, TileLayer);
		
		if (moveQuadrant != Angles.Quadrant.Right)
		{
			collisionFlagWall |= CollideWallsOnGlide(wallRadius, Constants.Direction.Negative);
		}
		
		if (moveQuadrant != Angles.Quadrant.Left)
		{
			collisionFlagWall |= CollideWallsOnGlide(wallRadius, Constants.Direction.Positive);
		}
		
		collisionFlagWall |= CollideWithCeilingOnGlide(wallRadius, moveQuadrant);
		
		if (moveQuadrant != Angles.Quadrant.Up && CollideWithFloorOnGlide())
		{
			LandWhenGlide();
		}
		else if (collisionFlagWall)
		{
			AttachToWallWhenGlide(wallRadius, climbY);
		}
	}

	private bool CollideWallsOnGlide(int wallRadius, Constants.Direction direction)
	{
		var sing = (int)direction;
		int wallDistance = TileCollider.FindDistance(sing * wallRadius, 0, false, direction);

		if (wallDistance >= 0) return false;
		
		Position += new Vector2(sing * wallDistance, 0f);
		TileCollider.Position = (Vector2I)Position;
		Velocity.X = 0f;
		return true;
	}

	private bool CollideWithCeilingOnGlide(int wallRadius, Angles.Quadrant moveQuadrant)
	{
		if (moveQuadrant == Angles.Quadrant.Down) return false;
		
		int roofDistance = TileCollider.FindClosestDistance(
			-Radius.X, -Radius.Y, Radius.X, -Radius.Y, 
			true, Constants.Direction.Negative);
			
		if (moveQuadrant == Angles.Quadrant.Left && roofDistance <= -14 && SharedData.PlayerPhysics >= PhysicsTypes.S3)
		{
			// Perform right wall collision instead if moving mostly left and too far into the ceiling
			return CollideWallsOnGlide(wallRadius, Constants.Direction.Positive);
		}

		if (roofDistance >= 0) return false;
		
		Position -= new Vector2(0f, roofDistance);
		TileCollider.Position = (Vector2I)Position;
		if (Velocity.Y < 0f || moveQuadrant == Angles.Quadrant.Up)
		{
			Velocity.Y = 0f;
		}
		return false;
	}

	private bool CollideWithFloorOnGlide()
	{
		(int floorDistance, float floorAngle) = TileCollider.FindClosestTile(
			-Radius.X, Radius.Y, Radius.X, Radius.Y,
			true, Constants.Direction.Positive);
	
		if ((GlideStates)ActionState == GlideStates.Ground)
		{
			if (floorDistance > 14)
			{
				ReleaseGlide();
				return false;
			}
			
			Position += new Vector2(0f, floorDistance);
			Angle = floorAngle;
			return false;
		}

		if (floorDistance >= 0) return false;
		
		Position += new Vector2(0f, floorDistance);
		TileCollider.Position = (Vector2I)Position;
		Angle = floorAngle;
		Velocity.Y = 0f;
		return true;
	}

	private void LandWhenGlide()
	{
		switch ((GlideStates)ActionState)
		{
			case GlideStates.Air: LandWhenGlideAir(); break;
			case GlideStates.Fall: LandWhenGlideAir(); break;
		}
	}

	private void LandWhenGlideAir()
	{
		if (Angles.GetQuadrant(Angle) != Angles.Quadrant.Down)
		{
			GroundSpeed.Value = Angle < 180 ? Velocity.X : -Velocity.X;
			Land();
			return;
		}
				
		Animation = Animations.GlideGround;
		ActionState = (int)GlideStates.Ground;
		ActionValue = 0f;
		Gravity = 0f;
	}

	private void LandWhenGlideFall()
	{
		AudioPlayer.Sound.Play(SoundStorage.Land);
		Land();		
		
		if (Angles.GetQuadrant(Angle) != Angles.Quadrant.Down)
		{
			GroundSpeed.Value = Velocity.X;
			return;
		}
					
		Animation = Animations.GlideLand;
		GroundLockTimer = 16f;
		GroundSpeed.Value = 0f;
		Velocity.X = 0f;
	}

	private void AttachToWallWhenGlide(int wallRadius, int climbY)
	{
		if (ActionState != (int)GlideStates.Air) return;

		CheckCollisionOnAttaching(wallRadius, climbY);
			
		if (Facing == Constants.Direction.Negative)
		{
			Position += Vector2.Right;
		}
			
		Animation = Animations.ClimbWall;
		Action = Actions.Climb;
		ActionState = (int)ClimbStates.Normal;
		ActionValue = 0f;
		GroundSpeed.Value = 0f;
		Velocity.Y = 0f;
		Gravity	= 0f;
			
		AudioPlayer.Sound.Play(SoundStorage.Grab);
	}

	private void CheckCollisionOnAttaching(int wallRadius, int climbY)
	{
		// Cast a horizontal sensor just above Knuckles. If the distance returned is not 0, he
		// is either inside the ceiling or above the floor edge
		TileCollider.Position = TileCollider.Position with { Y = climbY - Radius.Y };
		if (TileCollider.FindDistance(wallRadius * (int)Facing, 0, false, Facing) == 0) return;
		
		// The game casts a vertical sensor now in front of Knuckles, facing downwards. If the distance
		// returned is negative, Knuckles is inside the ceiling, else he is above the edge
			
		// Note that tile behaviour here is set to Constants.TileBehaviours.Ceiling.
		// LBR tiles are not ignored in this case
		TileCollider.TileBehaviour = Constants.TileBehaviours.Ceiling;
		int floorDistance = TileCollider.FindDistance(
			(wallRadius + 1) * (int)Facing, -1, true, Constants.Direction.Positive);
				
		if (floorDistance is < 0 or >= 12)
		{
			ReleaseGlide();
			return;
		}
		
		// Adjust Knuckles' y-position to place him just below the edge
		Position += new Vector2(0f, floorDistance);
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
			CarryTimer -= Scene.Local.ProcessSpeed;
			if (CarryTimer > 0f) return;
		}
	
		if (CarryTarget != null)
		{
			CarryTarget.OnAttached(this);
			return;
		}
		
		if (Action != Actions.Flight) return;

		GrabAnotherPlayer();
	}

	private void GrabAnotherPlayer()
	{
		foreach (Player player in Scene.Local.Players.Values)
		{
			if (player == this) continue;
			if (player.Action is Actions.SpinDash or Actions.Carried) continue;
			if (!player.IsControlRoutineEnabled || !player.IsObjectInteractionEnabled) continue;

			Vector2 delta = (player.Position - Position).Abs();
			if (delta.X >= 16f || delta.Y >= 48f) continue;
				
			player.ResetState();
			AudioPlayer.Sound.Play(SoundStorage.Grab);
				
			player.Animation = Animations.Grab;
			player.Action = Actions.Carried;
			CarryTarget = player;

			player.AttachToPlayer(this);
		}
	}
}
