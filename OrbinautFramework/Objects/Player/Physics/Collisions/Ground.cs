using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;

namespace OrbinautFramework3.Objects.Player.Physics.Collisions;

public struct Ground
{
	private const int MinTolerance = 4;
	private const int MaxTolerance = 14;
	
	public void CollideWalls()
    {
        if (!IsGrounded) return;
		
		// Exit collision while on a left wall or a ceiling, unless angle is cardinal
		// and S3K physics are enabled
		if (Angle is > 90f and <= 270f && (SharedData.PhysicsType < PhysicsTypes.SK || Angle % 90f != 0f)) return;

		int wallRadius = RadiusNormal.X + 1;
		int offsetY = Angle == 0f ? 8 : 0;
		
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
				wallRadius = -wallRadius;
				break;
			
			default: return;
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
		float offset = wallDistance / Scene.Instance.ProcessSpeed;
		
		switch (quadrant)
		{
			case Angles.Quadrant.Down or Angles.Quadrant.Up:
				Velocity.Modify(new Vector2(-offset, 0f));
				GroundSpeed.Value = 0f;
				
				if (Facing == firstDirection && !IsSpinning)
				{
					SetPushAnimationBy = this;
				}
				break;
				
			case Angles.Quadrant.Right or Angles.Quadrant.Left:
				Velocity.Modify(new Vector2(0f, offset));
				break;
		}
    }
    
	// Each tile type has its own rules about how it should react to a specific tile check
	// Since we're going to rotate player's sensors, "rotate" tile properties as well
	public void CollideFloor()
	{
		if (!IsGrounded || OnObject != null) return;

		TileBehaviour = GetTileBehaviour();
		TileCollider.SetData((Vector2I)Position, TileLayer, TileBehaviour);

		(int distance, float angle) = FindTile();
		
		if (GoAirborne()) return;

		if (distance < -MaxTolerance) return;
		
		Position += TileBehaviour switch
		{
			Constants.TileBehaviours.Floor => new Vector2(0f, distance),
			Constants.TileBehaviours.RightWall => new Vector2(distance, 0f),
			Constants.TileBehaviours.Ceiling => new Vector2(0f, -distance),
			Constants.TileBehaviours.LeftWall => new Vector2(-distance, 0f),
			_ => throw new ArgumentOutOfRangeException()
		};
		
		Angle = SharedData.PhysicsType >= PhysicsTypes.S2 ? SnapFloorAngle(angle) : angle;
	}

	private Constants.TileBehaviours GetTileBehaviour() => Angle switch
	{
		<= 45 or >= 315 => Constants.TileBehaviours.Floor,
		> 45 and < 135 => Constants.TileBehaviours.RightWall,
		>= 135 and <= 225 => Constants.TileBehaviours.Ceiling,
		_ => Constants.TileBehaviours.LeftWall
	};

	private (int distance, float angle) FindTile() => TileBehaviour switch
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

	private bool GoAirborne(int distance)
	{
		if (IsStickToConvex) return false;
		
		float toleranceCheckSpeed = TileBehaviour switch
		{
			Constants.TileBehaviours.Floor or Constants.TileBehaviours.Ceiling => Velocity.X,
			Constants.TileBehaviours.RightWall or Constants.TileBehaviours.LeftWall => Velocity.Y,
			_ => throw new ArgumentOutOfRangeException(TileBehaviour.ToString())
		};
			
		float tolerance = SharedData.PhysicsType < PhysicsTypes.S2 ? 
			MaxTolerance : Math.Min(MinTolerance + Math.Abs(MathF.Floor(toleranceCheckSpeed)), MaxTolerance);

		if (distance <= tolerance) return false;
		
		SetPushAnimationBy = null;
		IsGrounded = false;
						
		OverrideAnimationFrame = 0;
		return true;
	}

	private float SnapFloorAngle(float floorAngle)
	{
		float difference = Math.Abs(Angle % 180f - floorAngle % 180f);
		return difference is < 45f or > 135f ? floorAngle : MathF.Round(Angle / 90f) % 4f * 90f;
	}
}