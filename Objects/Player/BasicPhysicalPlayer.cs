using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Spawnable.Barrier;

namespace OrbinautFramework3.Objects.Player;

public abstract partial class BasicPhysicalPlayer : PlayerData
{
	protected event Action LandHandler;
	protected PhysicParams PhysicParams;
	
	public override void _Process(double delta)
	{
		UpdatePhysicParams();
		ProcessCorePhysics();
	}

	protected void UpdatePhysicParams() => PhysicParams = PhysicParams.Get(IsUnderwater, IsSuper, Type, ItemSpeedTimer);
	
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
		
		Action = Actions.None;
		IsDead = true;
		ObjectInteraction = false;
		IsGrounded = false;
		OnObject = null;
		Barrier.Type = Barrier.Types.None;
		Animation = Animations.Death;
		Gravity = GravityType.Default;
		Velocity.Vector = new Vector2(0f, -7f);
		GroundSpeed = 0f;
		ZIndex = (int)Constants.ZIndexes.AboveForeground;
		
		//TODO: Audio
		//audio_play_sfx(sfx_hurt);

		if (Id != 0) return;
		FrameworkData.UpdateObjects = false;
		FrameworkData.UpdateTimer = false;
		FrameworkData.AllowPause = false;
	}
	
	public void Land()
	{
		ResetGravity();
		
		IsGrounded = true;
	
		if (Action == Actions.Flight)
		{
			//TODO: audio
			//audio_stop_sfx(sfx_flight);
			//audio_stop_sfx(sfx_flight2);
		}
		else if (Action is Actions.SpinDash or Actions.PeelOut)
		{
			if (Action == Actions.PeelOut)
			{
				GroundSpeed = ActionValue2;
			}
			
			return;
		}
	
		if (Barrier.State == Barrier.States.Active && Barrier.Type == Barrier.Types.Water)
		{
			float force = IsUnderwater ? -4f : -7.5f;
			float radians = Mathf.DegToRad(Angle);
			Velocity.Vector = new Vector2(MathF.Sin(radians), MathF.Sin(radians)) * force;

			Barrier.State = Barrier.States.None;
			OnObject = null;
			IsGrounded = false;
		
			Barrier.UpdateFrame(0, 1, [3, 2]);
			Barrier.UpdateDuration([7, 12]);
			Barrier.Timer = 20d;
			
			//TODO: audio
			//audio_play_sfx(sfx_barrier_water2);
		
			return;
		}
	
		if (OnObject == null)
		{
			switch (Animation)
			{
				case Animations.Idle:
				case Animations.Duck:
				case Animations.HammerDash:
				case Animations.GlideGround: 
					break;
			
				default:
					Animation = Animations.Move;
					break;
			}
		}
		else
		{
			Animation = Animations.Move;
		}
	
		if (IsHurt)
		{
			InvincibilityTimer = 120;
			GroundSpeed = 0f;
		}
	
		IsAirLock = false;
		IsSpinning	= false;
		IsJumping = false;
		SetPushAnimationBy = null;
		IsHurt = false;
	
		Barrier.State = Barrier.States.None;
		ComboCounter = 0;
	
		CpuState = CpuStates.Main;

		LandHandler?.Invoke();
	
		if (Action != Actions.HammerDash)
		{
			Action = Actions.None;
		}
		else
		{
			GroundSpeed	= 6 * (int)Facing;
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
		GroundSpeed = Velocity.X;
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
		if (Animation is not Animations.Spin and not Animations.SpinDash)
		{
			Animation = Animations.Move;
		}
		
		SetPushAnimationBy = null;
	}
	
    private void ProcessSlopeResist()
	{
		if (!IsGrounded || IsSpinning || Angle is > 135f and <= 225f) return;
		if (Action is Actions.HammerDash or Actions.PeelOut) return;
		
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
		if (Action is Actions.SpinDash or Actions.PeelOut or Actions.HammerDash) return;
		
		// If Knuckles is standing up from a slide and DOWN button is pressed, cancel
		// control lock. This allows him to Spin Dash
		if (Animation == Animations.GlideGround && Input.Down.Down)
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
				GroundSpeed = 0.5f * sign;
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
		
		byte quadrant = Angles.GetQuadrant(Angle);
		if (quadrant == 0 && GroundSpeed == 0f)
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

		if (quadrant != 0 || !doSkid || Math.Abs(GroundSpeed) < 4f) return;
		
		ActionValue2 = 0f; // We'll use this as a timer to spawn dust particles in UpdateStatus()
		Animation = Animations.Skid;
		
		//TODO: audio
		//audio_play_sfx(sfx_skid);
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
		// Control routine checks
		if (!IsGrounded || !IsSpinning) return;
		
		if (GroundLockTimer <= 0f)
		{
			if (Input.Down.Left)
			{
				RollOnGround(Constants.Direction.Negative); // Move left
			}
			
			if (Input.Down.Right)
			{
				RollOnGround(Constants.Direction.Positive); // Move right
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
		GroundSpeed = sign * 0.5f;
	}
	
	private void UpdateSpinningOnGround()
	{
		if (StopSpinning()) return;
		ForceSpin();
	}
	
	private bool StopSpinning()
	{
		if (IsForcedRoll) return false;
		
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
				GroundSpeed = 2f;
			}
			return;
		}
		
		if (GroundSpeed != 0f) return;
		GroundSpeed = (SharedData.PlayerPhysics == PhysicsTypes.S1 ? 2f : 4f) * (float)Facing;
	}
	
	private void ProcessMovementAir()
	{
		if (IsGrounded || IsDead) return;
		
		// Action checks
		if (Action is Actions.Carried or Actions.Climb or Actions.Glide 
		    && (GlideStates)ActionState != GlideStates.Fall) return;
	
		// Update Angle (rotate player)
		if (!Mathf.IsEqualApprox(Angle, 360f))
		{
			float speed = Angles.ByteAngleStep * FrameworkData.ProcessSpeed;
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
	
		// Limit upward speed
		if (!IsJumping && Action != Actions.SpinDash && !IsForcedRoll && Velocity.Y < -15.75f)
		{
			Velocity.Y = -15.75f;
		}
	
		// Limit downward speed
		if (SharedData.PlayerPhysics == PhysicsTypes.CD && Velocity.Y > 16f)
		{
			Velocity.Y = 16f;
		}

		if (Action == Actions.HammerDash)
		{
			if (Input.Down.Left)
			{
				Facing = Constants.Direction.Negative;
			}
			else if (Input.Down.Right)
			{
				Facing = Constants.Direction.Positive;
			}
			
			return;
		}
	
		if (!IsAirLock)
		{
			if (Input.Down.Left)
			{
				MoveInAir(Constants.Direction.Negative, () => Velocity.MaxX(-PhysicParams.AccelerationTop));
			}
			
			if (Input.Down.Right)
			{
				MoveInAir(Constants.Direction.Positive, () => Velocity.MinX(PhysicParams.AccelerationTop));
			}
		}
	
		// Apply air drag
		if (!IsHurt && Velocity.Y is < 0f and > -4f)
		{
			Velocity.AccelerationX = MathF.Floor(Velocity.X * 8f) / -256f;
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
	
	private void ProcessBalance()
	{
		if (!IsGrounded || IsSpinning) return;
		if (GroundSpeed != 0 || Action is Actions.SpinDash or Actions.PeelOut) return;
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

		return true;
	}
	
	private void BalanceOnTiles()
	{
		const Constants.Direction direction = Constants.Direction.Positive;	
		
		if (Angles.GetQuadrant(Angle) > 0) return;
		TileCollider.SetData((Vector2I)Position + new Vector2I(0, Radius.Y), TileLayer, TileMap);
		
		if (TileCollider.FindDistance(new Vector2I(), true, direction) < 12) return;
			
		(_, float angleLeft) = TileCollider.FindTile(new Vector2I(-Radius.X, 0), true, direction);
		(_, float angleRight) = TileCollider.FindTile(new Vector2I(Radius.X, 0), true, direction);
		
		if (!float.IsNaN(angleLeft) && !float.IsNaN(angleRight)) return;
		
		int sign = float.IsNaN(angleLeft) ? -1 : 1;
		bool isPanic = TileCollider.FindDistance(new Vector2I(-6 * sign, 0), true, direction) >= 12;
		BalanceToDirection((Constants.Direction)sign, isPanic);
	}

	private void BalanceToDirection(Constants.Direction direction, bool isPanic)
	{
		switch (Type)
		{
			case Types.Amy or Types.Tails:
			case Types.Sonic when IsSuper:
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
		
		if (SharedData.PlayerPhysics < PhysicsTypes.SK)
		{
			if (Angle is > 90f and <= 270f) return;
		}
		else if (Angle is >= 90f and <= 270f && Angle % 90f != 0f)
		{
			return;
		}

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
		
		TileCollider.SetData((Vector2I)Velocity.CalculateNewPosition(Position), TileLayer, TileMap, GroundMode);
		
		int castQuadrant = Angle switch
		{
			>= 45f and <= 128f => 1,
			> 128f and < 225f => 2,
			>= 225f and < 315f => 3,
			_ => 0
		};
		
		int wallDistance = castQuadrant switch
		{
			0 => TileCollider.FindDistance(new Vector2I(-wallRadius, offsetY), false, firstDirection),
			1 => TileCollider.FindDistance(new Vector2I(0, wallRadius), true, secondDirection),
			2 => TileCollider.FindDistance(new Vector2I(wallRadius, 0), false, secondDirection),
			3 => TileCollider.FindDistance(new Vector2I(0, -wallRadius), true, firstDirection),
			_ => throw new ArgumentOutOfRangeException()
		};
		
		if (wallDistance >= 0) return;
		byte quadrant = Angles.GetQuadrant(Angle);
		wallDistance *= quadrant > 1 ? -sign : sign;
		
		switch (quadrant & 1)
		{
			case 0:
				Velocity.X -= wallDistance / FrameworkData.ProcessSpeed;
				GroundSpeed = 0f;
					
				if (Facing == firstDirection && !IsSpinning)
				{
					SetPushAnimationBy = this;
				}
				break;
				
			case 1:
				Velocity.Y += wallDistance / FrameworkData.ProcessSpeed;
				break;
		}
	}
	
	private void ProcessRollStart()
	{
		if (!IsGrounded || IsSpinning || Action is Actions.SpinDash or Actions.HammerDash) return;
		if (!IsForcedRoll && (Input.Down.Left || Input.Down.Right)) return;
		
		var allowSpin = false;
		if (Input.Down.Down)
		{
			if (SharedData.PlayerPhysics == PhysicsTypes.SK)
			{
				if (Math.Abs(GroundSpeed) >= 1f)
				{
					allowSpin = true;
				}
				else
				{
					Animation = Animations.Duck;
				}
			}
			else if (Math.Abs(GroundSpeed) >= 0.5f)
			{
				allowSpin = true;
			}
		}

		if (!allowSpin && !IsForcedRoll) return;
		Position += new Vector2(0f, Radius.Y - RadiusSpin.Y);
		Radius = RadiusSpin;
		IsSpinning = true;
		Animation = Animations.Spin;
		
		//TODO: audio
		//audio_play_sfx(sfx_roll);
	}
	
	private void ProcessLevelBound()
	{
		if (IsDead) return;
		
		Camera camera = Camera.Main;
		
		if (camera == null) return;
		
		// Note that position here is checked including subpixel
		if (Position.X + Velocity.X < camera.Limit.X + 16f)
		{
			GroundSpeed = 0f;
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
			GroundSpeed = 0f;
			Velocity.X = 0f;
			Position = new Vector2(rightBound, Position.Y);
		}
		
		switch (Action)
		{
			case Actions.Flight or Actions.Climb:
			{
				if (Position.Y + Velocity.Y < camera.Limit.Y + 16f)
				{ 	
					if (Action == Actions.Flight)
					{
						Gravity	= GravityType.TailsDown;
					}

					Velocity.Y = 0f;
					Position = new Vector2(Position.X, camera.Limit.Y + 16f);
				}

				break;
			}
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
		
		if (StickToConvex)
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

		GroundMode = Angle switch
		{
			<= 45 or >= 315 => Constants.GroundMode.Floor,
			> 45 and < 135 => Constants.GroundMode.RightWall,
			>= 135 and <= 225 => Constants.GroundMode.Ceiling,
			_ => Constants.GroundMode.LeftWall
		};
		
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
		
		const int minTolerance = 4;
		const int maxTolerance = 14;
		
		if (!StickToConvex)
		{
			float toleranceCheckSpeed = GroundMode switch
			{
				Constants.GroundMode.Floor or Constants.GroundMode.Ceiling => Velocity.X,
				Constants.GroundMode.RightWall or Constants.GroundMode.LeftWall => Velocity.Y,
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
			GroundLockTimer -= FrameworkData.ProcessSpeed;
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
					GroundSpeed.Acceleration = Angle < 180f ? -0.5f : 0.5f;
				}
		
				GroundLockTimer = 30f;
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
		byte moveQuadrant = Angles.GetQuadrant(Angles.GetVector256(Velocity));
		
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
				Velocity.X = 0f;
				
				if (moveQuadrant == 3)
				{
					GroundSpeed = Velocity.Y;
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
				Velocity.X = 0f;
				
				if (moveQuadrant == 1)
				{
					GroundSpeed = Velocity.Y;
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
					Velocity.X = 0f;
					
					return;
				}
			}
			else if (roofDistance < 0)
			{
				Position -= new Vector2(0f, roofDistance);
				if (moveQuadrant == 2 && Angles.GetQuadrant(roofAngle) % 2 > 0 && Action != Actions.Flight)
				{
					Angle = roofAngle;
					GroundSpeed = roofAngle < 180f ? -Velocity.Y : Velocity.Y;
					Velocity.Y = 0f;
					
					Land();
				}
				else
				{
					if (Velocity.Y < 0f)
					{
						Velocity.Y = 0f;
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
					
			float minClip = -(Velocity.Y + 8f);		
			if (distance >= 0 || minClip >= distanceL && minClip >= distanceR) return;
					
			if (Angles.GetQuadrant(angle) > 0)
			{
				if (Velocity.Y > 15.75f)
				{
					Velocity.Y = 15.75f;
				}
						
				GroundSpeed = angle < 180f ? -Velocity.Y : Velocity.Y;
				Velocity.X = 0f;
			}
			else if (angle is > 22.5f and <= 337.5f)
			{
				GroundSpeed = angle < 180f ? -Velocity.Y : Velocity.Y;
				GroundSpeed /= 2f;
			}
			else 
			{
				GroundSpeed = Velocity.X;
				Velocity.Y = 0f;
			}
		}
		else if (Velocity.Y >= 0)
		{
			(distance, angle) = TileCollider.FindClosestTile(
				Radius.Shuffle(-1, 1), 
				Radius.Shuffle(1, 1),
				true, Constants.Direction.Positive);
			
			if (distance >= 0) return;
				
			GroundSpeed = Velocity.X;
			Velocity.Y = 0f;
		}
		else
		{
			return;
		}

		Position += new Vector2(0f, distance);
		Angle = angle;
			
		Land();
	}
}
