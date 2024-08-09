using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Physics;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

[FsmSourceGenerator.FsmState("Action")]
public struct Climb(PlayerData data)
{
	private enum ClimbStates : byte
	{
		Normal, Ledge, WallJump, Fall
	}
	
	private ClimbStates _state = ClimbStates.Normal;
	private float _animationValue;
	private const int ClimbAnimationFrameNumber = 6; // TODO: remove somehow? Or not...

	public void Perform()
    {
	    switch (_state)
	    {
		    case ClimbStates.Normal: ClimbNormal(); break;
		    case ClimbStates.Ledge: ClimbLedge(); break;
		    case ClimbStates.WallJump: ClimbJump(); break;
		    default: throw new ArgumentOutOfRangeException(_state.ToString());
	    }
    }

	private void ClimbNormal()
	{
		if (!Mathf.IsEqualApprox(data.PlayerNode.Position.X, data.PlayerNode.PreviousPosition.X) || 
		    data.Physics.Velocity.X != 0f)
		{
			ReleaseClimb();
			return;
		}
		
		const int stepsPerClimbFrame = 4;
		UpdateVerticalSpeedOnClimb(ClimbAnimationFrameNumber * stepsPerClimbFrame);
		
		int radiusX = data.Collision.Radius.X;
		if (data.Visual.Facing == Constants.Direction.Negative)
		{
			radiusX++;
		}
		
		data.TileCollider.SetData((Vector2I)data.PlayerNode.Position, data.Collision.TileLayer);

		if (data.Physics.Velocity.Y < 0 ? ClimbUpOntoWall(radiusX) : ReleaseClimbing(radiusX)) return;
		
		if (!data.Input.Press.Abc)
		{
			// Update animation frame if still climbing
			if (data.Physics.Velocity.Y != 0)
			{
				data.Visual.OverrideFrame = Mathf.FloorToInt(_animationValue / stepsPerClimbFrame);
			}
			return;
		}

		ClimbJump();
	}

	private void ClimbJump()
	{
		data.Visual.Animation = Animations.Spin;
		data.Visual.Facing = (Constants.Direction)(-(int)data.Visual.Facing);
		
		data.State = States.Default;
		
		data.Physics.IsJumping = true;
		data.Physics.IsSpinning = true;
		data.Physics.ResetGravity(data.Water.IsUnderwater);
		data.Physics.Velocity.Vector = new Vector2(
			3.5f * (float)data.Visual.Facing, 
			PhysicParams.MinimalJumpSpeed);
		
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
			data.Physics.Velocity.Y = 0f;
			data.Physics.Gravity = 0f;
			return true;
		}

		// If Knuckles has encountered a small dip in the wall, cancel climb movement
		if (wallDistance != 0)
		{
			data.Physics.Velocity.Y = 0f;
		}

		// If Knuckles has bumped into the ceiling, cancel climb movement and push him out
		int ceilDistance = data.TileCollider.FindDistance(
			radiusX * (int)data.Visual.Facing, 
			1 - data.Collision.RadiusNormal.Y, 
			true, 
			Constants.Direction.Negative);

		if (ceilDistance >= 0) return false;
		data.PlayerNode.Position -= new Vector2(0f, ceilDistance);
		data.Physics.Velocity.Y = 0f;
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
		
		data.PlayerNode.Position += 
			new Vector2(0f, distance + data.Collision.RadiusNormal.Y - data.Collision.Radius.Y);
		data.Rotation.Angle = angle;
				
		Land();

		data.Visual.Animation = Animations.Idle;
		data.Physics.Velocity.Y = 0f;
				
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

			data.Physics.Velocity.Y = -PhysicParams.AccelerationClimb;
			return;
		}
		
		if (data.Input.Down.Down)
		{
			_animationValue -= Scene.Instance.ProcessSpeed;
			if (_animationValue < 0f)
			{
				_animationValue = maxValue;
			}

			data.Physics.Velocity.Y = PhysicParams.AccelerationClimb;
			return;
		}

		data.Physics.Velocity.Y = 0f;
	}

	private void ReleaseClimb()
	{
		data.Visual.Animation = Animations.GlideFall;
		data.State = States.Glide;
		data.Collision.Radius = data.Collision.RadiusNormal;
		
		_state = ClimbStates.Fall;
		_animationValue = 1f;
		
		data.Physics.ResetGravity(data.Water.IsUnderwater);
	}

	private void ClimbLedge()
	{
		if (data.Visual.Animation != Animations.ClimbLedge)
		{
			data.Visual.Animation = Animations.ClimbLedge;
			data.PlayerNode.Position += new Vector2(3f * (float)data.Visual.Facing, -3f);
		}
		else if (data.PlayerNode.Sprite.IsFrameChanged)
		{
			switch (data.PlayerNode.Sprite.Frame)
			{
				case 1: data.PlayerNode.Position += new Vector2(8f * (float)data.Visual.Facing, -10f); break;
				case 2: data.PlayerNode.Position -= new Vector2(8f * (float)data.Visual.Facing, 12f); break;
			}
		}
		else if (data.PlayerNode.Sprite.IsFinished)
		{
			Land();
			data.Visual.Animation = Animations.Idle;
			data.PlayerNode.Position += new Vector2(8f * (float)data.Visual.Facing, 4f);

			// Subtract that 1px that was applied when we attached to the wall
			if (data.Visual.Facing == Constants.Direction.Negative)
			{
				data.PlayerNode.Position += Vector2.Left;
			}
		}
	}
}