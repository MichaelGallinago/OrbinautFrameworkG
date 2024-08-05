using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;

namespace OrbinautFramework3.Objects.Player.Physics.Movements;

public struct Air
{
	public void Move()
	{
		if (IsGrounded || IsDead) return;
		if (Action is Actions.Carried or Actions.Climb or Actions.SpinDash || 
		    Action == Actions.Glide && ActionState != (int)GlideStates.Fall) return;

		RotateInAir();
		LimitVerticalVelocity();
		if (ChangeHammerDashFacingInAir()) return;
		MoveInAirHorizontally();
		ApplyAirDrag();
	}

	private void RotateInAir()
	{
		if (Mathf.IsEqualApprox(Angle, 0f)) return;
		
		float speed = Angles.ByteAngleStep * Scene.Instance.ProcessSpeed;
		Angle += Angle >= 180f ? speed : -speed;
		
		if (Angle is < 0f or >= 360f)
		{
			Angle = 0f;
		}
	}

	private void LimitVerticalVelocity()
	{
		if (!IsJumping && Action != Actions.SpinDash && !IsForcedSpin && Velocity.Y < -15.75f)
		{
			Velocity.Y = -15.75f;
		}
		else if (SharedData.PhysicsType == PhysicsTypes.CD && Velocity.Y > 16f)
		{
			Velocity.Y = 16f;
		}
	}

	private bool ChangeHammerDashFacingInAir()
	{
		if (Action != Actions.HammerDash) return false;
		
		if (Input.Down.Left)
		{
			Facing = Constants.Direction.Negative;
		}
		else if (Input.Down.Right)
		{
			Facing = Constants.Direction.Positive;
		}
		
		return true;
	}

	private void MoveInAirHorizontally()
	{
		if (IsAirLock) return;
		
		if (Input.Down.Left)
		{
			if (Velocity.X > 0f)
			{
				Velocity.AccelerationX = -PhysicParams.AccelerationAir;
			}
			else if (!SharedData.NoSpeedCap || -Velocity.X < PhysicParams.AccelerationTop)
			{
				Velocity.AccelerationX = -PhysicParams.AccelerationAir;
				Velocity.MaxX(-PhysicParams.AccelerationTop);
			}
			
			Facing = Constants.Direction.Negative;
		}
		else if (Input.Down.Right)
		{
			if (Velocity.X < 0f)
			{
				Velocity.AccelerationX = PhysicParams.AccelerationAir;
			}
			else if (!SharedData.NoSpeedCap || Velocity.X < PhysicParams.AccelerationTop)
			{
				Velocity.AccelerationX = PhysicParams.AccelerationAir;
				Velocity.MinX(PhysicParams.AccelerationTop);
			}
			
			Facing = Constants.Direction.Positive;
		}
	}

	private void ApplyAirDrag()
	{
		if (!IsHurt && Velocity.Y is < 0f and > -4f)
		{
			Velocity.AccelerationX = MathF.Floor(Velocity.X * 8f) / -256f;
		}
	}
}
