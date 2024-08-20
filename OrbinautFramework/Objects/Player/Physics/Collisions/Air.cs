using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Physics.Collisions;

public struct Air(PlayerData data, IPlayerLogic logic)
{
    public void Collide()
	{
		if (data.Movement.IsGrounded || data.Death.IsDead) return;
		if (logic.Action is States.GlideAir or States.GlideFall or States.GlideGround or States.Climb) return;
		
		int wallRadius = data.Collision.RadiusNormal.X + 1;
		Angles.Quadrant moveQuadrant = Angles.GetQuadrant(Angles.GetVector256(data.Movement.Velocity));
		
		logic.TileCollider.SetData((Vector2I)data.Node.Position, data.Collision.TileLayer);

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
		data.Node.Position += new Vector2(sign * wallDistance, 0f);
		logic.TileCollider.Position = (Vector2I)data.Node.Position;
		data.Movement.Velocity.X = 0f;
		
		if (moveQuadrantValue != (int)Angles.Quadrant.Up - sign) return false;
		data.Movement.GroundSpeed.Value = data.Movement.Velocity.Y;
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
			int wallDist = logic.TileCollider.FindDistance(wallRadius, 0, false, Constants.Direction.Positive);

			if (wallDist >= 0) return false;
			
			data.Node.Position += new Vector2(wallDist, 0f);
			data.Movement.Velocity.X = 0f;
			return true;
		}
#endif
		
		if (roofDistance >= 0) return false;
		
		data.Node.Position -= new Vector2(0f, roofDistance);
		if (moveQuadrant == Angles.Quadrant.Up && logic.Action != States.Flight && 
		    Angles.GetQuadrant(roofAngle) is Angles.Quadrant.Right or Angles.Quadrant.Left)
		{
			data.Movement.Angle = roofAngle;
			data.Movement.GroundSpeed.Value = roofAngle < 180f ? -data.Movement.Velocity.Y : data.Movement.Velocity.Y;
			data.Movement.Velocity.Y = 0f;
					
			logic.Land();
			return true;
		}
		
		if (data.Movement.Velocity.Y < 0f)
		{
			data.Movement.Velocity.Y = 0f;
		}
		
		if (logic.Action == States.Flight)
		{
			data.Movement.Gravity = GravityType.TailsDown;
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
		else if (data.Movement.Velocity.Y >= 0) // If moving mostly left or right, continue if our vertical velocity is positive
		{
			if (FallOnGround(out distance, out angle)) return;
		}
		else return;
		
		data.Node.Position += new Vector2(0f, distance);
		data.Movement.Angle = angle;
		
		logic.Land();
	}

	private bool LandOnFeet(out int distance, out float angle)
	{
		(int distanceL, float angleL) = logic.TileCollider.FindTile(
			-data.Collision.Radius.X, data.Collision.Radius.Y, true, Constants.Direction.Positive);
			
		(int distanceR, float angleR) = logic.TileCollider.FindTile(
			data.Collision.Radius.X, data.Collision.Radius.Y, true, Constants.Direction.Positive);

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
		
		float minClip = -(data.Movement.Velocity.Y + 8f);		
		if (distance >= 0 || minClip >= distanceL && minClip >= distanceR) return true;
		
		if (Angles.GetQuadrant(angle) != Angles.Quadrant.Down)
		{
			if (data.Movement.Velocity.Y > 15.75f)
			{
				data.Movement.Velocity.Y = 15.75f;
			}
			
			data.Movement.GroundSpeed.Value = angle < 180f ? -data.Movement.Velocity.Y : data.Movement.Velocity.Y;
			data.Movement.Velocity.X = 0f;
		}
		else if (angle is > 22.5f and <= 337.5f)
		{
			data.Movement.GroundSpeed.Value = angle < 180f ? -data.Movement.Velocity.Y : data.Movement.Velocity.Y;
			data.Movement.GroundSpeed.Value *= 0.5f;
		}
		else 
		{
			data.Movement.GroundSpeed.Value = data.Movement.Velocity.X;
			data.Movement.Velocity.Y = 0f;
		}
		
		return false;
	}

	private bool FallOnGround(out int distance, out float angle)
	{
		(distance, angle) = logic.TileCollider.FindClosestTile(
			-data.Collision.Radius.X, 
			data.Collision.Radius.Y, 
			data.Collision.Radius.X, 
			data.Collision.Radius.Y,
			true, 
			Constants.Direction.Positive);
		
		if (distance >= 0) return true;
		
		data.Movement.GroundSpeed.Value = data.Movement.Velocity.X;
		data.Movement.Velocity.Y = 0f;
		
		return false;
	}
}
