using System;
using EnumToStringNameSourceGenerator;
using Godot;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.Animations;
using OrbinautFrameworkG.Framework.StaticStorages;
using OrbinautFrameworkG.Framework.Tiles;
using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Sprite;

namespace OrbinautFrameworkG.Objects.Player.Characters.Tails.Tail;

[Tool]
public partial class TailNode : AdvancedAnimatedSprite
{
	[EnumToStringName] public enum TailAnimations : byte { Idle, Fly, Move, Hidden }
	
	public void Animate(IPlayer player)
	{
		if (Scene.Instance.State == Scene.States.Paused) return;
		
		Offset = Vector2.Zero;
		
		switch (player.Data.Sprite.Animation)
		{
			case Animations.Idle or Animations.Wait or Animations.Duck or Animations.LookUp:
				PlayAnimation(TailAnimationsStringNames.Idle);
				break;
			
			case Animations.Fly or Animations.FlyTired:
				float speed = player.Data.Movement.Velocity.Y >= 0f || 
				              player.Data.Sprite.Animation == Animations.FlyTired ? 0.5f : 1f;
				PlayAnimation(TailAnimationsStringNames.Fly, speed);
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
				PlayAnimation(TailAnimationsStringNames.Move);
				break;
			
			default:
				PlayAnimation(TailAnimationsStringNames.Hidden);
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
			
		if (data.Visual.Scale.X < 0f)
		{
			angle += Angles.CircleHalf;
		}
		
		if (Improvements.SmoothRotation) return angle;
		
		return MathF.Ceiling((angle - 22.5f) / 45f) * 45f;
	}
	
	private void ChangeDirection(PlayerData data)
	{
		Scale = Scale with { X = data.Movement.IsSpinning && data.Movement.IsGrounded ? 
			data.Movement.GroundSpeed >= 0f ? 1f : -1f : data.Visual.Scale.X };
	}
}
