using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Sprite;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

//TODO: check speed
[FsmSourceGenerator.FsmState("Action")]
public struct GlideAir(PlayerData data, IPlayerLogic logic)
{
	private readonly GlideCollisionLogic _collision = new(data, logic);
	
	private float _glideAngle = data.Visual.Facing == Constants.Direction.Negative ? 0f : 180f;
	//TODO: data.Physics.GroundSpeed - glide speed

	public void Enter()
	{
		data.Sprite.Animation = Animations.GlideAir;
		data.Collision.Radius = new Vector2I(10, 10);

		MovementData movement = data.Movement;
		movement.Velocity.X = 0f;
		movement.Velocity.Y += 2f;
		movement.IsAirLock = false;
		movement.IsSpinning = false;
		movement.GroundSpeed.Value = 4f;
		
		if (movement.Velocity.Y < 0f)
		{
			movement.Velocity.Y = 0f;
		}
	}
	
	public States Perform()
	{
		UpdateSpeed();
		TurnAroundAir();
		UpdateGravityAndHorizontalVelocity();
		UpdateAirAnimationFrame();
		
		if (data.Input.Down.Abc) return States.GlideAir;
		
		data.Movement.Velocity.X *= 0.25f;
		return States.GlideFall;
	}

	private void UpdateSpeed()
	{
		AcceleratedValue groundSpeed = data.Movement.GroundSpeed;
		if (groundSpeed < 4f)
		{
			const float glideAcceleration = 0.03125f;
			groundSpeed.Acceleration = glideAcceleration;
			return;
		}

		if (_glideAngle % 180f != 0f) return;
		groundSpeed.Acceleration = data.Physics.AccelerationGlide;
		groundSpeed.SetMin(24f);
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
	
	public States LatePerform()
	{
		(bool isWallCollided, int wallRadius) = _collision.CollideWallsAndCeiling(out Angles.Quadrant moveQuadrant);
		
		if (moveQuadrant != Angles.Quadrant.Up && _collision.CollideFloor())
		{
			Land();
			return States.GlideGround;
		}
		
		return isWallCollided ? AttachToWall(wallRadius, (int)data.Node.Position.Y) : States.Default;
	}
	
	private void Land()
	{
		MovementData movement = data.Movement;
		if (Angles.GetQuadrant(movement.Angle) != Angles.Quadrant.Down)
		{
			movement.GroundSpeed.Value = movement.Angle < 180 ? movement.Velocity.X : -movement.Velocity.X;
			
			logic.Land();
			return;
		}
				
		data.Sprite.Animation = Animations.GlideGround;
		movement.Gravity = 0f;
	}

	private States AttachToWall(int wallRadius, int climbY)
	{
		if (CheckCollisionOnAttaching(wallRadius, climbY)) return States.GlideFall;
			
		if (data.Visual.Facing == Constants.Direction.Negative)
		{
			data.Node.Position += Vector2.Right;
		}

		bool isWallJump = SharedData.SuperstarsTweaks && (data.Input.Down.Up || data.Input.Down.Down);
		
		data.Sprite.Animation = Animations.ClimbWall;
		data.Movement.GroundSpeed.Value = 0f;
		data.Movement.Velocity.Y = 0f;
		data.Movement.Gravity = 0f;
			
		AudioPlayer.Sound.Play(SoundStorage.Grab);
		
		return isWallJump ? States.ClimbWallJump : States.Climb;
	}
	
	private bool CheckCollisionOnAttaching(int wallRadius, int climbY)
	{
		// Cast a horizontal sensor just above Knuckles. If the distance returned is not 0, he
		// is either inside the ceiling or above the floor edge
		logic.TileCollider.Position = logic.TileCollider.Position with { Y = climbY - data.Collision.Radius.Y };

		Constants.Direction facing = data.Visual.Facing;
		if (logic.TileCollider.FindDistance(wallRadius * (int)facing, 0, false, facing) == 0) return false;
		
		// The game casts a vertical sensor now in front of Knuckles, facing downwards. If the distance
		// returned is negative, Knuckles is inside the ceiling, else he is above the edge
			
		// Note that tile behaviour here is set to Constants.TileBehaviours.Ceiling.
		// LBR tiles are not ignored in this case
		logic.TileCollider.TileBehaviour = Constants.TileBehaviours.Ceiling;
		int floorDistance = logic.TileCollider.FindDistance(
			(wallRadius + 1) * (int)facing, -1, true, Constants.Direction.Positive);

		if (floorDistance is < 0 or >= 12) return true;

		// Adjust Knuckles' y-position to place him just below the edge
		data.Node.Position += new Vector2(0f, floorDistance);
		return false;
	}
}
