using Godot;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.MathTypes;
using OrbinautFrameworkG.Framework.StaticStorages;
using OrbinautFrameworkG.Framework.Tiles;
using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Logic;
using static OrbinautFrameworkG.Objects.Player.ActionFsm;

namespace OrbinautFrameworkG.Objects.Player.Physics.Collisions;

public readonly struct Air(PlayerData data, IPlayerLogic logic)
{
    public void Collide()
	{
		if (logic.Action is States.GlideAir or States.GlideFall or States.GlideGround or States.Climb) return;
		
		int wallRadius = data.Collision.RadiusNormal.X + 1;
		Angles.Quadrant moveQuadrant = Angles.GetQuadrant(Angles.GetRoundedVector(data.Movement.Velocity));
		
		logic.TileCollider.SetData((Vector2I)data.Movement.Position, data.Collision.TileLayer);

		var moveQuadrantValue = (int)moveQuadrant;

		if (CollideWalls(wallRadius, moveQuadrantValue, Constants.Direction.Negative)) return;
		if (CollideWalls(wallRadius, moveQuadrantValue, Constants.Direction.Positive)) return;
		
#if S3_PHYSICS || SK_PHYSICS
		if (CollideCeiling(wallRadius, moveQuadrant)) return;
#else
		if (CollideCeiling(moveQuadrant)) return;
#endif

		CollideFloor(moveQuadrant);
	}

	private bool CollideWalls(int wallRadius, int moveQuadrantValue, Constants.Direction direction)
	{
		var sign = (int)direction;
		
		if (moveQuadrantValue == (int)Angles.Quadrant.Up + sign) return false;
		
		int wallDistance = logic.TileCollider.FindDistance(sign * wallRadius, 0, false, direction);
		if (wallDistance >= 0f) return false;

		MovementData movement = data.Movement;
		movement.Position.X += sign * wallDistance;
		logic.TileCollider.Position = (Vector2I)movement.Position;
		movement.Velocity.X = 0f;
		
		if (moveQuadrantValue != (int)Angles.Quadrant.Up - sign) return false;
		movement.GroundSpeed = movement.Velocity.Y;
		return true;
	}

#if S3_PHYSICS || SK_PHYSICS
	private bool CollideCeiling(int wallRadius, Angles.Quadrant moveQuadrant)
#else
	private bool CollideCeiling(Angles.Quadrant moveQuadrant)
#endif
	{
		if (moveQuadrant == Angles.Quadrant.Down) return false;

		Vector2I radius = data.Collision.Radius;
		(int roofDistance, float roofAngle) = logic.TileCollider.FindClosestTile(
			-radius.X, -radius.Y, radius.X, -radius.Y, true, Constants.Direction.Negative);

#if S3_PHYSICS || SK_PHYSICS
		if (moveQuadrant == Angles.Quadrant.Up && roofDistance <= -14f)
		{
			// Perform right wall collision if moving mostly left and too far into the ceiling
			int wallDistance = logic.TileCollider.FindDistance(
				wallRadius, 0, false, Constants.Direction.Positive);

			if (wallDistance >= 0) return false;
			
			data.Movement.Position.X += wallDistance;
			data.Movement.Velocity.X = 0f;
			return true;
		}
#endif
		
		if (roofDistance >= 0) return false;

		MovementData movement = data.Movement;
		
		movement.Position.Y -= roofDistance;
		if (moveQuadrant == Angles.Quadrant.Up && logic.Action != States.Flight && 
		    Angles.GetQuadrant(roofAngle) is Angles.Quadrant.Right or Angles.Quadrant.Left)
		{
			movement.Angle = roofAngle;
			movement.GroundSpeed = roofAngle < 180f ? -movement.Velocity.Y : movement.Velocity.Y;
			movement.Velocity.Y = 0f;
					
			logic.Land();
			return true;
		}
		
		if (movement.Velocity.Y < 0f)
		{
			movement.Velocity.Y = 0f;
		}
		
		if (logic.Action == States.Flight)
		{
			movement.Gravity = GravityType.FlightDown;
		}
		
		return true;
	}
	
	private void CollideFloor(Angles.Quadrant moveQuadrant)
	{
		if (moveQuadrant == Angles.Quadrant.Up) return;
		
		int distance;
		float angle;
		
		if (moveQuadrant == Angles.Quadrant.Down)
		{
			if (LandOnFeet(out distance, out angle)) return;
		}
		else if (data.Movement.Velocity.Y >= 0)
		{
			// If moving mostly left or right, continue if our vertical velocity is positive
			if (FallOnGround(out distance, out angle)) return;
		}
		else
		{
			return;
		}
		
		data.Movement.Position.Y += distance;
		data.Movement.Angle = angle;
		
		logic.Land();
	}
	
	private bool LandOnFeet(out int distance, out float angle)
	{
		Vector2I radius = data.Collision.Radius;
		
		(int distanceLeft, float angleLeft) = logic.TileCollider.FindTile(
			-radius.X, radius.Y, true, Constants.Direction.Positive);
		
		(int distanceRight, float angleRight) = logic.TileCollider.FindTile(
			radius.X, radius.Y, true, Constants.Direction.Positive);
		
		if (distanceLeft > distanceRight)
		{
			distance = distanceRight;
			angle = angleRight;
		}
		else
		{
			distance = distanceLeft;
			angle = angleLeft;
		}
		
		// Exit if BOTH sensors are way too far into the surface. This means the game doesn't care
		// how far we're clipping into the ground if we're landing on a ledge
		
		if (distance >= 0) return true;
		float minimalClip = -(data.Movement.Velocity.Y + 8f);
		if (minimalClip >= distanceLeft && minimalClip >= distanceRight) return true;
		
		SetLandingSpeedAndVelocity(angle);
		
		return false;
	}
	
	private void SetLandingSpeedAndVelocity(float angle)
	{
		MovementData movement = data.Movement;
		AcceleratedVector2 velocity = movement.Velocity;
		
		if (Angles.GetQuadrant(angle) != Angles.Quadrant.Down)
		{
			if (velocity.Y > 15.75f)
			{
				velocity.Y = 15.75f;
			}
			
			movement.GroundSpeed = angle < 180f ? -velocity.Y : velocity.Y;
			velocity.X = 0f;
		}
		else if (angle is > 22.5f and <= 337.5f)
		{
			movement.GroundSpeed = angle < 180f ? -velocity.Y : velocity.Y;
			movement.GroundSpeed *= 0.5f;
		}
		else
		{
			movement.GroundSpeed = velocity.X;
			velocity.Y = 0f;
		}
	}

	private bool FallOnGround(out int distance, out float angle)
	{
		Vector2I radius = data.Collision.Radius;
		(distance, angle) = logic.TileCollider.FindClosestTile(
			-radius.X, radius.Y, radius.X, radius.Y, 
			true, Constants.Direction.Positive);
		
		if (distance >= 0) return true;

		MovementData movement = data.Movement;
		movement.GroundSpeed = movement.Velocity.X;
		movement.Velocity.Y = 0f;
		
		return false;
	}
}
