using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Animations;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.Characters.Tails.Tail;

[Tool]
public partial class TailNode : AdvancedAnimatedSprite //TODO: refactor this
{
	public void Animate(IPlayer player)
	{
		if (Scene.Instance.State == Scene.States.Paused) return;
		
		Offset = Vector2.Zero;
		
		switch (player.Data.Sprite.Animation)
		{
			case Animations.Idle or Animations.Duck or Animations.LookUp:
				SetAnimation("Idle");
				break;
			
			case Animations.Fly or Animations.FlyTired:
				float speed = player.Data.Movement.Velocity.Y >= 0f || 
				              player.Data.Sprite.Animation == Animations.FlyTired ? 0.5f : 1f;
				SetAnimation("Fly", speed);
				break;
			
			case Animations.Push or Animations.Skid or Animations.Spin or 
				Animations.Grab or Animations.Balance or Animations.SpinDash:
				var offset = new Vector2I(-23, 0);
				if (player.Data.Sprite.Animation is Animations.SpinDash or Animations.Grab)
				{
					offset.X += 5;
				}
				else if (player.Data.Sprite.Animation != Animations.Spin)
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
		
		RotationDegrees = GetTailAngle(player.Data);
		ChangeDirection(player.Data);
	}
	
	private static float GetTailAngle(PlayerData data)
	{
		if (!data.Movement.IsSpinning) return data.Node.RotationDegrees;
		
		if (data.Movement.IsGrounded) return data.Visual.Angle;
		
		float angle = Mathf.RadToDeg(MathF.Atan2(data.Movement.Velocity.Y, data.Movement.Velocity.X));
			
		if (data.Node.Scale.X < 0f)
		{
			angle += 180f;
		}
		
		if (SharedData.RotationMode != 0) return angle;
		
		return MathF.Ceiling((angle - 22.5f) / 45f) * 45f;
	}
	
	private void ChangeDirection(PlayerData data)
	{
		Scale = Scale with { X = data.Movement.IsSpinning && data.Movement.IsGrounded ? 
			data.Movement.GroundSpeed.Value >= 0f ? 1f : -1f : data.Node.Scale.X };
	}
}
