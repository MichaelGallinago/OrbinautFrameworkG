using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Animations;

namespace OrbinautFramework3.Objects.Player;

[Tool]
public partial class Tail : AdvancedAnimatedSprite
{
	public void Animate(ITailed data)
	{
		if (Scene.Local.State == Scene.States.Paused) return;
		
		Offset = Vector2.Zero;
		
		switch (data.Animation)
		{
			case Animations.Idle or Animations.Duck or Animations.LookUp:
				SetAnimation("Idle");
				break;
			
			case Animations.Fly or Animations.FlyTired:
				float speed = data.Velocity.Y >= 0f || data.Animation == Animations.FlyTired ? 0.5f : 1f;
				SetAnimation("Fly", speed);
				break;
			
			case Animations.Push or Animations.Skid or Animations.Spin or 
				Animations.Grab or Animations.Balance or Animations.SpinDash:
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
		
		RotationDegrees = GetTailAngle(data);
		ChangeDirection(data);
	}
	
	private static float GetTailAngle(ITailed data)
	{
		if (!data.IsSpinning) return data.RotationDegrees;
		
		if (data.IsGrounded) return data.VisualAngle;
		
		float angle = Mathf.RadToDeg(MathF.Atan2(data.Velocity.Y, data.Velocity.X));
			
		if (data.Scale.X < 0f)
		{
			angle += 180f;
		}
		
		if (SharedData.RotationMode != 0) return angle;
		
		return MathF.Ceiling((angle - 22.5f) / 45f) * 45f;
	}
	
	private void ChangeDirection(ITailed data)
	{
		Scale = Scale with { X = data.IsSpinning && data.IsGrounded ? 
			data.GroundSpeed.Value >= 0f ? 1f : -1f : data.Scale.X };
	}
}
