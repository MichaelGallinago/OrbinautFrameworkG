using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Physics;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

public struct Climb() : IAction
{
	public PlayerData Data { private get; init; }
	
	public enum States : byte
	{
		Normal, Ledge, WallJump
	}
	
	private enum ClimbLedgeStates : byte
	{
		None, Frame0, Frame1, Frame2, End
	}
	
	private States _state = States.Normal;
	private float _step;
	private const int ClimbAnimationFrameNumber = 6; // TODO: remove somehow? Or not...

	public void Perform()
    {
	    switch (_state)
	    {
		    case States.Normal: ClimbNormal(); break;
		    case States.Ledge: ClimbLedge(); break;
		    case States.WallJump: ClimbJump(); break;
		    default: throw new ArgumentOutOfRangeException(_state.ToString());
	    }
    }

	private void ClimbNormal()
	{
		if (!Mathf.IsEqualApprox(Player.Position.X, PreviousPosition.X) || Velocity.X != 0f)
		{
			ReleaseClimb();
			return;
		}
		
		const int stepsPerClimbFrame = 4;
		UpdateVerticalSpeedOnClimb(ClimbAnimationFrameNumber * stepsPerClimbFrame);
		
		int radiusX = Player.CollisionBoxes.Radius.X;
		if (Player.Data.Facing == Constants.Direction.Negative)
		{
			radiusX++;
		}
		
		Player.Data.TileCollider.SetData((Vector2I)Player.Position, Player.Data.TileLayer);

		if (Player.PhysicsCore.Velocity.Y < 0 ? ClimbUpOntoWall(radiusX) : ReleaseClimbing(radiusX)) return;
		
		if (!Input.Press.Abc)
		{
			// Update animation frame if still climbing
			if (Velocity.Y != 0)
			{
				OverrideAnimationFrame = Mathf.FloorToInt(_step / stepsPerClimbFrame);
			}
			return;
		}

		ClimbJump();
	}

	private void ClimbJump()
	{
		Animation = Animations.Spin;
		IsSpinning = true;
		IsJumping = true;
		Action = Actions.None;
		Facing = (Constants.Direction)(-(int)Facing);
		Velocity.Vector = new Vector2(3.5f * (float)Facing, PhysicParams.MinimalJumpSpeed);
			
		AudioPlayer.Sound.Play(SoundStorage.Jump);
		ResetGravity();
	}

	private bool ClimbUpOntoWall(int radiusX)
	{
		// If the wall is far away from Knuckles then he must have reached a ledge, make him climb up onto it
		int wallDistance = TileCollider.FindDistance(radiusX * (int)Facing, -Radius.Y - 1, false, Facing);
		
		if (wallDistance >= 4)
		{
			_state = (int)States.Ledge;
			_step = 0f;
			Velocity.Y = 0f;
			Gravity = 0f;
			return true;
		}

		// If Knuckles has encountered a small dip in the wall, cancel climb movement
		if (wallDistance != 0)
		{
			Velocity.Y = 0f;
		}

		// If Knuckles has bumped into the ceiling, cancel climb movement and push him out
		int ceilDistance = TileCollider.FindDistance(
			radiusX * (int)Facing, 1 - RadiusNormal.Y, true, Constants.Direction.Negative);

		if (ceilDistance >= 0) return false;
		Position -= new Vector2(0f, ceilDistance);
		Velocity.Y = 0f;
		return false;
	}

	private bool ReleaseClimbing(int radiusX)
	{
		// If Knuckles is no longer against the wall, make him let go
		if (TileCollider.FindDistance(radiusX * (int)Facing, Radius.Y + 1, false, Facing) == 0)
		{
			return LandAfterClimbing(radiusX);
		}
		
		ReleaseClimb();
		return true;
	}

	private bool LandAfterClimbing(int radiusX)
	{
		(int distance, float angle) = TileCollider.FindTile(
			radiusX * (int)Facing, RadiusNormal.Y, true, Constants.Direction.Positive);

		if (distance >= 0) return false;
		Position += new Vector2(0f, distance + RadiusNormal.Y - Radius.Y);
		Angle = angle;
				
		Land();

		Animation = Animations.Idle;
		Velocity.Y = 0f;
				
		return true;
	}

	private void UpdateVerticalSpeedOnClimb(int maxValue)
	{
		if (Input.Down.Up)
		{
			_step += Scene.Instance.ProcessSpeed;
			if (_step > maxValue)
			{
				_step = 0f;
			}

			Velocity.Y = -PhysicParams.AccelerationClimb;
			return;
		}
		
		if (Input.Down.Down)
		{
			_step -= Scene.Instance.ProcessSpeed;
			if (_step < 0f)
			{
				_step = maxValue;
			}

			Velocity.Y = PhysicParams.AccelerationClimb;
			return;
		}

		Velocity.Y = 0f;
	}

	private void ReleaseClimb()
	{
		Animation = Animations.GlideFall;
		Action = Actions.Glide;
		_state = (int)GlideStates.Fall;
		_step = 1f;
		Radius = RadiusNormal;
		
		ResetGravity();
	}

	private void ClimbLedge()
	{
		//TODO: check this
		
		ClimbLedgeStates previousState = GetClimbLedgeState(_step);
		_step += Scene.Instance.ProcessSpeed;
		ClimbLedgeStates state = GetClimbLedgeState(_step);
		if (state == previousState) return;
		
		switch (state)
		{
			case ClimbLedgeStates.Frame0:
				Animation = Animations.ClimbLedge;
				Position += new Vector2(3f * (float)Facing, -3f);
				break;
					
			case ClimbLedgeStates.Frame1:
				Position += new Vector2(8f * (float)Facing, -10f);
				break;
					
			case ClimbLedgeStates.Frame2:
				Position -= new Vector2(8f * (float)Facing, 12f);
				break;
					
			case ClimbLedgeStates.End:
				Land();
				Animation = Animations.Idle;
				Position += new Vector2(8f * (float)Facing, 4f);

				// Subtract that 1px that was applied when we attached to the wall
				if (Facing == Constants.Direction.Negative)
				{
					Position += Vector2.Left;
				}
				break;
		}
	}
	
	private static ClimbLedgeStates GetClimbLedgeState(float value) => value switch
	{
		<= 0f => ClimbLedgeStates.None,
		<= 6f => ClimbLedgeStates.Frame0,
		<= 12f => ClimbLedgeStates.Frame1,
		<= 18f => ClimbLedgeStates.Frame2,
		_ => ClimbLedgeStates.End
	};
}