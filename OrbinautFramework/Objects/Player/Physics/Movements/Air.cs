using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Physics.Movements;

public readonly struct Air(PlayerData data, IPlayerLogic logic)
{
	public void Move()
	{
		if (logic.Action is States.Climb or States.SpinDash or States.GlideAir or States.GlideFall) return;
		
		Rotate();
		LimitVerticalVelocity();
		
		if (logic.Action == States.HammerDash) return;
		
		MoveHorizontally();
		ApplyDrag();
	}

	private void Rotate()
	{
		if (Mathf.IsEqualApprox(data.Movement.Angle, 0f)) return;
		
		float speed = Angles.ByteStep * Scene.Instance.Speed;
		data.Movement.Angle += data.Movement.Angle >= Angles.CircleHalf ? speed : -speed;
		
		if (data.Movement.Angle is < 0f or >= Angles.CircleFull)
		{
			data.Movement.Angle = 0f;
		}
	}

	private void LimitVerticalVelocity()
	{
		if (!data.Movement.IsJumping && logic.Action != States.SpinDash && 
		    !data.Movement.IsForcedRoll && data.Movement.Velocity.Y < -15.75f)
		{
			data.Movement.Velocity.Y = -15.75f;
		}
		
#if CD_PHYSICS
		if (data.Movement.Velocity.Y > 16f)
		{
			data.Movement.Velocity.Y = 16f;
		}
#endif
	}

	private void MoveHorizontally()
	{
		if (data.Movement.IsAirLock) return;
		
		if (data.Input.Down.Left)
		{
			MoveTo(Constants.Direction.Negative);
		}
		else if (data.Input.Down.Right)
		{
			MoveTo(Constants.Direction.Positive);
		}
	}

	private void MoveTo(Constants.Direction direction)
	{
		var sign = (int)direction;
		float velocity = sign * data.Movement.Velocity.X;
		float acceleration = sign * data.Physics.AccelerationAir;
		
		if (velocity < 0f)
		{
			data.Movement.Velocity.SetAccelerationX(acceleration);
		}
		else if (!SharedData.NoSpeedCap || velocity < data.Physics.AccelerationTop)
		{
			data.Movement.Velocity.SetAccelerationX(acceleration);
			data.Movement.Velocity.LimitX(sign * data.Physics.AccelerationTop, direction);
		}
		
		data.Visual.Facing = direction;
	}

	private void ApplyDrag()
	{
		if (data.Movement.Velocity.Y is < 0f and > -4f)
		{
			data.Movement.Velocity.SetAccelerationX(MathF.Floor(data.Movement.Velocity.X * 8f) / -256f);
		}
	}
}
