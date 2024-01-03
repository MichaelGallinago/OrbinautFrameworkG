using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Animations;

namespace OrbinautFramework3.Objects.Player;

[Tool]
public partial class Tail : AdvancedAnimatedSprite
{
	private float _angle;

	public void Animate(TailAnimationData data)
	{
		switch (data.AnimationType)
		{
			case Animations.Idle:
			case Animations.Duck:
			case Animations.LookUp:
				SetAnimation("Idle");
				break;
			
			case Animations.FlyLift:
			case Animations.Fly:
			case Animations.FlyTired:
				float speed = data.Speed.Y >= 0f || data.AnimationType == Animations.FlyTired ? 0.5f : 1f;
				SetAnimation("Fly", speed);
				break;
			
			case Animations.Push:
			case Animations.Skid:
			case Animations.Spin:
			case Animations.Grab:
			case Animations.Balance:
			case Animations.SpinDash:
				var offset = new Vector2I(-23, 0);
				if (data.AnimationType is Animations.SpinDash or Animations.Grab)
				{
					offset.X += 5;
				}
				else if (data.AnimationType != Animations.Spin)
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

	private void ChangeDirection(TailAnimationData data)
	{
		float scaleX = data.IsSpinning && data.IsGrounded
			? (data.GroundSpeed >= 0f ? 1f : -1f) * Mathf.Abs(data.Scale.X)
			: data.Scale.X;

		Scale = new Vector2(scaleX, Scale.Y);
	}

	private void UpdateAngle(TailAnimationData data)
	{
		if (!data.IsSpinning)
		{
			_angle = data.VisualAngle;
		}
		else 
		{
			if (data.IsGrounded)
			{
				// Smooth rotation code by Nihil
				var angle = 360f;
				float step;
				
				if (data.Angle > 22.5 && data.Angle <= 337.5)
				{
					angle = 360f - data.Angle;
					step  = Mathf.Abs(data.GroundSpeed) * 3f / -32f + 2f;
				}
				else
				{
					step = Mathf.Abs(data.GroundSpeed) / -16f + 2f;
				}
				// TODO: Check Atan2 1
				angle = Mathf.DegToRad(angle);
				float mainAngle = Mathf.DegToRad(_angle);
				_angle = Mathf.RadToDeg(MathF.Atan2(
					MathF.Sin(angle) + MathF.Sin(mainAngle) * step, 
					MathF.Cos(angle) + MathF.Cos(mainAngle) * step));
			}
			else
			{	
				//TODO: check Atan2 2
				_angle = Mathf.RadToDeg(Mathf.Atan2(data.Speed.Y, data.Speed.X));
				
				if (data.Scale.X < 0)
				{
					_angle += 180f;
				}
			}
		}
		
		RotationDegrees = FrameworkData.RotationMode == 0 ? Mathf.Ceil((_angle - 22.5f) / 45f) * 45f : _angle;
	}
}
