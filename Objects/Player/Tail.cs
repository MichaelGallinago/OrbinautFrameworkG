using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Animations;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Player;

public partial class Tail : BaseObject
{
	[Export] public AnimatedSprite Sprite { get; private set; }
	
	public float Angle { get; set; }
	public Player Target { get; set; }

	public Tail(Player target)
	{
		Target = target;
	}
    
	public override void _Ready()
	{
		SetBehaviour(BehaviourType.Unique);
	}
	
	public override void _Process(double delta)
	{
		// Get target player
		if (!IsInstanceValid(Target) || Target.Type != Player.Types.Tails)
		{
			QueueFree();
			return;
		}
		
		switch (Target.Sprite.AnimationType)
		{
			case Animations.Idle:
			case Animations.Duck:
			case Animations.LookUp:
				Sprite.SetAnimation("idle", [8]);
				break;
			case Animations.FlyLift:
			case Animations.Fly:
			case Animations.FlyTired:
				int speed = Target.Speed.Y >= 0f || Target.Sprite.AnimationType == Animations.FlyTired ? 2 : 1;
				Sprite.SetAnimation("fly");
				Sprite.UpdateDuration([speed]);
				break;
			case Animations.Push:
			case Animations.Skid:
			case Animations.Spin:
			case Animations.Grab:
			case Animations.Balance:
			case Animations.SpinDash:
				var offsetX = 36;
				var offsetY = 24;
				
				if (Target.Sprite.AnimationType is Animations.SpinDash or Animations.Grab)
				{
					offsetX -= 5;
				}
				else if (Target.Sprite.AnimationType != Animations.Spin)
				{
					offsetX -= 7;
					offsetY -= 5;
				}

				Sprite.Offset = new Vector2(offsetX, offsetY);
				Sprite.SetAnimation("tail", [4]);
				break;
			default:
				Sprite.SetAnimation("hidden");
				break;
		}
		
		if (!Target.IsSpinning)
		{
			Angle = Target.VisualAngle;
		}
		else 
		{
			if (Target.IsGrounded)
			{
				// Smooth rotation code by Nihil
				var angle = 360f;
				float step;
				
				if (Target.Angle > 22.5 && Target.Angle <= 337.5)
				{
					angle = Target.Angle;
					step  = Mathf.Abs(Target.GroundSpeed) * 3f / -32f + 2f;
				}
				else
				{
					step = Mathf.Abs(Target.GroundSpeed) / -16f + 2f;
				}
				// TODO: Check Atan2 1
				angle = Mathf.DegToRad(angle);
				float mainAngle = Mathf.DegToRad(Angle);
				Angle = Mathf.RadToDeg(MathF.Atan2(
					MathF.Sin(angle) + MathF.Sin(mainAngle) * step, 
					MathF.Cos(angle) + MathF.Cos(mainAngle) * step));
			}
			else
			{	
				//TODO: check Atan2 2
				Angle = Mathf.RadToDeg(Mathf.Atan2(Target.Speed.Y, Target.Speed.X));
				
				if (Target.Scale.X < 0)
				{
					Angle += 180f;
				}
			}
		}
		
		if (FrameworkData.RotationMode == 0)
		{
			RotationDegrees = Mathf.Ceil((Angle - 22.5f) / 45f) * 45f;
		}
		else
		{
			RotationDegrees = Angle;
		}
		
		if (Target.IsSpinning && Target.IsGrounded)
		{
			Scale = new Vector2((Target.GroundSpeed >= 0f ? 1f : -1f) * Mathf.Abs(Target.Scale.X), Scale.Y);
		}
		else
		{
			Scale = new Vector2(Target.Scale.X, Scale.Y);
		}

		//TODO: Check position & depth
		Position = Target.Position;
		ZIndex = Target.ZIndex - 1;
	}
}
