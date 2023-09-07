using Godot;
using System;
using System.Linq;

public partial class Tail : CommonObject
{
    public float Angle { get; set; }
    public Player Target { get; set; }

    public Tail(Player target)
    {
	    Target = target;
    }
    
    public override void _Ready()
    {
	    SetBehaviour(ObjectRespawnData.BehaviourType.Unique);
    }

    protected override void Step(double processSpeed)
    {
        // Get target player
        if (Target == null || !IsInstanceValid(Target) || Target.Type != PlayerConstants.Type.Tails)
		{
			QueueFree();
			return;
		}
		
		switch (Target.Animation)
		{
			case PlayerConstants.Animation.Idle:
			case PlayerConstants.Animation.Duck:
			case PlayerConstants.Animation.LookUp:
				Sprite.SetAnimation("idle", new[]{8});
				break;
			case PlayerConstants.Animation.FlyLift:
			case PlayerConstants.Animation.Fly:
			case PlayerConstants.Animation.FlyTired:
				int speed = Target.Speed.Y >= 0f || Target.Animation == PlayerConstants.Animation.FlyTired ? 2 : 1;
				Sprite.SetAnimation("fly");
				Sprite.UpdateDuration(new[]{speed});
				break;
			case PlayerConstants.Animation.Push:
			case PlayerConstants.Animation.Skid:
			case PlayerConstants.Animation.Spin:
			case PlayerConstants.Animation.Grab:
			case PlayerConstants.Animation.Balance:
			case PlayerConstants.Animation.SpinDash:
				var offsetX = 36;
				var offsetY = 24;
				
				if (Target.Animation is PlayerConstants.Animation.SpinDash or PlayerConstants.Animation.Grab)
				{
					offsetX -= 5;
				}
				else if (Target.Animation != PlayerConstants.Animation.Spin)
				{
					offsetX -= 7;
					offsetY -= 5;
				}

				Sprite.Offset = new Vector2(offsetX, offsetY);
				Sprite.SetAnimation("tail", new[]{4});
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
				var step  = 0f;
				
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
				Angle = Mathf.RadToDeg(Mathf.Atan2(
					Mathf.Sin(angle) + Mathf.Sin(Angle) * step, 
					Mathf.Cos(angle) + Mathf.Cos(Angle) * step));
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
		//Position = Target.Position;
		//depth = _player.depth + 1;
		
		// TODO: check Triangly
		// visible = _player.visible;
    }
}
