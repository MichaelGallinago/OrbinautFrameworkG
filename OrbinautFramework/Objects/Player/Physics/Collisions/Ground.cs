using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using static OrbinautFramework3.Framework.Constants;

namespace OrbinautFramework3.Objects.Player.Physics.Collisions;

public readonly struct Ground(PlayerData data, IPlayerLogic logic)
{
	private const int MinTolerance = 4;
	private const int MaxTolerance = 14;
	
	public void CollideWalls()
    {
	    MovementData movement = data.Movement;
	    float angle = movement.Angle;
#if SK_PHYSICS
		// Exit collision while on a left wall or a ceiling, unless angle is cardinal
	    if (angle is > 90f and <= 270f && angle % 90f != 0f) return;
#else
	    // Exit collision while on a left wall or a ceiling
	    if (angle is > 90f and <= 270f) return;
#endif

		int wallRadius = data.Collision.RadiusNormal.X + 1;
		int offsetY = angle == 0f ? 8 : 0;
		
		int sign;
		Direction firstDirection, secondDirection;
		switch ((float)movement.GroundSpeed)
		{
			case < 0f:
				sign = (int)Direction.Positive;
				firstDirection = Direction.Negative;
				secondDirection = Direction.Positive;
				break;
			
			case > 0f:
				sign = (int)Direction.Negative;
				firstDirection = Direction.Positive;
				secondDirection = Direction.Negative;
				wallRadius = -wallRadius;
				break;
			
			default: return;
		}

		TileCollider tileCollider = logic.TileCollider;
		var position = (Vector2I)(movement.Position + movement.Velocity.Acceleration);
		tileCollider.SetData(position, data.Collision.TileLayer, data.Collision.TileBehaviour);
		
		int wallDistance = GetWallCastQuadrant(angle) switch
		{
			0 => tileCollider.FindDistance(-wallRadius, offsetY, false, firstDirection),
			1 => tileCollider.FindDistance(0, wallRadius, true, secondDirection),
			2 => tileCollider.FindDistance(wallRadius, 0, false, secondDirection),
			3 => tileCollider.FindDistance(0, -wallRadius, true, firstDirection),
			_ => throw new ArgumentOutOfRangeException()
		};
		
		if (wallDistance >= 0) return;
		
		Angles.Quadrant quadrant = Angles.GetQuadrant(angle);
		wallDistance *= quadrant > Angles.Quadrant.Right ? -sign : sign;
		float offset = wallDistance / Scene.Instance.Speed;
		
		switch (quadrant)
		{
			case Angles.Quadrant.Down or Angles.Quadrant.Up:
				movement.Velocity.Modify(new Vector2(-offset, 0f));
				movement.GroundSpeed = 0f;
				
				if (data.Visual.Facing == firstDirection && !movement.IsSpinning)
				{
					data.Visual.SetPushBy = data.Node;
				}
				break;
				
			case Angles.Quadrant.Right or Angles.Quadrant.Left:
				movement.Velocity.Modify(new Vector2(0f, offset));
				break;
		}
    }

	private static int GetWallCastQuadrant(float angle) => angle switch
	{
		>= 45f and <= 128f => 1,
		> 128f and < 225f => 2,
		>= 225f and < 315f => 3,
		_ => 0
	};
    
	// Each tile type has its own rules about how it should react to a specific tile check
	// Since we're going to rotate player's sensors, "rotate" tile properties as well
	public void CollideFloor()
	{
		CollisionData collision = data.Collision;
		if (collision.OnObject != null) return;
		
		collision.TileBehaviour = GetTileBehaviour();
		logic.TileCollider.SetData(
			(Vector2I)data.Movement.Position,
			collision.TileLayer,
			collision.TileBehaviour);
		
		(int distance, float angle) = FindTile();
		
		if (GoAirborne(distance)) return;
		if (distance < -MaxTolerance) return;

		MovementData movement = data.Movement;
		movement.Position += collision.TileBehaviour switch
		{
			TileBehaviours.Floor => new Vector2(0f, distance),
			TileBehaviours.RightWall => new Vector2(distance, 0f),
			TileBehaviours.Ceiling => new Vector2(0f, -distance),
			TileBehaviours.LeftWall => new Vector2(-distance, 0f),
			_ => throw new ArgumentOutOfRangeException()
		};

#if S3_PHYSICS || SK_PHYSICS
		movement.Angle = SnapFloorAngle(angle);
#else
		movement.Angle = angle;
#endif
	}

	private TileBehaviours GetTileBehaviour() => data.Movement.Angle switch
	{
		<= 45 or >= 315 => TileBehaviours.Floor,
		> 45 and < 135 => TileBehaviours.RightWall,
		>= 135 and <= 225 => TileBehaviours.Ceiling,
		_ => TileBehaviours.LeftWall
	};

	private (int distance, float angle) FindTile()
	{
		Vector2I radius = data.Collision.Radius;
		return data.Collision.TileBehaviour switch
		{
			TileBehaviours.Floor => logic.TileCollider.FindClosestTile(
				-radius.X, radius.Y, radius.X, radius.Y, true, Direction.Positive),

			TileBehaviours.RightWall => logic.TileCollider.FindClosestTile(
				radius.Y, radius.X, radius.Y, -radius.X, false, Direction.Positive),

			TileBehaviours.Ceiling => logic.TileCollider.FindClosestTile(
				radius.X, -radius.Y, -radius.X, -radius.Y, true, Direction.Negative),

			TileBehaviours.LeftWall => logic.TileCollider.FindClosestTile(
				-radius.Y, -radius.X, -radius.Y, radius.X, false, Direction.Negative),

			_ => throw new ArgumentOutOfRangeException()
		};
	}

	private bool GoAirborne(int distance)
	{
		if (data.Collision.IsStickToConvex) return false;
		
		float toleranceCheckSpeed = data.Collision.TileBehaviour switch
		{
			TileBehaviours.Floor or TileBehaviours.Ceiling => data.Movement.Velocity.X,
			TileBehaviours.RightWall or TileBehaviours.LeftWall => data.Movement.Velocity.Y,
			_ => throw new ArgumentOutOfRangeException(data.Collision.TileBehaviour.ToString())
		};

#if S1_PHYSICS || CD_PHYSICS
		if (distance <= MaxTolerance) return false;
#else
		float tolerance = Math.Min(MinTolerance + Math.Abs(MathF.Floor(toleranceCheckSpeed)), MaxTolerance);
		if (distance <= tolerance) return false;
#endif
		data.Visual.SetPushBy = null;
		data.Visual.OverrideFrame = 0;
		data.Movement.IsGrounded = false;
		
		return true;
	}
	
#if S3_PHYSICS || SK_PHYSICS
	private float SnapFloorAngle(float floorAngle)
	{
		float difference = Math.Abs(data.Movement.Angle % 180f - floorAngle % 180f);
		return difference is < 45f or > 135f ? floorAngle : MathF.Round(data.Movement.Angle / 90f) % 4f * 90f;
	}
#endif
}
