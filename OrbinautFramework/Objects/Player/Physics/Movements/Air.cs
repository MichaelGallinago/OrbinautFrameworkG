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
		if (data.Movement.IsGrounded || data.Death.IsDead) return;
		if (logic.Action is States.Carried or States.Climb or 
		    States.SpinDash or States.GlideAir or States.GlideFall) return;

		RotateInAir();
		LimitVerticalVelocity();
		if (ChangeHammerDashFacingInAir()) return;
		MoveInAirHorizontally();
		ApplyAirDrag();
	}

	private void RotateInAir()
	{
		if (Mathf.IsEqualApprox(data.Movement.Angle, 0f)) return;
		
		float speed = Angles.ByteAngleStep * Scene.Instance.ProcessSpeed;
		data.Movement.Angle += data.Movement.Angle >= 180f ? speed : -speed;
		
		if (data.Movement.Angle is < 0f or >= 360f)
		{
			data.Movement.Angle = 0f;
		}
	}

	private void LimitVerticalVelocity()
	{
		if (!data.Movement.IsJumping && logic.Action != States.SpinDash && 
		    !data.Movement.IsForcedSpin && data.Movement.Velocity.Y < -15.75f)
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

	private bool ChangeHammerDashFacingInAir()
	{
		if (logic.Action != States.HammerDash) return false;
		
		if (data.Input.Down.Left)
		{
			data.Visual.Facing = Constants.Direction.Negative;
		}
		else if (data.Input.Down.Right)
		{
			data.Visual.Facing = Constants.Direction.Positive;
		}
		
		return true;
	}

	private void MoveInAirHorizontally()
	{
		if (data.Movement.IsAirLock) return;
		
		if (data.Input.Down.Left)
		{
			if (data.Movement.Velocity.X > 0f)
			{
				data.Movement.Velocity.AccelerationX = -data.Physics.AccelerationAir;
			}
			else if (!SharedData.NoSpeedCap || -data.Movement.Velocity.X < data.Physics.AccelerationTop)
			{
				data.Movement.Velocity.AccelerationX = -data.Physics.AccelerationAir;
				data.Movement.Velocity.MaxX(-data.Physics.AccelerationTop);
			}
			
			data.Visual.Facing = Constants.Direction.Negative;
		}
		else if (data.Input.Down.Right)
		{
			if (data.Movement.Velocity.X < 0f)
			{
				data.Movement.Velocity.AccelerationX = data.Physics.AccelerationAir;
			}
			else if (!SharedData.NoSpeedCap || data.Movement.Velocity.X < data.Physics.AccelerationTop)
			{
				data.Movement.Velocity.AccelerationX = data.Physics.AccelerationAir;
				data.Movement.Velocity.MinX(data.Physics.AccelerationTop);
			}
			
			data.Visual.Facing = Constants.Direction.Positive;
		}
	}

	private void ApplyAirDrag()
	{
		if (!data.Damage.IsHurt && data.Movement.Velocity.Y is < 0f and > -4f)
		{
			data.Movement.Velocity.AccelerationX = MathF.Floor(data.Movement.Velocity.X * 8f) / -256f;
		}
	}
}
