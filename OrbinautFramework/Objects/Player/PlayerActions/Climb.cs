using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

[FsmSourceGenerator.FsmState("Action")]
public struct Climb(PlayerData data)
{
	private const int StepsPerClimbFrame = 4;
	private const int ClimbAnimationFrameNumber = 6; // TODO: remove somehow? Or not...
	
	private float _animationValue;

	public void Enter()
	{
		
	}

	public void Perform()
    {
	    if (!Mathf.IsEqualApprox(data.Node.Position.X, data.Node.PreviousPosition.X) || 
	        data.Movement.Velocity.X != 0f)
	    {
		    ReleaseClimb();
		    return;
	    }
		
	    UpdateVerticalSpeedOnClimb(ClimbAnimationFrameNumber * StepsPerClimbFrame);
		
	    int radiusX = data.Collision.Radius.X;
	    if (data.Visual.Facing == Constants.Direction.Negative)
	    {
		    radiusX++;
	    }
		
	    data.TileCollider.SetData((Vector2I)data.Node.Position, data.Collision.TileLayer);

	    if (data.Movement.Velocity.Y < 0 ? ClimbUpOntoWall(radiusX) : ReleaseClimbing(radiusX)) return;
		
	    if (!data.Input.Press.Abc)
	    {
		    UpdateAnimationFrame();
		    return;
	    }

	    ClimbJump();
    }

	private void UpdateAnimationFrame()
	{
		if (data.Movement.Velocity.Y != 0)
		{
			data.Visual.OverrideFrame = Mathf.FloorToInt(_animationValue / StepsPerClimbFrame);
		}
	}

	private void ClimbJump()
	{
		data.State = States.Jump;
		
		data.ResetGravity();
		
		data.Visual.Facing = (Constants.Direction)(-(int)data.Visual.Facing);
		
		var velocity = new Vector2(3.5f * (float)data.Visual.Facing, data.Physics.MinimalJumpSpeed);
		data.Movement.Velocity.Vector = velocity;
		
		AudioPlayer.Sound.Play(SoundStorage.Jump);
	}

	private bool ClimbUpOntoWall(int radiusX)
	{
		// If the wall is far away from Knuckles then he must have reached a ledge, make him climb up onto it
		int wallDistance = data.TileCollider.FindDistance(
			radiusX * (int)data.Visual.Facing, 
			-data.Collision.Radius.Y - 1, 
			false, 
			data.Visual.Facing);
		
		if (wallDistance >= 4)
		{
			_state = ClimbStates.Ledge;
			_animationValue = 0f;
			data.Movement.Velocity.Y = 0f;
			data.Movement.Gravity = 0f;
			return true;
		}

		// If Knuckles has encountered a small dip in the wall, cancel climb movement
		if (wallDistance != 0)
		{
			data.Movement.Velocity.Y = 0f;
		}

		// If Knuckles has bumped into the ceiling, cancel climb movement and push him out
		int ceilDistance = data.TileCollider.FindDistance(
			radiusX * (int)data.Visual.Facing, 
			1 - data.Collision.RadiusNormal.Y, 
			true, 
			Constants.Direction.Negative);

		if (ceilDistance >= 0) return false;
		data.Node.Position -= new Vector2(0f, ceilDistance);
		data.Movement.Velocity.Y = 0f;
		return false;
	}

	private bool ReleaseClimbing(int radiusX)
	{
		// If Knuckles is no longer against the wall, make him let go
		if (data.TileCollider.FindDistance(
			    radiusX * (int)data.Visual.Facing, 
			    data.Collision.Radius.Y + 1, 
			    false, 
			    data.Visual.Facing) == 0)
		{
			return LandAfterClimbing(radiusX);
		}
		
		ReleaseClimb();
		return true;
	}

	private bool LandAfterClimbing(int radiusX)
	{
		(int distance, float angle) = data.TileCollider.FindTile(
			radiusX * (int)data.Visual.Facing, 
			data.Collision.RadiusNormal.Y, 
			true, 
			Constants.Direction.Positive);
		
		if (distance >= 0) return false;
		
		var position = new Vector2(0f, distance + data.Collision.RadiusNormal.Y - data.Collision.Radius.Y);
		data.Node.Position += position;
		data.Movement.Angle = angle;
				
		Land();

		data.Visual.Animation = Animations.Idle;
		data.Movement.Velocity.Y = 0f;
				
		return true;
	}

	private void UpdateVerticalSpeedOnClimb(int maxValue)
	{
		if (data.Input.Down.Up)
		{
			_animationValue += Scene.Instance.ProcessSpeed;
			if (_animationValue > maxValue)
			{
				_animationValue = 0f;
			}

			data.Movement.Velocity.Y = -data.Physics.AccelerationClimb;
			return;
		}
		
		if (data.Input.Down.Down)
		{
			_animationValue -= Scene.Instance.ProcessSpeed;
			if (_animationValue < 0f)
			{
				_animationValue = maxValue;
			}

			data.Movement.Velocity.Y = data.Physics.AccelerationClimb;
			return;
		}

		data.Movement.Velocity.Y = 0f;
	}

	private void ReleaseClimb()
	{
		data.State = States.GlideFall;
		data.Visual.OverrideFrame = 1;
	}
}