using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Objects.Player;

public abstract partial class BasicPhysicalPlayer : PlayerData
{
	protected event Action LandHandler;
	protected PhysicParams PhysicParams;

	protected void UpdatePhysicParameters()
	{
		PhysicParams = PhysicParams.Get(IsUnderwater, SuperTimer > 0f, Type, ItemSpeedTimer);
	}

	protected void ProcessCorePhysics()
	{
		ProcessSlopeResist();
		ProcessSlopeResistRoll();
		ProcessMovementGround();
		ProcessMovementRoll();
		ProcessMovementAir();
		ProcessBalance();
		ProcessCollisionGroundWalls();
		ProcessRollStart();
		ProcessLevelBound();
		ProcessPosition();
		ProcessCollisionGroundFloor();
		ProcessSlopeRepel();
		ProcessCollisionAir();
	}
	
	public void Kill()
	{
		if (IsDead) return;
		
		ResetState();
		AudioPlayer.Sound.Play(SoundStorage.Hurt);

		if (Id == 0)
		{
			Scene.Local.AllowPause = false;
			Scene.Local.State = Scene.States.StopObjects;
			
			SharedData.PlayerShield = ShieldContainer.Types.None;
		}
		
		ZIndex = (int)Constants.ZIndexes.AboveForeground;
		Action = Actions.None;
		Animation = Animations.Death;
		IsDead = true;
		IsObjectInteractionEnabled = false;
		Gravity = GravityType.Default;
		Velocity.Vector = new Vector2(0f, -7f);
		GroundSpeed.Value = 0f;

		if (IsCameraTarget(out ICamera camera))
		{
			camera.IsMovementAllowed = false;
		}
	}
	
	public void Hurt(float positionX = 0f)
	{
		if (IsInvincible || IsDebugMode) return;

		if (Id == 0 && SharedData.PlayerRings == 0 && SharedData.PlayerShield == ShieldContainer.Types.None)
		{
			Kill();
			return;
		}
		
		ResetState();

		const float velocityX = 2f, velocityY = 4f;
		Velocity.Vector = new Vector2(Position.X - positionX < 0f ? -velocityX : velocityX, velocityY);
		Gravity = GravityType.HurtFall;
		Animation = Animations.Hurt;
		IsHurt = true;
		IsAirLock = true;
		InvincibilityTimer = 120f;

		if (IsUnderwater)
		{
			Velocity.Vector *= 0.5f;
			Gravity -= 0.15625f;
		}
		
		if (Id > 0 || SharedData.PlayerShield > ShieldContainer.Types.None)
		{
			if (Id == 0)
			{
				SharedData.PlayerShield = ShieldContainer.Types.None;
			}
			
			AudioPlayer.Sound.Play(SoundStorage.Hurt);
			return;
		}
		
		DropRings();
	}
	
	public void Land()
	{
		ResetGravity();
		
		IsGrounded = true;
	
		switch (Action)
		{
			case Actions.Flight:
				AudioPlayer.Sound.Stop(SoundStorage.Flight);
				AudioPlayer.Sound.Stop(SoundStorage.Flight2);
				break;
			
			case Actions.SpinDash or Actions.Dash:
				if (Action == Actions.Dash)
				{
					GroundSpeed.Value = ActionValue2;
				}
				return;
		}
	
		if (WaterBarrierBounce()) return;
		SetLandAnimation();
	
		if (IsHurt)
		{
			GroundSpeed.Value = 0f;
		}
	
		IsAirLock = false;
		IsSpinning	= false;
		IsJumping = false;
		SetPushAnimationBy = null;
		IsHurt = false;
	
		Shield.State = ShieldContainer.States.None;
		ComboCounter = 0;
		TileBehaviour = Constants.TileBehaviours.Floor;
	
		CpuState = CpuStates.Main;

		LandHandler?.Invoke();
	
		if (Action != Actions.HammerDash)
		{
			Action = Actions.None;
		}
		else
		{
			GroundSpeed.Value = 6 * (int)Facing;
		}

		if (IsSpinning) return;
		Position += new Vector2(0f, Radius.Y - RadiusNormal.Y);
		
		Radius = RadiusNormal;
	}

	protected void SetCameraDelayX(float delay)
	{
		if (!SharedData.CdCamera && IsCameraTarget(out ICamera camera))
		{
			camera.SetCameraDelayX(delay);
		}
	}

	private void DropRings()
	{
		int ringFlip = -1;
		var ringAngle = 101.25f;
		var ringSpeed = 4;
		uint count = Math.Min(SharedData.PlayerRings, 32);
				
		for (var i = 0; i < count; i++) 
		{
			//TODO: spawn ring
			/*
			instance_create(_player.x, _player.y, obj_ring,
			{
				State: RING_STATE_DROPPED,			
				VelocityX: ringSpeed * dcos(ringAngle) * -ringFlip,
				VelocityY: ringSpeed * -dsin(ringAngle)
			});
			*/
							
			if (ringFlip == 1)
			{
				ringAngle += 22.5f;
			}
							
			ringFlip = -ringFlip;

			if (i != 15) continue;
			
			ringSpeed = 2;
			ringAngle = 101.25f;
		}

		Scene.Local.RingSpillTimer = 256f;
	
		SharedData.PlayerRings = 0;
		SharedData.LifeRewards = SharedData.LifeRewards with { X = 100 };
			
		AudioPlayer.Sound.Play(SoundStorage.RingLoss);
	}

	private bool WaterBarrierBounce()
	{
		if (Shield.State != ShieldContainer.States.Active || 
		    SharedData.PlayerShield != ShieldContainer.Types.Bubble) return false;
		
		float force = IsUnderwater ? -4f : -7.5f;
		float radians = Mathf.DegToRad(Angle);
		Velocity.Vector = new Vector2(MathF.Sin(radians), MathF.Cos(radians)) * force;

		Shield.State = ShieldContainer.States.None;
		OnObject = null;
		IsGrounded = false;

		//TODO: replace animation
		Shield.AnimationType = ShieldContainer.AnimationTypes.BubbleBounce;
			
		AudioPlayer.Sound.Play(SoundStorage.ShieldBubble2);
		
		return true;
	}

	private void SetLandAnimation()
	{
		if (OnObject != null)
		{
			Animation = Animations.Move;
			return;
		}
		
		if (Animation is Animations.Idle or Animations.Duck or Animations.HammerDash or Animations.GlideGround) return;
		Animation = Animations.Move;
	}
	
    private void ProcessSlopeResist()
	{
		if (!IsGrounded || IsSpinning || Action is Actions.HammerDash or Actions.Dash) return;
		if (Angle is >= 135f and < 225f) return; // Exit if we're on ceiling
		
		float slopeGravity = 0.125f * MathF.Sin(Mathf.DegToRad(Angle));
		
		// Decrease ground speed
		if (GroundSpeed != 0f || SharedData.PlayerPhysics >= PhysicsTypes.S3 && Math.Abs(slopeGravity) > 0.05078125f)
		{
			GroundSpeed.Acceleration = -slopeGravity;
		}
	}
	
	private void ProcessSlopeResistRoll()
	{
		if (!IsGrounded || !IsSpinning) return;
		if (Angle is >= 135f and < 225f) return; // Exit if we're on ceiling
	
		float angleSine = MathF.Sin(Mathf.DegToRad(Angle));
		float slopeGravity = Math.Sign(GroundSpeed) == Math.Sign(angleSine) ? 0.078125f : 0.3125f;
		GroundSpeed.Acceleration = -slopeGravity * angleSine;
	}
	
	private void ProcessMovementGround()
	{
		if (!IsGrounded || IsSpinning) return;
		if (Action is Actions.SpinDash or Actions.Dash or Actions.HammerDash) return;
		
		// Cancel Knuckles' glide-landing animation
		if (Animation is Animations.GlideGround or Animations.GlideLand && (Input.Down.Down || GroundSpeed != 0))
		{
			GroundLockTimer = 0f;
		}
		
		if (GroundLockTimer <= 0f)
		{
			var doSkid = false;
			
			if (Input.Down.Left)
			{
				doSkid |= MoveOnGround(Constants.Direction.Negative);
			}
			
			if (Input.Down.Right)
			{
				doSkid |= MoveOnGround(Constants.Direction.Positive);
			}
			
			UpdateMovementGroundAnimation(doSkid);
		}
		
		if (Input.Down is { Left: false, Right: false })
		{
			ApplyGroundFriction(PhysicParams.Friction);
		}
		
		Velocity.SetDirectionalValue(GroundSpeed, Angle);
	}
	
	private void ApplyGroundFriction(float friction)
	{
		int sign = Math.Sign(GroundSpeed);
		GroundSpeed.Acceleration = -sign * friction;
		
		switch (sign)
		{
			case  1: GroundSpeed.Max(0f); break;
			case -1: GroundSpeed.Min(0f); break;
		}
	}
	
	private bool MoveOnGround(Constants.Direction direction)
	{
		var sign = (float)direction;
		
		if (GroundSpeed * sign < 0f)
		{
			GroundSpeed.Acceleration = sign * PhysicParams.Deceleration;
			if (direction == Constants.Direction.Positive == GroundSpeed >= 0f)
			{
				GroundSpeed.Value = 0.5f * sign;
			}
			
			return true;
		}

		if (!SharedData.NoSpeedCap || GroundSpeed * sign < PhysicParams.AccelerationTop)
		{
			float acceleration = PhysicParams.Acceleration;
			GroundSpeed.Acceleration = acceleration * (float)direction;
			
			switch (direction)
			{
				case Constants.Direction.Positive: GroundSpeed.Min( PhysicParams.AccelerationTop); break;
				case Constants.Direction.Negative: GroundSpeed.Max(-PhysicParams.AccelerationTop); break;
			}
		}
		
		// Cancel skid animation
		if (Animation == Animations.Skid)
		{
			Animation = Animations.Move;
		}
		
		// Turn around
		if (Facing == direction) return false;
		
		Animation = Animations.Move;
		Facing = direction;
		SetPushAnimationBy = null;
		OverrideAnimationFrame = 0;
		
		return false;
	}
	
	private void UpdateMovementGroundAnimation(bool doSkid)
	{
		// Set push animation once animation frame changes
		if (SetPushAnimationBy != null && IsAnimationFrameChanged)
		{
			Animation = Animations.Push;
		}
		
		// Set idle animation
		Angles.Quadrant quadrant = Angles.GetQuadrant(Angle);
		if (quadrant == Angles.Quadrant.Down && GroundSpeed == 0f)
		{
			Animation = Input.Down.Up ? Animations.LookUp : Input.Down.Down ? Animations.Duck : Animations.Idle;
			SetPushAnimationBy = null;
			return;
		}
			
		if (Animation == Animations.Skid) return;
		
		if (Animation is not (Animations.Push or Animations.Skid))
		{
			Animation = Animations.Move;
		}

		// Perform skid
		if (quadrant != Angles.Quadrant.Down || !doSkid) return;
		if (Math.Abs(GroundSpeed) < PlayerConstants.SkidSpeedThreshold) return;
		
		ActionValue2 = 0f; // We'll use this as a timer to spawn dust particles in UpdateStatus()
		Animation = Animations.Skid;
		
		AudioPlayer.Sound.Play(SoundStorage.Skid);
	}
	
	private void ProcessMovementRoll()
	{
		if (!IsGrounded || !IsSpinning) return;
		
		if (GroundLockTimer <= 0f)
		{
			if (Input.Down.Left)
			{
				RollOnGround(Constants.Direction.Negative);
			}
			
			if (Input.Down.Right)
			{
				RollOnGround(Constants.Direction.Positive);
			}
		}

		ApplyGroundFriction(PhysicParams.FrictionRoll);
		UpdateSpinningOnGround();
	
		Velocity.SetDirectionalValue(GroundSpeed, Angle);
		Velocity.ClampX(-16f, 16f);
	}
	
	private void RollOnGround(Constants.Direction direction)
	{
		var sign = (float)direction;
		float absoluteSpeed = sign * GroundSpeed;
		if (absoluteSpeed < 0f)
		{
			GroundSpeed.Acceleration = sign * PhysicParams.DecelerationRoll;
			if (direction == Constants.Direction.Positive == GroundSpeed < 0f) return;
			GroundSpeed.Value = sign * 0.5f;
			return;
		}
		
		Facing = direction;
		SetPushAnimationBy = null;
	}
	
	private void UpdateSpinningOnGround()
	{
		if (StopSpinning()) return;
		ForceSpin();
	}
	
	private bool StopSpinning()
	{
		if (IsForcedSpin) return false;
		
		if (GroundSpeed != 0f)
		{
			if (SharedData.PlayerPhysics != PhysicsTypes.SK || Math.Abs(GroundSpeed) >= 0.5f) return true;
		}
		
		Position += new Vector2(0f, Radius.Y - RadiusNormal.Y);
		
		IsSpinning = false;
		Radius = RadiusNormal;
		Animation = Animations.Idle;
		
		return true;
	}
	
	private void ForceSpin()
	{
		if (SharedData.PlayerPhysics == PhysicsTypes.CD)
		{
			if (GroundSpeed.Value is >= 0f and < 2f)
			{
				GroundSpeed.Value = 2f;
			}
			return;
		}
		
		if (GroundSpeed != 0f) return;
		GroundSpeed.Value = SharedData.PlayerPhysics == PhysicsTypes.S1 ? 2f : 4f * (float)Facing;
	}
	
	private void ProcessMovementAir()
	{
		if (IsGrounded || IsDead) return;
		if (Action is Actions.Carried or Actions.Climb or Actions.Glide && ActionState != (int)GlideStates.Fall) return;

		RotateInAir();
		LimitVerticalVelocity();
		if (ChangeHammerDashFacingInAir()) return;
		MoveInAirHorizontally();
		ApplyAirDrag();
	}

	private void RotateInAir()
	{
		if (Mathf.IsEqualApprox(Angle, 0f)) return;
		
		float speed = Angles.ByteAngleStep * Scene.Local.ProcessSpeed;
		Angle += Angle >= 180f ? speed : -speed;
		
		if (Angle is < 0f or >= 360f)
		{
			Angle = 0f;
		}
	}

	private void LimitVerticalVelocity()
	{
		if (!IsJumping && Action != Actions.SpinDash && !IsForcedSpin && Velocity.Y < -15.75f)
		{
			Velocity.Y = -15.75f;
		}
		else if (SharedData.PlayerPhysics == PhysicsTypes.CD && Velocity.Y > 16f)
		{
			Velocity.Y = 16f;
		}
	}

	private bool ChangeHammerDashFacingInAir()
	{
		if (Action != Actions.HammerDash) return false;
		
		if (Input.Down.Left)
		{
			Facing = Constants.Direction.Negative;
		}
		else if (Input.Down.Right)
		{
			Facing = Constants.Direction.Positive;
		}
		
		return true;
	}

	private void MoveInAirHorizontally()
	{
		if (IsAirLock) return;
		
		if (Input.Down.Left)
		{
			if (Velocity.X > 0f)
			{
				Velocity.AccelerationX = -PhysicParams.AccelerationAir;
			}
			else if (!SharedData.NoSpeedCap || -Velocity.X < PhysicParams.AccelerationTop)
			{
				Velocity.AccelerationX = -PhysicParams.AccelerationAir;
				Velocity.MaxX(-PhysicParams.AccelerationTop);
			}
			
			Facing = Constants.Direction.Negative;
		}
		else if (Input.Down.Right)
		{
			if (Velocity.X < 0f)
			{
				Velocity.AccelerationX = PhysicParams.AccelerationAir;
			}
			else if (!SharedData.NoSpeedCap || Velocity.X < PhysicParams.AccelerationTop)
			{
				Velocity.AccelerationX = PhysicParams.AccelerationAir;
				Velocity.MinX(PhysicParams.AccelerationTop);
			}
			
			Facing = Constants.Direction.Positive;
		}
	}

	private void ApplyAirDrag()
	{
		if (!IsHurt && Velocity.Y is < 0f and > -4f)
		{
			Velocity.AccelerationX = MathF.Floor(Velocity.X * 8f) / -256f;
		}
	}
	
	private void ProcessBalance()
	{
		if (!IsGrounded || IsSpinning) return;
		if (GroundSpeed != 0 || Action is Actions.SpinDash or Actions.Dash) return;
		
		// Don't allow player to duck or look up
		if (SharedData.PlayerPhysics == PhysicsTypes.SK && (Input.Down.Down || Input.Down.Up)) return;
		
		if (BalanceOnTiles()) return;
		BalanceOnObject();
	}
	
	private void BalanceOnObject()
	{
		if (OnObject == null) return;
		// TODO: check IsInstanceValid == instance_exist
		if (!IsInstanceValid(OnObject) || OnObject.SolidData.NoBalance) return;
		
		const int leftEdge = 2;
		const int panicOffset = 4;
		
		int rightEdge = OnObject.SolidData.Radius.X * 2 - leftEdge;
		int playerX = Mathf.FloorToInt(OnObject.SolidData.Radius.X - OnObject.Position.X + Position.X);
		
		if (playerX < leftEdge)
		{
			BalanceToDirection(Constants.Direction.Negative, playerX < leftEdge - panicOffset);
		}
		else if (playerX > rightEdge)
		{
			BalanceToDirection(Constants.Direction.Positive, playerX > rightEdge + panicOffset);
		}
	}
	
	private bool BalanceOnTiles()
	{
		if (OnObject != null) return false;
		
		const Constants.Direction direction = Constants.Direction.Positive;	
		
		if (Angles.GetQuadrant(Angle) > Angles.Quadrant.Down) return true;
		TileCollider.SetData((Vector2I)Position + new Vector2I(0, Radius.Y), TileLayer);
		
		if (TileCollider.FindDistance(0, 0, true, direction) < 12) return true;
		
		(_, float angleLeft) = TileCollider.FindTile(-Radius.X, 0, true, direction);
		(_, float angleRight) = TileCollider.FindTile(Radius.X, 0, true, direction);
		
		if (float.IsNaN(angleLeft) == float.IsNaN(angleRight)) return true;
		
		int sign = float.IsNaN(angleLeft) ? -1 : 1;
		bool isPanic = TileCollider.FindDistance(-6 * sign, 0, true, direction) >= 12;
		BalanceToDirection((Constants.Direction)sign, isPanic);
		return true;
	}

	private void BalanceToDirection(Constants.Direction direction, bool isPanic)
	{
		switch (Type)
		{
			case Types.Amy or Types.Tails:
			case Types.Sonic when SuperTimer > 0f:
				Animation = Animations.Balance;
				Facing = direction;
				break;
			
			case Types.Knuckles:
				if (Facing == direction)
				{
					Animation = Animations.Balance;
				}
				else if (Animation != Animations.BalanceFlip)
				{
					Animation = Animations.BalanceFlip;
					Facing = direction;
				}
				break;
			
			case Types.Sonic:
				if (!isPanic)
				{
					Animation = Facing == direction ? Animations.Balance : Animations.BalanceFlip;
				}
				else if (Facing != direction)
				{
					Animation = Animations.BalanceTurn;
					Facing = direction;
				}
				else if (Animation != Animations.BalanceTurn)
				{
					Animation = Animations.BalancePanic;
				}
				break;
		}
	}
	
	private void ProcessCollisionGroundWalls()
	{
		if (!IsGrounded) return;
		
		// Exit collision while on a left wall or a ceiling, unless angle is cardinal
		// and S3K physics are enabled
		if (Angle is > 90f and <= 270f && (SharedData.PlayerPhysics < PhysicsTypes.SK || Angle % 90f != 0f)) return;

		int wallRadius = RadiusNormal.X + 1;
		int offsetY = Angle == 0f ? 8 : 0;
		
		int sign;
		Constants.Direction firstDirection, secondDirection;
		switch ((float)GroundSpeed)
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
			
			default: return;
		}
		
		TileCollider.SetData((Vector2I)Velocity.CalculateNewPosition(Position), TileLayer, TileBehaviour);
		
		int castQuadrant = Angle switch
		{
			>= 45f and <= 128f => 1,
			> 128f and < 225f => 2,
			>= 225f and < 315f => 3,
			_ => 0
		};
		
		int wallDistance = castQuadrant switch
		{
			0 => TileCollider.FindDistance(-wallRadius, offsetY, false, firstDirection),
			1 => TileCollider.FindDistance(0, wallRadius, true, secondDirection),
			2 => TileCollider.FindDistance(wallRadius, 0, false, secondDirection),
			3 => TileCollider.FindDistance(0, -wallRadius, true, firstDirection),
			_ => throw new ArgumentOutOfRangeException()
		};
		
		if (wallDistance >= 0) return;
		Angles.Quadrant quadrant = Angles.GetQuadrant(Angle);
		wallDistance *= quadrant > Angles.Quadrant.Right ? -sign : sign;
		
		//TODO: check "/"
		switch (quadrant)
		{
			case Angles.Quadrant.Down or Angles.Quadrant.Up:
				Velocity.X -= wallDistance / Scene.Local.ProcessSpeed;
				GroundSpeed.Value = 0f;
					
				if (Facing == firstDirection && !IsSpinning)
				{
					SetPushAnimationBy = this;
				}
				break;
				
			case Angles.Quadrant.Right or Angles.Quadrant.Left:
				Velocity.Y += wallDistance / Scene.Local.ProcessSpeed;
				break;
		}
	}
	
	private void ProcessRollStart()
	{
		if (!IsGrounded || IsSpinning || Action is Actions.SpinDash or Actions.HammerDash) return;
		if (!IsForcedSpin && (Input.Down.Left || Input.Down.Right)) return;

		if (!CheckSpinPossibility() && !IsForcedSpin) return;
		
		Position += new Vector2(0f, Radius.Y - RadiusSpin.Y);
		Radius = RadiusSpin;
		IsSpinning = true;
		Animation = Animations.Spin;
		
		AudioPlayer.Sound.Play(SoundStorage.Roll);
	}

	private bool CheckSpinPossibility()
	{
		if (!Input.Down.Down) return false;
		
		if (SharedData.PlayerPhysics != PhysicsTypes.SK)
		{
			return Math.Abs(GroundSpeed) >= 0.5f;
		}

		if (Math.Abs(GroundSpeed) >= 1f) return true;

		Animation = Animations.Duck;
		return false;
	}
	
	private void ProcessLevelBound()
	{
		if (IsDead) return;
		
		if (!IsCameraTarget(out ICamera camera) && !Scene.Local.Players.First().IsCameraTarget(out camera)) return;
		
		// Left bound
		if (Position.X + Velocity.X < camera.Boundary.X + 16f)
		{
			GroundSpeed.Value = 0f;
			Velocity.X = 0f;
			Position = new Vector2(camera.Boundary.X + 16f, Position.Y);
		}
		
		float rightBound = camera.Boundary.Z - 24f;
		
		// Allow player to walk past the right bound if they crossed Sign Post
		//TODO: replace instance_exists
		/*if (instance_exists(obj_signpost))
		{
			// TODO: There should be a better way?
			rightBound += 64;
		}*/
		
		// Right bound
		if (Position.X + Velocity.X > rightBound)
		{
			GroundSpeed.Value = 0f;
			Velocity.X = 0f;
			Position = new Vector2(rightBound, Position.Y);
		}
		
		// Top bound
		switch (Action)
		{
			case Actions.Flight or Actions.Climb:
				if (Position.Y + Velocity.Y >= camera.Boundary.Y + 16f) break;
	
				if (Action == Actions.Flight)
				{
					Gravity	= GravityType.TailsDown;
				}

				Velocity.Y = 0f;
				Position = new Vector2(Position.X, camera.Boundary.Y + 16f);
				break;
			
			case Actions.Glide when Position.Y < camera.Boundary.Y + 10f:
				GroundSpeed.Value = 0f;
				break;
		}
	
		// Bottom bound
		if (AirTimer > 0f && Position.Y > Math.Max(camera.Boundary.W, camera.TargetBoundary.W))
		{
			Kill();
		}
	}
	
	private void ProcessPosition()
	{
		if (Action == Actions.Carried) return;
		
		if (IsStickToConvex)
		{
			Velocity.Clamp(-16f * Vector2.One, 16f * Vector2.One);
		}
		
		Position = Velocity.CalculateNewPosition(Position);
		Velocity.Vector = Velocity.Vector;
		
		if (IsGrounded) return;
		Velocity.AccelerationY = Gravity;
	}
	
	private void ProcessCollisionGroundFloor()
	{
		if (!IsGrounded || OnObject != null) return;

		// Each tile type has its own rules about how it should react to a specific tile check
		// Since we're going to rotate player's sensors, "rotate" tile properties as well
		
		TileBehaviour = Angle switch
		{
			<= 45 or >= 315 => Constants.TileBehaviours.Floor,
			> 45 and < 135 => Constants.TileBehaviours.RightWall,
			>= 135 and <= 225 => Constants.TileBehaviours.Ceiling,
			_ => Constants.TileBehaviours.LeftWall
		};
		
		TileCollider.SetData((Vector2I)Position, TileLayer, TileBehaviour);

		(int distance, float angle) = TileBehaviour switch
		{
			Constants.TileBehaviours.Floor => TileCollider.FindClosestTile(
				-Radius.X, Radius.Y, Radius.X, Radius.Y, 
				true, Constants.Direction.Positive),
			
			Constants.TileBehaviours.RightWall => TileCollider.FindClosestTile(
				Radius.Y, Radius.X, Radius.Y, -Radius.X, 
				false, Constants.Direction.Positive),
			
			Constants.TileBehaviours.Ceiling => TileCollider.FindClosestTile(
				Radius.X, -Radius.Y, -Radius.X, -Radius.Y, 
				true, Constants.Direction.Negative),
			
			Constants.TileBehaviours.LeftWall => TileCollider.FindClosestTile(
				-Radius.Y, -Radius.X, -Radius.Y, Radius.X, 
				false, Constants.Direction.Negative),
			
			_ => throw new ArgumentOutOfRangeException()
		};
		
		const int minTolerance = 4;
		const int maxTolerance = 14;
		
		if (!IsStickToConvex)
		{
			float toleranceCheckSpeed = TileBehaviour switch
			{
				Constants.TileBehaviours.Floor or Constants.TileBehaviours.Ceiling => Velocity.X,
				Constants.TileBehaviours.RightWall or Constants.TileBehaviours.LeftWall => Velocity.Y,
				_ => throw new ArgumentOutOfRangeException()
			};
			
			float tolerance = SharedData.PlayerPhysics < PhysicsTypes.S2 ? 
				maxTolerance : Math.Min(minTolerance + Math.Abs(MathF.Floor(toleranceCheckSpeed)), maxTolerance);
			
			if (distance > tolerance)
			{
				SetPushAnimationBy = null;
				IsGrounded = false;
						
				OverrideAnimationFrame = 0;
				return;
			}
		}

		if (distance < -maxTolerance) return;
		
		Position += TileBehaviour switch
		{
			Constants.TileBehaviours.Floor => new Vector2(0f, distance),
			Constants.TileBehaviours.RightWall => new Vector2(distance, 0f),
			Constants.TileBehaviours.Ceiling => new Vector2(0f, -distance),
			Constants.TileBehaviours.LeftWall => new Vector2(-distance, 0f),
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
		if (!IsGrounded || IsStickToConvex || Action == Actions.HammerDash) return;
	
		if (GroundLockTimer > 0f)
		{
			GroundLockTimer -= Scene.Local.ProcessSpeed;
			return;
		}

		if (Math.Abs(GroundSpeed) >= 2.5f) return;

		// Original slope physics
		if (SharedData.PlayerPhysics < PhysicsTypes.S3)
		{
			if (Angles.GetQuadrant(Angle) == Angles.Quadrant.Down) return;
				
			GroundSpeed.Value = 0f;	
			GroundLockTimer = 30f;
			IsGrounded = false;
			return;
		}

		// Sonic 3 and onwards slope physics
		switch (Angle)
		{
			case <= 33.75f or > 326.25f: return;
			
			case > 67.5f and <= 292.5f:
				IsGrounded = false;
				break;
			
			default:
				GroundSpeed.Acceleration = Angle < 180f ? -0.5f : 0.5f;
				break;
		}

		GroundLockTimer = 30f;
	}

	private void ProcessCollisionAir()
	{
		if (IsGrounded || IsDead || Action is Actions.Glide or Actions.Climb) return;
		
		int wallRadius = RadiusNormal.X + 1;
		Angles.Quadrant moveQuadrant = Angles.GetQuadrant(Angles.GetVector256(Velocity));
		
		TileCollider.SetData((Vector2I)Position, TileLayer);

		var moveQuadrantValue = (int)moveQuadrant;

		if (CollideWallsInAir(wallRadius, moveQuadrantValue, Constants.Direction.Negative)) return;
		if (CollideWallsInAir(wallRadius, moveQuadrantValue, Constants.Direction.Positive)) return;

		if (CollideWithCeilingInAir(wallRadius, moveQuadrant)) return;

		CollideWithFloorInAir(moveQuadrant);
	}

	private bool CollideWallsInAir(int wallRadius, int moveQuadrantValue, Constants.Direction direction)
	{
		var sign = (int)direction;
		
		if (moveQuadrantValue == (int)Angles.Quadrant.Up + sign) return false;
		
		int wallDistance = TileCollider.FindDistance(sign * wallRadius, 0, false, direction);
		
		if (wallDistance >= 0f) return false;
		Position += new Vector2(sign * wallDistance, 0f);
		TileCollider.Position = (Vector2I)Position;
		Velocity.X = 0f;
		
		if (moveQuadrantValue != (int)Angles.Quadrant.Up - sign) return false;
		GroundSpeed.Value = Velocity.Y;
		return true;
	}

	private bool CollideWithCeilingInAir(int wallRadius, Angles.Quadrant moveQuadrant)
	{
		if (moveQuadrant == Angles.Quadrant.Down) return false;
		
		(int roofDistance, float roofAngle) = TileCollider.FindClosestTile(
			-Radius.X, -Radius.Y, Radius.X, -Radius.Y,
			true, Constants.Direction.Negative);
			
		if (moveQuadrant == Angles.Quadrant.Up && SharedData.PlayerPhysics >= PhysicsTypes.S3 && roofDistance <= -14f)
		{
			// Perform right wall collision if moving mostly left and too far into the ceiling
			int wallDist = TileCollider.FindDistance(wallRadius, 0, false, Constants.Direction.Positive);

			if (wallDist >= 0) return false;
			
			Position += new Vector2(wallDist, 0f);
			Velocity.X = 0f;
			return true;
		}
		
		if (roofDistance >= 0) return false;
		
		Position -= new Vector2(0f, roofDistance);
		if (moveQuadrant == Angles.Quadrant.Up && Action != Actions.Flight && 
		    Angles.GetQuadrant(roofAngle) is Angles.Quadrant.Right or Angles.Quadrant.Left)
		{
			Angle = roofAngle;
			GroundSpeed.Value = roofAngle < 180f ? -Velocity.Y : Velocity.Y;
			Velocity.Y = 0f;
					
			Land();
			return true;
		}
		
		if (Velocity.Y < 0f)
		{
			Velocity.Y = 0f;
		}
		
		if (Action == Actions.Flight)
		{
			Gravity	= GravityType.TailsDown;
		}
		
		return true;
	}
	
	private void CollideWithFloorInAir(Angles.Quadrant moveQuadrant)
	{
		if (moveQuadrant == Angles.Quadrant.Up) return;
		
		int distance;
		float angle;

		if (moveQuadrant == Angles.Quadrant.Down)
		{
			if (LandOnFeet(out distance, out angle)) return;
		}
		else if (Velocity.Y >= 0) // If moving mostly left or right, continue if our vertical velocity is positive
		{
			if (FallOnGround(out distance, out angle)) return;
		}
		else return;
		
		Position += new Vector2(0f, distance);
		Angle = angle;
		
		Land();
	}

	private bool LandOnFeet(out int distance, out float angle)
	{
		(int distanceL, float angleL) = TileCollider.FindTile(
			-Radius.X, Radius.Y, true, Constants.Direction.Positive);
			
		(int distanceR, float angleR) = TileCollider.FindTile(
			Radius.X, Radius.Y, true, Constants.Direction.Positive);

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
		
		// Exit if too far into the ground when BOTH sensors find it.
		// So if we're landing on a ledge, it doesn't matter how far we're clipping into the ground
		
		float minClip = -(Velocity.Y + 8f);		
		if (distance >= 0 || minClip >= distanceL && minClip >= distanceR) return true;
		
		if (Angles.GetQuadrant(angle) != Angles.Quadrant.Down)
		{
			if (Velocity.Y > 15.75f)
			{
				Velocity.Y = 15.75f;
			}
			
			GroundSpeed.Value = angle < 180f ? -Velocity.Y : Velocity.Y;
			Velocity.X = 0f;
		}
		else if (angle is > 22.5f and <= 337.5f)
		{
			GroundSpeed.Value = angle < 180f ? -Velocity.Y : Velocity.Y;
			GroundSpeed.Value *= 0.5f;
		}
		else 
		{
			GroundSpeed.Value = Velocity.X;
			Velocity.Y = 0f;
		}
		
		return false;
	}

	private bool FallOnGround(out int distance, out float angle)
	{
		(distance, angle) = TileCollider.FindClosestTile(
			-Radius.X, Radius.Y, Radius.X, Radius.Y,
			true, Constants.Direction.Positive);
		
		if (distance >= 0) return true;
		
		GroundSpeed.Value = Velocity.X;
		Velocity.Y = 0f;
		
		return false;
	}
}
