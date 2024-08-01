using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;

namespace OrbinautFramework3.Objects.Player.Physics.Collisions;

public struct Air
{
    public void Collide()
	{
		if (IsGrounded || IsDead || Action is Actions.Glide or Actions.Climb) return;
		
		int wallRadius = RadiusNormal.X + 1;
		Angles.Quadrant moveQuadrant = Angles.GetQuadrant(Angles.GetVector256(Velocity));
		
		TileCollider.SetData((Vector2I)Position, TileLayer);

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
		
		int wallDistance = TileCollider.FindDistance(sign * wallRadius, 0, false, direction);
		
		if (wallDistance >= 0f) return false;
		Position += new Vector2(sign * wallDistance, 0f);
		TileCollider.Position = (Vector2I)Position;
		Velocity.X = 0f;
		
		if (moveQuadrantValue != (int)Angles.Quadrant.Up - sign) return false;
		GroundSpeed.Value = Velocity.Y;
		return true;
	}

	private bool CollideCeiling(int wallRadius, Angles.Quadrant moveQuadrant)
	{
		if (moveQuadrant == Angles.Quadrant.Down) return false;
		
		(int roofDistance, float roofAngle) = TileCollider.FindClosestTile(
			-Radius.X, -Radius.Y, Radius.X, -Radius.Y,
			true, Constants.Direction.Negative);
			
		if (moveQuadrant == Angles.Quadrant.Up && SharedData.PhysicsType >= PhysicsTypes.S3 && roofDistance <= -14f)
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
	
	private void CollideFloor(Angles.Quadrant moveQuadrant)
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
