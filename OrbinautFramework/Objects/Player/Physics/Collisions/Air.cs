using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Modules;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Physics.Collisions;

public struct Air(PlayerData data)
{
    public void Collide()
	{
		if (data.Movement.IsGrounded || data.Death.IsDead || data.State is States.Glide or States.Climb) return;
		
		int wallRadius = data.Collision.RadiusNormal.X + 1;
		Angles.Quadrant moveQuadrant = Angles.GetQuadrant(Angles.GetVector256(data.Movement.Velocity));
		
		data.TileCollider.SetData((Vector2I)data.Node.Position, data.Collision.TileLayer);

		var moveQuadrantValue = (int)moveQuadrant;

		if (CollideWalls(wallRadius, moveQuadrantValue, Constants.Direction.Negative)) return;
		if (CollideWalls(wallRadius, moveQuadrantValue, Constants.Direction.Positive)) return;

		if (CollideCeiling(wallRadius, moveQuadrant)) return;

		CollideFloor(moveQuadrant);
	}

	private bool CollideWalls(int wallRadius, int moveQuadrantValue, Constants.Direction direction)
	{
		var sign = (int)direction;
		
		if (moveQuadrantValue == (int)Angles.Quadrant.Up + sign) return false;
		
		int wallDistance = data.TileCollider.FindDistance(sign * wallRadius, 0, false, direction);
		
		if (wallDistance >= 0f) return false;
		data.Node.Position += new Vector2(sign * wallDistance, 0f);
		data.TileCollider.Position = (Vector2I)data.Node.Position;
		data.Movement.Velocity.X = 0f;
		
		if (moveQuadrantValue != (int)Angles.Quadrant.Up - sign) return false;
		data.Movement.GroundSpeed.Value = data.Movement.Velocity.Y;
		return true;
	}

	private bool CollideCeiling(int wallRadius, Angles.Quadrant moveQuadrant)
	{
		if (moveQuadrant == Angles.Quadrant.Down) return false;
		
		(int roofDistance, float roofAngle) = data.TileCollider.FindClosestTile(
			-data.Collision.Radius.X, -data.Collision.Radius.Y, data.Collision.Radius.X, -data.Collision.Radius.Y,
			true, Constants.Direction.Negative);
			
		if (moveQuadrant == Angles.Quadrant.Up && SharedData.PhysicsType >= PhysicsCore.Types.S3 && roofDistance <= -14f)
		{
			// Perform right wall collision if moving mostly left and too far into the ceiling
			int wallDist = data.TileCollider.FindDistance(wallRadius, 0, false, Constants.Direction.Positive);

			if (wallDist >= 0) return false;
			
			data.Node.Position += new Vector2(wallDist, 0f);
			data.Movement.Velocity.X = 0f;
			return true;
		}
		
		if (roofDistance >= 0) return false;
		
		data.Node.Position -= new Vector2(0f, roofDistance);
		if (moveQuadrant == Angles.Quadrant.Up && data.State != States.Flight && 
		    Angles.GetQuadrant(roofAngle) is Angles.Quadrant.Right or Angles.Quadrant.Left)
		{
			data.Movement.Angle = roofAngle;
			data.Movement.GroundSpeed.Value = roofAngle < 180f ? -data.Movement.Velocity.Y : data.Movement.Velocity.Y;
			data.Movement.Velocity.Y = 0f;
					
			Land();
			return true;
		}
		
		if (data.Movement.Velocity.Y < 0f)
		{
			data.Movement.Velocity.Y = 0f;
		}
		
		if (data.State == States.Flight)
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
		
		Land();
	}

	private bool LandOnFeet(out int distance, out float angle)
	{
		(int distanceL, float angleL) = data.TileCollider.FindTile(
			-data.Collision.Radius.X, data.Collision.Radius.Y, true, Constants.Direction.Positive);
			
		(int distanceR, float angleR) = data.TileCollider.FindTile(
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
		(distance, angle) = data.TileCollider.FindClosestTile(
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
