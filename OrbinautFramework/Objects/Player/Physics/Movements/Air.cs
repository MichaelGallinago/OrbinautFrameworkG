using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Modules;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Physics.Movements;

public struct Air(PlayerData data)
{
	public void Move()
	{
		if (data.Physics.IsGrounded || data.Death.IsDead) return;
		if (data.State is States.Carried or States.Climb or States.SpinDash || 
		    data.State == States.Glide && ActionState != (int)GlideStates.Fall) return;

		RotateInAir();
		LimitVerticalVelocity();
		if (ChangeHammerDashFacingInAir()) return;
		MoveInAirHorizontally();
		ApplyAirDrag();
	}

	private void RotateInAir()
	{
		if (Mathf.IsEqualApprox(data.Rotation.Angle, 0f)) return;
		
		float speed = Angles.ByteAngleStep * Scene.Instance.ProcessSpeed;
		data.Rotation.Angle += data.Rotation.Angle >= 180f ? speed : -speed;
		
		if (data.Rotation.Angle is < 0f or >= 360f)
		{
			data.Rotation.Angle = 0f;
		}
	}

	private void LimitVerticalVelocity()
	{
		if (!data.Physics.IsJumping && data.State != States.SpinDash && 
		    !data.Physics.IsForcedSpin && data.Physics.Velocity.Y < -15.75f)
		{
			data.Physics.Velocity.Y = -15.75f;
		}
		else if (SharedData.PhysicsType == PhysicsCore.Types.CD && data.Physics.Velocity.Y > 16f)
		{
			data.Physics.Velocity.Y = 16f;
		}
	}

	private bool ChangeHammerDashFacingInAir()
	{
		if (data.State != States.HammerDash) return false;
		
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
		if (data.Physics.IsAirLock) return;
		
		if (data.Input.Down.Left)
		{
			if (data.Physics.Velocity.X > 0f)
			{
				data.Physics.Velocity.AccelerationX = -PhysicParams.AccelerationAir;
			}
			else if (!SharedData.NoSpeedCap || -data.Physics.Velocity.X < PhysicParams.AccelerationTop)
			{
				data.Physics.Velocity.AccelerationX = -PhysicParams.AccelerationAir;
				data.Physics.Velocity.MaxX(-PhysicParams.AccelerationTop);
			}
			
			data.Visual.Facing = Constants.Direction.Negative;
		}
		else if (data.Input.Down.Right)
		{
			if (data.Physics.Velocity.X < 0f)
			{
				data.Physics.Velocity.AccelerationX = PhysicParams.AccelerationAir;
			}
			else if (!SharedData.NoSpeedCap || data.Physics.Velocity.X < PhysicParams.AccelerationTop)
			{
				data.Physics.Velocity.AccelerationX = PhysicParams.AccelerationAir;
				data.Physics.Velocity.MinX(PhysicParams.AccelerationTop);
			}
			
			data.Visual.Facing = Constants.Direction.Positive;
		}
	}

	private void ApplyAirDrag()
	{
		if (!data.Damage.IsHurt && data.Physics.Velocity.Y is < 0f and > -4f)
		{
			data.Physics.Velocity.AccelerationX = MathF.Floor(data.Physics.Velocity.X * 8f) / -256f;
		}
	}
}
