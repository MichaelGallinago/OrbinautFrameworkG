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
        if (!data.Movement.IsGrounded) return;
        
#if SK_PHYSICS
		// Exit collision while on a left wall or a ceiling, unless angle is cardinal
	    if (data.Movement.Angle is > 90f and <= 270f && data.Movement.Angle % 90f != 0f) return;
#else
	    // Exit collision while on a left wall or a ceiling
	    if (data.Movement.Angle is > 90f and <= 270f) return;
#endif

		int wallRadius = data.Collision.RadiusNormal.X + 1;
		int offsetY = data.Movement.Angle == 0f ? 8 : 0;
		
		int sign;
		Direction firstDirection, secondDirection;
		switch (data.Movement.GroundSpeed.Value)
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
		
		logic.TileCollider.SetData(
			(Vector2I)data.Movement.Velocity.CalculateNewPosition(data.Node.Position), 
			data.Collision.TileLayer,
			data.Collision.TileBehaviour);
		
		int castQuadrant = data.Movement.Angle switch
		{
			>= 45f and <= 128f => 1,
			> 128f and < 225f => 2,
			>= 225f and < 315f => 3,
			_ => 0
		};
		
		int wallDistance = castQuadrant switch
		{
			0 => logic.TileCollider.FindDistance(-wallRadius, offsetY, false, firstDirection),
			1 => logic.TileCollider.FindDistance(0, wallRadius, true, secondDirection),
			2 => logic.TileCollider.FindDistance(wallRadius, 0, false, secondDirection),
			3 => logic.TileCollider.FindDistance(0, -wallRadius, true, firstDirection),
			_ => throw new ArgumentOutOfRangeException()
		};
		
		if (wallDistance >= 0) return;
		
		Angles.Quadrant quadrant = Angles.GetQuadrant(data.Movement.Angle);
		wallDistance *= quadrant > Angles.Quadrant.Right ? -sign : sign;
		float offset = wallDistance / Scene.Instance.Speed;
		
		switch (quadrant)
		{
			case Angles.Quadrant.Down or Angles.Quadrant.Up:
				data.Movement.Velocity.Modify(new Vector2(-offset, 0f));
				data.Movement.GroundSpeed.Value = 0f;
				
				if (data.Visual.Facing == firstDirection && !data.Movement.IsSpinning)
				{
					data.Visual.SetPushBy = data.Node;
				}
				break;
				
			case Angles.Quadrant.Right or Angles.Quadrant.Left:
				data.Movement.Velocity.Modify(new Vector2(0f, offset));
				break;
		}
    }
    
	// Each tile type has its own rules about how it should react to a specific tile check
	// Since we're going to rotate player's sensors, "rotate" tile properties as well
	public void CollideFloor()
	{
		if (!data.Movement.IsGrounded || data.Collision.OnObject != null) return;
		
		data.Collision.TileBehaviour = GetTileBehaviour();
		logic.TileCollider.SetData(
			(Vector2I)data.Node.Position,
			data.Collision.TileLayer,
			data.Collision.TileBehaviour);
		
		(int distance, float angle) = FindTile();
		
		if (GoAirborne(distance)) return;
		if (distance < -MaxTolerance) return;
		
		data.Node.Position += data.Collision.TileBehaviour switch
		{
			TileBehaviours.Floor => new Vector2(0f, distance),
			TileBehaviours.RightWall => new Vector2(distance, 0f),
			TileBehaviours.Ceiling => new Vector2(0f, -distance),
			TileBehaviours.LeftWall => new Vector2(-distance, 0f),
			_ => throw new ArgumentOutOfRangeException()
		};

#if S3_PHYSICS || SK_PHYSICS
		data.Movement.Angle = SnapFloorAngle(angle);
#else
		data.Movement.Angle = angle;
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
		data.Movement.IsGrounded = false;
						
		data.Visual.OverrideFrame = 0;
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
