using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Modules;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

//TODO: check speed
[FsmSourceGenerator.FsmState("Action")]
public struct Glide(PlayerData data)
{
	public enum GlideStates : byte
	{
		Air, Ground, Fall
	}

	private GlideStates _glideState;
	private float _glideAngle;
	private float _glideValue;
	// data.Physics.GroundSpeed - glide speed
	
	public void Perform()
    {
	    if (_glideState == GlideStates.Fall) return;
		
	    switch (_glideState)
	    {
		    case GlideStates.Air: GlideAir(); break;
		    case GlideStates.Ground: GlideGround(); break;
	    }
    }

	private void GlideAir()
	{
		UpdateSpeed();
		TurnAroundAir();
		UpdateGravityAndHorizontalVelocity();
		UpdateAirAnimationFrame();
		
		if (data.Input.Down.Abc) return;

		Release();
		data.Movement.Velocity.X *= 0.25f;
	}

	private void UpdateSpeed()
	{
		const float glideAcceleration = 0.03125f;
		
		if (data.Movement.GroundSpeed < 4f)
		{
			data.Movement.GroundSpeed.Acceleration = glideAcceleration;
			return;
		}

		if (_glideAngle % 180f != 0f) return;
		data.Movement.GroundSpeed.Acceleration = data.Physics.AccelerationGlide;
		data.Movement.GroundSpeed.SetMin(24f);
	}

	private void UpdateGravityAndHorizontalVelocity()
	{
		const float glideGravity = 0.125f;

		data.Movement.Velocity.X = data.Movement.GroundSpeed * -MathF.Cos(Mathf.DegToRad(_glideAngle));
		data.Movement.Gravity = data.Movement.Velocity.Y < 0.5f ? glideGravity : -glideGravity;
	}

	private void UpdateAirAnimationFrame()
	{
		float angle = Math.Abs(_glideAngle) % 180f;
		switch (angle)
		{
			case < 30f or > 150f:
				data.Visual.OverrideFrame = 0;
				break;
			case < 60f or > 120f:
				data.Visual.OverrideFrame = 1;
				break;
			default:
				data.Visual.Facing = angle < 90 ? Constants.Direction.Negative : Constants.Direction.Positive;
				data.Visual.OverrideFrame = 2;
				break;
		}
	}

	private void GlideGround()
	{
		UpdateGroundVelocityX();
		
		// Stop sliding
		if (data.Movement.Velocity.X == 0f)
		{
			Land();
			data.Visual.OverrideFrame = 1;
			
			data.Visual.Animation = Animations.GlideGround;
			data.Movement.GroundLockTimer = 16f;
			data.Movement.GroundSpeed.Value = 0f;
			
			return;
		}

		// Spawn dust particles
		if (_glideValue % 4f < Scene.Instance.ProcessSpeed)
		{
			//TODO: obj_dust_skid
			//instance_create(x, y + data.Collision.Radius.Y, obj_dust_skid);
		}
				
		if (_glideValue > 0f && _glideValue % 8f < Scene.Instance.ProcessSpeed)
		{
			AudioPlayer.Sound.Play(SoundStorage.Slide);
		}
					
		_glideValue += Scene.Instance.ProcessSpeed;
	}

	private void UpdateGroundVelocityX()
	{
		if (!data.Input.Down.Abc)
		{
			data.Movement.Velocity.X = 0f;
			return;
		}
		
		const float slideFriction = -0.09375f;
		
		float speedX = data.Movement.Velocity.X;
		data.Movement.Velocity.AccelerationX = Math.Sign(data.Movement.Velocity.X) * slideFriction;
		switch (speedX)
		{
			case > 0f: data.Movement.Velocity.MaxX(0f); break;
			case < 0f: data.Movement.Velocity.MinX(0f); break;
		}
	}

	private void TurnAroundAir()
	{
		float speed = Angles.ByteAngleStep * Scene.Instance.ProcessSpeed;
		if (data.Input.Down.Left && !Mathf.IsZeroApprox(_glideAngle))
		{
			if (_glideAngle > 0f)
			{
				_glideAngle = -_glideAngle;
			}
			
			_glideAngle += speed;
			
			if (_glideAngle < 0f)
			{
				_glideAngle = 0f;
			}
			return;
		}
		
		if (data.Input.Down.Right && !Mathf.IsEqualApprox(_glideAngle, 180f))
		{
			if (_glideAngle < 0f)
			{
				_glideAngle = -_glideAngle;
			}
			
			_glideAngle += speed;

			if (_glideAngle > 180f)
			{
				_glideAngle = 180f;
			}
			return;
		}
		
		if (Mathf.IsZeroApprox(_glideAngle % 180f)) return;
		_glideAngle += speed;
	}
	
	public void LatePerform()
	{
		var climbY = (int)data.PlayerNode.Position.Y;
		var collisionFlagWall = false;
		int wallRadius = data.Collision.RadiusNormal.X + 1;
		Angles.Quadrant moveQuadrant = Angles.GetQuadrant(Angles.GetVector256(data.Movement.Velocity));

		data.TileCollider.SetData((Vector2I)data.PlayerNode.Position, data.Collision.TileLayer);
		
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
		int wallDistance = data.TileCollider.FindDistance(sing * wallRadius, 0, false, direction);

		if (wallDistance >= 0) return false;
		
		data.PlayerNode.Position += new Vector2(sing * wallDistance, 0f);
		data.TileCollider.Position = (Vector2I)data.PlayerNode.Position;
		data.Movement.Velocity.X = 0f;
		return true;
	}

	private bool CollideCeiling(int wallRadius, Angles.Quadrant moveQuadrant)
	{
		if (moveQuadrant == Angles.Quadrant.Down) return false;

		Vector2I radius = data.Collision.Radius;
		int roofDistance = data.TileCollider.FindClosestDistance(
			-radius.X, -radius.Y, radius.X, -radius.Y, true, Constants.Direction.Negative);
			
		if (moveQuadrant == Angles.Quadrant.Left && roofDistance <= -14 && 
		    SharedData.PhysicsType >= PhysicsCore.Types.S3)
		{
			// Perform right wall collision instead if moving mostly left and too far into the ceiling
			return CollideWalls(wallRadius, Constants.Direction.Positive);
		}

		if (roofDistance >= 0) return false;
		
		data.PlayerNode.Position -= new Vector2(0f, roofDistance);
		data.TileCollider.Position = (Vector2I)data.PlayerNode.Position;
		if (data.Movement.Velocity.Y < 0f || moveQuadrant == Angles.Quadrant.Up)
		{
			data.Movement.Velocity.Y = 0f;
		}
		return false;
	}

	private bool CollideFloor()
	{
		(int floorDistance, float floorAngle) = data.TileCollider.FindClosestTile(
			-data.Collision.Radius.X, data.Collision.Radius.Y, data.Collision.Radius.X, data.Collision.Radius.Y,
			true, Constants.Direction.Positive);
	
		if (_glideState == GlideStates.Ground)
		{
			if (floorDistance > 14)
			{
				Release();
				return false;
			}
			
			data.PlayerNode.Position += new Vector2(0f, floorDistance);
			data.Movement.Angle = floorAngle;
			return false;
		}

		if (floorDistance >= 0) return false;
		
		data.PlayerNode.Position += new Vector2(0f, floorDistance);
		data.TileCollider.Position = (Vector2I)data.PlayerNode.Position;
		data.Movement.Angle = floorAngle;
		data.Movement.Velocity.Y = 0f;
		return true;
	}

	private void LandWhenGlide()
	{
		switch (_glideState)
		{
			case GlideStates.Air: LandAir(); break;
			case GlideStates.Fall: LandFall(); break;
		}
	}

	private void LandAir()
	{
		if (Angles.GetQuadrant(data.Movement.Angle) != Angles.Quadrant.Down)
		{
			data.Movement.GroundSpeed.Value = data.Movement.Angle < 180 ? 
				data.Movement.Velocity.X : -data.Movement.Velocity.X;
			
			Land();
			return;
		}
				
		data.Visual.Animation = Animations.GlideGround;
		_glideState = GlideStates.Ground;
		_glideAngle = 0f;
		data.Movement.Gravity = 0f;
	}

	private void LandFall()
	{
		AudioPlayer.Sound.Play(SoundStorage.Land);
		Land();		
		
		if (Angles.GetQuadrant(data.Movement.Angle) != Angles.Quadrant.Down)
		{
			data.Movement.GroundSpeed.Value = data.Movement.Velocity.X;
			return;
		}
					
		data.Visual.Animation = Animations.GlideLand;
		data.Movement.GroundLockTimer = 16f;
		data.Movement.GroundSpeed.Value = 0f;
		data.Movement.Velocity.X = 0f;
	}

	private void AttachToWall(int wallRadius, int climbY)
	{
		if (_glideState != (int)GlideStates.Air) return;

		CheckCollisionOnAttaching(wallRadius, climbY);
			
		if (data.Visual.Facing == Constants.Direction.Negative)
		{
			data.PlayerNode.Position += Vector2.Right;
		}

		bool isWallJump = SharedData.SuperstarsTweaks && (data.Input.Down.Up || data.Input.Down.Down);
		data.State = States.Climb;
		_climbState = (int)(isWallJump ? ClimbStates.WallJump : ClimbStates.Normal); //TODO: set state inside Climb
		data.Visual.Animation = Animations.ClimbWall;
		_glideValue = 0f;
		data.Movement.GroundSpeed.Value = 0f;
		data.Movement.Velocity.Y = 0f;
		data.Movement.Gravity = 0f;
			
		AudioPlayer.Sound.Play(SoundStorage.Grab);
	}

	private void CheckCollisionOnAttaching(int wallRadius, int climbY)
	{
		// Cast a horizontal sensor just above Knuckles. If the distance returned is not 0, he
		// is either inside the ceiling or above the floor edge
		data.TileCollider.Position = data.TileCollider.Position with { Y = climbY - data.Collision.Radius.Y };
		
		if (data.TileCollider.FindDistance(
			    wallRadius * (int)data.Visual.Facing, 0, false, data.Visual.Facing) == 0) return;
		
		// The game casts a vertical sensor now in front of Knuckles, facing downwards. If the distance
		// returned is negative, Knuckles is inside the ceiling, else he is above the edge
			
		// Note that tile behaviour here is set to Constants.TileBehaviours.Ceiling.
		// LBR tiles are not ignored in this case
		data.TileCollider.TileBehaviour = Constants.TileBehaviours.Ceiling;
		int floorDistance = data.TileCollider.FindDistance(
			(wallRadius + 1) * (int)data.Visual.Facing, -1, true, Constants.Direction.Positive);
				
		if (floorDistance is < 0 or >= 12)
		{
			Release();
			return;
		}
		
		// Adjust Knuckles' y-position to place him just below the edge
		data.PlayerNode.Position += new Vector2(0f, floorDistance);
	}

	private void Release()
	{
		data.Visual.Animation = Animations.GlideFall;
		_glideState = GlideStates.Fall;
		_glideAngle = 0f;
		data.Collision.Radius = data.Collision.RadiusNormal;
		
		data.ResetGravity();
	}
}
