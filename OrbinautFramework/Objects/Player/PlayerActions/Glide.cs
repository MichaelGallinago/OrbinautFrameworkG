using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

public struct Glide(Glide.States state) : IAction
{
	public enum States : byte
	{
		Air, Ground, Fall
	}

	public void Perform(Player player)
    {
	    if (state == States.Fall) return;
		
	    switch (state)
	    {
		    case States.Air: GlideAir(); break;
		    case States.Ground: GlideGround(); break;
	    }
    }

	private void GlideAir()
	{
		UpdateSpeed();
		TurnAroundAir();
		UpdateGravityAndHorizontalVelocity();
		UpdateAirAnimationFrame();
		
		if (Input.Down.Abc) return;

		ReleaseGlide();
		Velocity.X *= 0.25f;
	}

	private void UpdateSpeed()
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

	private void UpdateGravityAndHorizontalVelocity()
	{
		const float glideGravity = 0.125f;

		Velocity.X = GroundSpeed * -MathF.Cos(Mathf.DegToRad(ActionValue));
		Gravity = Velocity.Y < 0.5f ? glideGravity : -glideGravity;
	}

	private void UpdateAirAnimationFrame()
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
		UpdateGroundVelocityX();
		
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

	private void UpdateGroundVelocityX()
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

	private void TurnAroundAir()
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
	
	private void ProcessCollision()
	{
		if (Action != Actions.Glide) return;
		
		var climbY = (int)Position.Y;
		var collisionFlagWall = false;
		int wallRadius = RadiusNormal.X + 1;
		Angles.Quadrant moveQuadrant = Angles.GetQuadrant(Angles.GetVector256(Velocity));

		TileCollider.SetData((Vector2I)Position, TileLayer);
		
		if (moveQuadrant != Angles.Quadrant.Right)
		{
			collisionFlagWall |= CollideWalls(wallRadius, Constants.Direction.Negative);
		}
		
		if (moveQuadrant != Angles.Quadrant.Left)
		{
			collisionFlagWall |= CollideWalls(wallRadius, Constants.Direction.Positive);
		}
		
		collisionFlagWall |= CollideCeiling(wallRadius, moveQuadrant);
		
		if (moveQuadrant != Angles.Quadrant.Up && CollideFloor())
		{
			LandWhenGlide();
		}
		else if (collisionFlagWall)
		{
			AttachToWall(wallRadius, climbY);
		}
	}

	private bool CollideWalls(int wallRadius, Constants.Direction direction)
	{
		var sing = (int)direction;
		int wallDistance = TileCollider.FindDistance(sing * wallRadius, 0, false, direction);

		if (wallDistance >= 0) return false;
		
		Position += new Vector2(sing * wallDistance, 0f);
		TileCollider.Position = (Vector2I)Position;
		Velocity.X = 0f;
		return true;
	}

	private bool CollideCeiling(int wallRadius, Angles.Quadrant moveQuadrant)
	{
		if (moveQuadrant == Angles.Quadrant.Down) return false;
		
		int roofDistance = TileCollider.FindClosestDistance(
			-Radius.X, -Radius.Y, Radius.X, -Radius.Y, 
			true, Constants.Direction.Negative);
			
		if (moveQuadrant == Angles.Quadrant.Left && roofDistance <= -14 && SharedData.PlayerPhysics >= PhysicsTypes.S3)
		{
			// Perform right wall collision instead if moving mostly left and too far into the ceiling
			return CollideWalls(wallRadius, Constants.Direction.Positive);
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

	private bool CollideFloor()
	{
		(int floorDistance, float floorAngle) = TileCollider.FindClosestTile(
			-Radius.X, Radius.Y, Radius.X, Radius.Y,
			true, Constants.Direction.Positive);
	
		if (state == States.Ground)
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
		switch (state)
		{
			case States.Air: LandAir(); break;
			case States.Fall: LandFall(); break;
		}
	}

	private void LandAir()
	{
		if (Angles.GetQuadrant(Angle) != Angles.Quadrant.Down)
		{
			GroundSpeed.Value = Angle < 180 ? Velocity.X : -Velocity.X;
			Land();
			return;
		}
				
		Animation = Animations.GlideGround;
		state = States.Ground;
		ActionValue = 0f;
		Gravity = 0f;
	}

	private void LandFall()
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

	private void AttachToWall(int wallRadius, int climbY)
	{
		if (state != (int)States.Air) return;

		CheckCollisionOnAttaching(wallRadius, climbY);
			
		if (Facing == Constants.Direction.Negative)
		{
			Position += Vector2.Right;
		}

		bool isWallJump = SharedData.SuperstarsTweaks && (Input.Down.Up || Input.Down.Down);
		state = (int)(isWallJump ? ClimbStates.WallJump : ClimbStates.Normal);
		Action = Actions.Climb;
		Animation = Animations.ClimbWall;
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
		state = (int)States.Fall;
		ActionValue = 0f;
		Radius = RadiusNormal;
		
		ResetGravity();
	}
}
