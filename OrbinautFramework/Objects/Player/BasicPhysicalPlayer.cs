using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
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
	}
	
	public void Kill()
	{
		if (IsDead) return;
		
		ResetState();
		AudioPlayer.Sound.Play(SoundStorage.Hurt);

		if (Id == 0)
		{
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
	
	public void LandOnSolid(BaseObject targetObject, Constants.SolidType type, int distance)
	{
		if (type is Constants.SolidType.AllReset or Constants.SolidType.TopReset)
		{
			ResetState();
		}

		Position = Position with { Y = Position.Y - distance + 1 };
		GroundSpeed.Value = Velocity.X;
		Velocity.X = 0f;
		Angle = 360f;
		
		OnObject = targetObject;

		if (IsGrounded) return;
		IsGrounded = true;

		Land();
	}
	
	public void ClearPush()
	{
		if (SetPushAnimationBy != this) return;
		if (Animation is not (Animations.Spin or Animations.SpinDash))
		{
			Animation = Animations.Move;
		}
		
		SetPushAnimationBy = null;
	}

	protected void SetCameraDelayX(float delay)
	{
		if (!SharedData.CdCamera && IsCameraTarget(out ICamera camera))
		{
			camera.Delay = camera.Delay with { X = delay };
		}
	}

	private bool WaterBarrierBounce()
	{
		if (Shield.State != ShieldContainer.States.Active || Shield.Type != ShieldContainer.Types.Bubble) return false;
		
		float force = IsUnderwater ? -4f : -7.5f;
		float radians = Mathf.DegToRad(Angle);
		Velocity.Vector = new Vector2(MathF.Sin(radians), MathF.Sin(radians)) * force;

		Shield.State = ShieldContainer.States.None;
		OnObject = null;
		IsGrounded = false;
		
		Shield.UpdateFrame(0, 1, [3, 2]);
		Shield.UpdateDuration([7, 12]);
		Shield.Timer = 20d;
			
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
		if (!IsGrounded || IsSpinning || Angle is > 135f and <= 225f) return;
		if (Action is Actions.HammerDash or Actions.Dash) return;
		
		float slopeGrv = 0.125f * MathF.Sin(Mathf.DegToRad(Angle));
		if (GroundSpeed != 0f || SharedData.PlayerPhysics >= PhysicsTypes.S3 && Math.Abs(slopeGrv) > 0.05078125f)
		{
			GroundSpeed.Acceleration = -slopeGrv;
		}
	}
	
	private void ProcessSlopeResistRoll()
	{
		if (!IsGrounded || !IsSpinning || Angle is > 135f and <= 225f) return;
	
		float angleSine = MathF.Sin(Mathf.DegToRad(Angle));
		float slopeGrv = Math.Sign(GroundSpeed) != Math.Sign(angleSine) ? 0.3125f : 0.078125f;
		GroundSpeed.Acceleration = -slopeGrv * angleSine;
	}
	
	private void ProcessMovementGround()
	{
		if (!IsGrounded || IsSpinning) return;
		if (Action is Actions.SpinDash or Actions.Dash or Actions.HammerDash) return;
		
		// Cancel Knuckles' glide-landing animation
		
		if (Animation == Animations.GlideGround && (Input.Down.Down || GroundSpeed != 0))
		{
			GroundLockTimer = 0f;
		}
		
		if (GroundLockTimer <= 0f)
		{
			var doSkid = false;
			
			if (Input.Down.Left)
			{
				doSkid = MoveOnGround(Constants.Direction.Negative);
			}
			
			if (Input.Down.Right)
			{
				doSkid = MoveOnGround(Constants.Direction.Positive);
			}
			
			UpdateMovementGroundAnimation(doSkid);
			SetPushAnimation();
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
			if (GroundSpeed * sign >= 0f)
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
		
		if (Animation == Animations.Skid)
		{
			Animation = Animations.Move;
		}
		
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
		
		Angles.Quadrant quadrant = Angles.GetQuadrant(Angle);
		if (quadrant == Angles.Quadrant.Down && GroundSpeed == 0f)
		{
			Animation = Input.Down.Up ? Animations.LookUp : Input.Down.Down ? Animations.Duck : Animations.Idle;
			SetPushAnimationBy = null;
			return;
		}
			
		if (Animation == Animations.Skid) return;
		
		if (Animation != Animations.Push)
		{
			Animation = Animations.Move;
		}

		if (quadrant != Angles.Quadrant.Down || !doSkid || Math.Abs(GroundSpeed) < 4f) return;
		
		ActionValue2 = 0f; // We'll use this as a timer to spawn dust particles in UpdateStatus()
		Animation = Animations.Skid;
		
		AudioPlayer.Sound.Play(SoundStorage.Skid);
	}
	
	private void SetPushAnimation()
	{
		if (SetPushAnimationBy == null)
		{
			if (Animation != Animations.Push) return;
			Animation = Animations.Move;
			return;
		}
		
		if (!IsAnimationFrameChanged) return;
		Animation = Animations.Push;
	}
	
	private void ProcessMovementGroundRoll()
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
		if (absoluteSpeed >= 0f || Mathf.IsZeroApprox(absoluteSpeed))
		{
			Facing = direction;
			SetPushAnimationBy = null;
			return;
		}

		GroundSpeed.Acceleration = sign * PhysicParams.DecelerationRoll;
		if (sign * GroundSpeed < 0f) return;
		GroundSpeed.Value = sign * 0.5f;
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

		Radius = RadiusNormal;
		
		IsSpinning = false;
		Animation = Animations.Idle;
		return true;
	}
	
	private void ForceSpin()
	{
		if (SharedData.PlayerPhysics == PhysicsTypes.CD)
		{
			if ((float)GroundSpeed is >= 0f and < 2f)
			{
				GroundSpeed.Value = 2f;
			}
			return;
		}
		
		if (GroundSpeed != 0f) return;
		GroundSpeed.Value = (SharedData.PlayerPhysics == PhysicsTypes.S1 ? 2f : 4f) * (float)Facing;
	}
	
	private void ProcessMovementAir()
	{
		if (IsGrounded || DeathState == DeathStates.Restart) return;
		
		if (Action is Actions.Carried or Actions.Climb or Actions.Glide 
		    && (GlideStates)ActionState != GlideStates.Fall) return;

		RotateInAir();
		LimitVerticalVelocity();
		if (ChangeHammerDashFacingInAir()) return;
		MoveInAirHorizontally();
		ApplyAirDrag();
	}

	private void RotateInAir()
	{
		if (Mathf.IsEqualApprox(Angle, 360f)) return;
		
		float speed = Angles.ByteAngleStep * Scene.Local.ProcessSpeed;
		if (Angle >= 180f)
		{
			Angle += speed;
		}
		else
		{
			Angle -= speed;
		}
		
		if (Angle is <= 0f or > 360f)
		{
			Angle = 360f;
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
			MoveInAir(Constants.Direction.Negative, () => Velocity.MaxX(-PhysicParams.AccelerationTop));
		}
			
		if (Input.Down.Right)
		{
			MoveInAir(Constants.Direction.Positive, () => Velocity.MinX(PhysicParams.AccelerationTop));
		}
	}

	private void MoveInAir(Constants.Direction direction, Action action)
	{
		var sign = (float)direction;
		
		Velocity.AccelerationX = sign * PhysicParams.AccelerationAir;
		if (!SharedData.NoSpeedCap || sign * Velocity.X < PhysicParams.AccelerationTop)
		{
			action();
		}
			
		Facing = direction;
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
		if (SharedData.PlayerPhysics == PhysicsTypes.SK && Input.Down.Down) return;
		
		if (BalanceOnObject()) return;
		BalanceOnTiles();
	}
	
	private bool BalanceOnObject()
	{
		if (OnObject == null) return false;
		// TODO: check IsInstanceValid == instance_exist
		if (!IsInstanceValid(OnObject) || OnObject.SolidData.NoBalance) return true;
		
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

		return true;
	}
	
	private void BalanceOnTiles()
	{
		const Constants.Direction direction = Constants.Direction.Positive;	
		
		if (Angles.GetQuadrant(Angle) > Angles.Quadrant.Down) return;
		TileCollider.SetData((Vector2I)Position + new Vector2I(0, Radius.Y), TileLayer);
		
		if (TileCollider.FindDistance(0, 0, true, direction) < 12) return;
		
		(_, float angleLeft) = TileCollider.FindTile(-Radius.X, 0, true, direction);
		(_, float angleRight) = TileCollider.FindTile(Radius.X, 0, true, direction);
		
		if (float.IsNaN(angleLeft) ^ float.IsNaN(angleRight)) return;
		
		int sign = float.IsNaN(angleLeft) ? -1 : 1;
		bool isPanic = TileCollider.FindDistance(-6 * sign, 0, true, direction) >= 12;
		BalanceToDirection((Constants.Direction)sign, isPanic);
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
				Animation = Facing == direction ? Animations.Balance : Animations.BalanceFlip;
				Facing = direction;
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
		if (Angle is > 90f and <= 270f && (SharedData.PlayerPhysics < PhysicsTypes.SK || Angle % 90f != 0f)) return;

		int wallRadius = RadiusNormal.X + 1;
		int offsetY = 8 * (Mathf.IsEqualApprox(Angle, 360f) ? 1 : 0);

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
			
			default:
				return;
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
		if (DeathState == DeathStates.Restart) return;
		
		Framework.Camera camera = Framework.Camera.Main;
		
		if (camera == null) return;
		
		// Note that position here is checked including subpixel
		if (Position.X + Velocity.X < camera.Limit.X + 16f)
		{
			GroundSpeed.Value = 0f;
			Velocity.X = 0f;
			Position = new Vector2(camera.Limit.X + 16f, Position.Y);
		}
		
		float rightBound = camera.Limit.Z - 24f;
		//TODO: replace instance_exists
		/*if (instance_exists(obj_signpost))
		{
			// TODO: There should be a better way?
			rightBound += 64;
		}*/
		
		if (Position.X + Velocity.X > rightBound)
		{
			GroundSpeed.Value = 0f;
			Velocity.X = 0f;
			Position = new Vector2(rightBound, Position.Y);
		}
		
		switch (Action)
		{
			case Actions.Flight or Actions.Climb:
				if (Position.Y + Velocity.Y >= camera.Limit.Y + 16f) break;
	
				if (Action == Actions.Flight)
				{
					Gravity	= GravityType.TailsDown;
				}

				Velocity.Y = 0f;
				Position = new Vector2(Position.X, camera.Limit.Y + 16f);
				break;
			
			case Actions.Glide when Position.Y < camera.Limit.Y + 10f:
				Velocity.X = 0f;
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
		
		if (IsStickToConvex)
		{
			Velocity.Clamp(-16f * Vector2.One, 16f * Vector2.One);
		}
		
		Position = Velocity.CalculateNewPosition(Position);
		Velocity.Vector = Velocity.Vector;
		
		if (!IsGrounded && Action != Actions.Carried)
		{
			Velocity.AccelerationY = Gravity;
		}
	}
	
	private void ProcessCollisionGroundFloor()
	{
		if (!IsGrounded || OnObject != null) return;

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

		if (SharedData.PlayerPhysics < PhysicsTypes.S3)
		{
			if (Angles.GetQuadrant(Angle) == Angles.Quadrant.Down) return;
				
			GroundSpeed.Value = 0f;	
			GroundLockTimer = 30f;
			IsGrounded = false;
			return;
		}

		switch (Angle)
		{
			case <= 33.75f or > 326.25f:
				return;
			
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
		if (IsGrounded || DeathState == DeathStates.Restart) return;
		if (Action is Actions.Glide or Actions.Climb) return;
		
		int wallRadius = RadiusNormal.X + 1;
		Angles.Quadrant moveQuadrant = Angles.GetQuadrant(Angles.GetVector256(Velocity));
		
		TileCollider.SetData((Vector2I)Position, TileLayer);

		var moveQuadrantValue = (int)moveQuadrant;

		CollideWallsInAir(wallRadius, moveQuadrantValue, Constants.Direction.Negative);
		CollideWallsInAir(wallRadius, moveQuadrantValue, Constants.Direction.Positive);

		if (CollideWithCeilingInAir(wallRadius, moveQuadrant)) return;

		CollideWithFloorInAir(moveQuadrant);
	}

	private void CollideWallsInAir(int wallRadius, int moveQuadrantValue, Constants.Direction direction)
	{
		var sign = (int)direction;
		
		if (moveQuadrantValue == (int)Angles.Quadrant.Up + sign) return;
		
		int wallDistance = TileCollider.FindDistance(sign * wallRadius, 0, false, direction);
		
		if (wallDistance >= 0f) return;
		Position += new Vector2(sign * wallDistance, 0f);
		TileCollider.Position = (Vector2I)Position;
		Velocity.X = 0f;
		
		if (moveQuadrantValue != (int)Angles.Quadrant.Up - sign) return;
		GroundSpeed.Value = Velocity.Y;
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
		int distance;
		float angle;

		if (moveQuadrant == Angles.Quadrant.Down)
		{
			if (LandOnFeet(out distance, out angle)) return;
		}
		else if (Velocity.Y >= 0)
		{
			if (FallOnGround(out distance, out angle)) return;
		}
		else
		{
			return;
		}
		
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
		
		float minClip = -(Velocity.Y + 8f);		
		if (distance >= 0 || minClip >= distanceL && minClip >= distanceR) return true;
		
		if (Angles.GetQuadrant(angle) > 0)
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
			GroundSpeed.Value /= 2f;
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
