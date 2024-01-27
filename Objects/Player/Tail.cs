using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Animations;

namespace OrbinautFramework3.Objects.Player;

[Tool]
public partial class Tail : AdvancedAnimatedSprite
{
	private float _angle;

	public void Animate(ITailed data)
	{
		switch (data.Animation)
		{
			case Animations.Idle:
			case Animations.Duck:
			case Animations.LookUp:
				SetAnimation("Idle");
				break;
			
			case Animations.Fly:
			case Animations.FlyTired:
			case Animations.FlyCarry:
				float speed = data.Velocity.Y >= 0f || data.Animation == Animations.FlyTired ? 0.5f : 1f;
				SetAnimation("Fly", speed);
				break;
			
			case Animations.Push:
			case Animations.Skid:
			case Animations.Spin:
			case Animations.Grab:
			case Animations.Balance:
			case Animations.SpinDash:
				var offset = new Vector2I(-23, 0);
				if (data.Animation is Animations.SpinDash or Animations.Grab)
				{
					offset.X += 5;
				}
				else if (data.Animation != Animations.Spin)
				{
					offset += new Vector2I(7, 5);
				}
				
				Offset = offset;
				SetAnimation("Move");
				break;
			
			default:
				SetAnimation("Hidden");
				break;
		}
		
		UpdateAngle(data);
		ChangeDirection(data);
	}

	private void ChangeDirection(ITailed data)
	{
		float scaleX = (data.IsSpinning && data.IsGrounded 
			? (data.GroundSpeed >= 0f ? 1f : -1f) * Math.Abs(Scale.X) : 1f) * (float)data.Facing;

		Scale = Scale with { X = scaleX };
	}

	private void UpdateAngle(ITailed data)
	{
		_angle = GetTailAngle(data);
		RotationDegrees = FrameworkData.RotationMode == 0 ? MathF.Ceiling((_angle - 22.5f) / 45f) * 45f : _angle;
	}

	private float GetTailAngle(ITailed data)
	{
		if (!data.IsSpinning) return data.VisualAngle;

		float angle;
		if (!data.IsGrounded)
		{
			// TODO: Check Atan2
			angle = Mathf.RadToDeg(MathF.Atan2(data.Velocity.Y, data.Velocity.X));
			return (int)data.Facing >= 0 ? angle : angle + 180f;
		}

		// Smooth rotation code by Nihil
		angle = 360f;
		float step = Math.Abs(data.GroundSpeed);
				
		if (data.Angle is > 22.5f and <= 337.5f)
		{
			angle -= data.Angle;
			step = step * 3f / -32f + 2f;
		}
		else
		{
			step = step / -16f + 2f;
		}
		
		// TODO: Check Atan2
		angle = Mathf.DegToRad(angle);
		float mainAngle = Mathf.DegToRad(_angle);
		return Mathf.RadToDeg(MathF.Atan2(
			MathF.Sin(angle) + MathF.Sin(mainAngle) * step, 
			MathF.Cos(angle) + MathF.Cos(mainAngle) * step));
	}
}
