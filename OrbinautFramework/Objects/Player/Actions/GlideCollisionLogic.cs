using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;

namespace OrbinautFramework3.Objects.Player.Actions;

public readonly struct GlideCollisionLogic(PlayerData data, IPlayerLogic logic)
{
	public bool CollideFloor()
	{
		Vector2I radius = data.Collision.Radius;
		(int floorDistance, float floorAngle) = logic.TileCollider.FindClosestTile(
			-radius.X, radius.Y, radius.X, radius.Y, true, Constants.Direction.Positive);
		
		if (floorDistance >= 0) return false;
		
		MovementData movement = data.Movement;
		movement.Angle = floorAngle;
		movement.Velocity.Y = 0f;
		movement.Position.Y += floorDistance;
		logic.TileCollider.Position = (Vector2I)movement.Position;
		return true;
	}
	
    public (bool, int) CollideWallsAndCeiling(out Angles.Quadrant moveQuadrant)
	{
		var isWallCollided = false;
		int wallRadius = data.Collision.RadiusNormal.X + 1;
		moveQuadrant = Angles.GetQuadrant(Angles.GetRoundedVector(data.Movement.Velocity));

		logic.TileCollider.SetData((Vector2I)data.Movement.Position, data.Collision.TileLayer);
		
		if (moveQuadrant != Angles.Quadrant.Right)
		{
			isWallCollided |= CollideWalls(wallRadius, Constants.Direction.Negative);
		}
		
		if (moveQuadrant != Angles.Quadrant.Left)
		{
			isWallCollided |= CollideWalls(wallRadius, Constants.Direction.Positive);
		}

		if (moveQuadrant == Angles.Quadrant.Down) return (isWallCollided, wallRadius);
		
		int roofDistance = GetRoofDistance();
#if S3_PHYSICS || SK_PHYSICS
		if (moveQuadrant == Angles.Quadrant.Left && roofDistance <= -14)
		{
			// Perform right wall collision instead if moving mostly left and too far into the ceiling
			isWallCollided |= CollideWalls(wallRadius, Constants.Direction.Positive);
			return (isWallCollided, wallRadius);
		}
#endif
		CollideCeiling(roofDistance, moveQuadrant);

		return (isWallCollided, wallRadius);
	}

	private bool CollideWalls(int wallRadius, Constants.Direction direction)
	{
		var sing = (int)direction;
		int wallDistance = logic.TileCollider.FindDistance(sing * wallRadius, 0, false, direction);

		if (wallDistance >= 0) return false;

		MovementData movement = data.Movement;
		movement.Velocity.X = 0f;
		movement.Position.X += sing * wallDistance;
		logic.TileCollider.Position = (Vector2I)movement.Position;
		return true;
	}
	
	private void CollideCeiling(int roofDistance, Angles.Quadrant moveQuadrant)
	{
		if (roofDistance >= 0) return;
		
		MovementData movement = data.Movement;
		movement.Position.Y -= roofDistance;
		logic.TileCollider.Position = (Vector2I)movement.Position;
		if (movement.Velocity.Y < 0f || moveQuadrant == Angles.Quadrant.Up)
		{
			movement.Velocity.Y = 0f;
		}
	}
	
	private int GetRoofDistance()
	{
		Vector2I radius = data.Collision.Radius;
		return logic.TileCollider.FindClosestDistance(
			-radius.X, -radius.Y, radius.X, -radius.Y, true, Constants.Direction.Negative);
	}
}
