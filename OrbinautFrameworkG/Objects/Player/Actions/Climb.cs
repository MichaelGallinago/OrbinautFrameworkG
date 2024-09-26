using Godot;
using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework.SceneModule;
using OrbinautFrameworkG.Framework.StaticStorages;
using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Logic;
using OrbinautFrameworkG.Objects.Player.Sprite;
using static OrbinautFrameworkG.Objects.Player.ActionFsm;

namespace OrbinautFrameworkG.Objects.Player.Actions;

[FsmSourceGenerator.FsmState("Action")]
public struct Climb(PlayerData data, IPlayerLogic logic)
{
	private const int StepsPerClimbFrame = 4;

	private float _animationValue = 0f;
	
	public States Perform()
	{
		MovementData movement = data.Movement;
	    if (!Mathf.IsEqualApprox(movement.Position.X, data.Node.PreviousPosition.X)) return Release(); 
	    if (movement.Velocity.X != 0f) return Release();
	    
	    UpdateVerticalVelocity(data.Sprite.FrameCount * StepsPerClimbFrame);
		
	    int radiusX = data.Collision.Radius.X;
	    if (data.Visual.Facing == Constants.Direction.Negative)
	    {
		    radiusX++;
	    }
		
	    logic.TileCollider.SetData((Vector2I)movement.Position, data.Collision.TileLayer);

	    States state = movement.Velocity.Y < 0 ? ClimbUpOntoWall(radiusX) : Release(radiusX);
	    if (state != States.Climb) return state;
		
	    if (data.Input.Press.Aby)
	    {
		    Jump();
		    return States.Jump;
	    }

	    UpdateAnimationFrame();
	    return States.Climb;
    }
	
	private void Jump()
	{
		data.ResetGravity();
		
		data.Visual.Facing = (Constants.Direction)(-(int)data.Visual.Facing);
		
		var velocity = new Vector2(3.5f * (float)data.Visual.Facing, data.Physics.MinimalJumpSpeed);
		data.Movement.Velocity = velocity;
		
		AudioPlayer.Sound.Play(SoundStorage.Jump);
	}
	
	private void UpdateAnimationFrame()
	{
		if (data.Movement.Velocity.Y == 0f) return;
		
		data.Visual.OverrideFrame = Mathf.FloorToInt(_animationValue / StepsPerClimbFrame);
	}
	
	private States ClimbUpOntoWall(int radiusX)
	{
		// If the wall is far away from Knuckles then he must have reached a ledge, make him climb up onto it
		int wallDistance = logic.TileCollider.FindDistance(
			radiusX * (int)data.Visual.Facing, 
			-data.Collision.Radius.Y - 1, 
			false, 
			data.Visual.Facing);
		
		if (wallDistance >= 4)
		{
			data.Movement.Velocity.Y = 0f;
			data.Movement.Gravity = 0f;
			return States.ClimbLedge;
		}

		// If Knuckles has encountered a small dip in the wall, cancel climb movement
		if (wallDistance != 0)
		{
			data.Movement.Velocity.Y = 0f;
		}

		CollideCeiling(radiusX);
		return States.Climb;
	}

	private void CollideCeiling(int radiusX)
	{
		// If Knuckles has bumped into the ceiling, cancel climb movement and push him out
		int ceilDistance = logic.TileCollider.FindDistance(
			radiusX * (int)data.Visual.Facing, 
			1 - data.Collision.RadiusNormal.Y, 
			true, 
			Constants.Direction.Negative);

		if (ceilDistance >= 0) return;
		data.Movement.Velocity.Y = 0f;
		data.Movement.Position.Y -= ceilDistance;
	}

	private States Release(int radiusX)
	{
		// If Knuckles is no longer against the wall, make him let go
		int wallDistance = logic.TileCollider.FindDistance(
			radiusX * (int)data.Visual.Facing,
			data.Collision.Radius.Y + 1,
			false,
			data.Visual.Facing);
		
		return wallDistance == 0 ? Land(radiusX) : Release();
	}

	private States Land(int radiusX)
	{
		(int distance, float angle) = logic.TileCollider.FindTile(
			radiusX * (int)data.Visual.Facing, 
			data.Collision.RadiusNormal.Y, 
			true, 
			Constants.Direction.Positive);
		
		if (distance >= 0) return States.Climb;
		
		data.Movement.Position.Y += distance + data.Collision.RadiusNormal.Y - data.Collision.Radius.Y;
		data.Movement.Angle = angle;
		
		logic.Land();

		data.Sprite.Animation = Animations.Idle;
		data.Movement.Velocity.Y = 0f;
				
		return States.Default;
	}

	private void UpdateVerticalVelocity(int maxValue)
	{
		if (data.Input.Down.Up)
		{
			_animationValue += Scene.Instance.Speed;
			if (_animationValue > maxValue)
			{
				_animationValue = 0f;
			}
			
			data.Movement.Velocity.Y = -data.Physics.AccelerationClimb;
			return;
		}
		
		if (data.Input.Down.Down)
		{
			_animationValue -= Scene.Instance.Speed;
			if (_animationValue < 0f)
			{
				_animationValue = maxValue;
			}

			data.Movement.Velocity.Y = data.Physics.AccelerationClimb;
			return;
		}

		data.Movement.Velocity.Y = 0f;
	}

	private States Release()
	{
		data.Visual.OverrideFrame = 1;
		return States.GlideFall;
	}
}
