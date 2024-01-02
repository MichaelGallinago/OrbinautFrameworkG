using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Animations;
using OrbinautFramework3.Objects.Player.Extensions;

namespace OrbinautFramework3.Objects.Player;

[Tool]
public partial class PlayerAnimatedSprite : AdvancedAnimatedSprite
{
	[Export] private Godot.Collections.Array<AdvancedSpriteFrames> _array;
	
	public float AnimationTimer { get; set; }
	public Animations AnimationType { get; set; }
	public bool IsFrameChanged { get; private set; }

	private AnimationData _data;
	private Animations _animationBuffer;

	public override void _Ready()
	{
		base._Ready();
		FrameChanged += () => IsFrameChanged = true;
	}

	public void Animate(AnimationData data)
	{
		_data = data;
		
		if (FrameworkData.UpdateObjects)
		{
			if (_animationBuffer == Animations.None && AnimationTimer > 0f)
			{
				_animationBuffer = AnimationType;
			}
		
			if (AnimationTimer < 0)
			{
				if (AnimationType == _animationBuffer)
				{
					AnimationType = Animations.Move;
				}
			
				AnimationTimer = 0;
				_animationBuffer = Animations.None;
			}
			else if (_animationBuffer != Animations.None)
			{
				AnimationTimer--;
			}
		}
		
		if (AnimationType != Animations.Spin || IsFrameChanged)
		{
			Scale = new Vector2(Math.Abs(Scale.X) * (float)data.Facing, Scale.Y);
		}
		
		switch (data.Type)
		{
			case Player.Types.Sonic when data.IsSuper: AnimateSuperSonic(); break;
			case Player.Types.Sonic: AnimateSonic(); break;
			case Player.Types.Tails: AnimateTails(); break;
			case Player.Types.Knuckles: AnimateTails(); break;
			case Player.Types.Amy: AnimateAmy(); break;
			
			case Player.Types.None:
			case Player.Types.Global:
			case Player.Types.GlobalAI:
				break;
		}
		
		IsFrameChanged = false;
	}
	
	private void AnimateSuperSonic()
	{
		float speed = AnimationType switch
		{
			Animations.Move => GetGroundAnimationSpeed(9f),
			Animations.Push => GetGroundAnimationSpeed(9f),
			Animations.Spin => GetGroundAnimationSpeed(5f),
			_ => 1f
		};
		
		string animationName = GetAnimationName(false, 8f);
		
		SetAnimation(animationName, speed);

		if (AnimationType != Animations.Move || animationName != "Walk") return;

		if (FrameworkData.Time % 4d > 1d) return;
		int frameCount = SpriteFrames.GetFrameCount(Animation);
		SetFrameAndProgress((Frame + frameCount / 2) % frameCount, FrameProgress);
	}
	
	private void AnimateSonic()
	{
		float speed = AnimationType switch
		{
			Animations.Move => GetGroundAnimationSpeed(9f),
			Animations.Push => GetGroundAnimationSpeed(9f),
			Animations.Spin => GetGroundAnimationSpeed(5f),
			_ => 1f
		};
		
		string animationName = GetAnimationName(SharedData.PeelOut, 6f);
		
		SetAnimation(animationName, speed);
	}

	private void AnimateTails()
	{
		float speed = AnimationType switch
		{
			Animations.Move => GetGroundAnimationSpeed(9f),
			Animations.Push => GetGroundAnimationSpeed(9f),
			Animations.Swim => _data.Speed.Y < 0f ? 1f : 0.5f,
			_ => 1f
		};
		
		string animationName = GetAnimationName(true, 6f);
		
		SetAnimation(animationName, speed);
		
		if (AnimationType != Animations.FlyLift) return;
		Frame = _data.Speed.Y < 0 ? 1 : 0;
	}
	
	private void AnimateKnuckles()
	{
		float speed = AnimationType switch
		{
			Animations.Move => GetGroundAnimationSpeed(9f),
			Animations.Push => GetGroundAnimationSpeed(9f),
			Animations.Spin => GetGroundAnimationSpeed(5f),
			_ => 1f
		};

		string animationName = GetAnimationName(false, 6f);
		
		if (AnimationType == Animations.GlideFall)
		{
			SetAnimation(animationName, (int)_data.ActionValue, speed);
			return;
		}
		
		SetAnimation(animationName, speed);
	}
	
	private void AnimateAmy()
	{
		float speed = AnimationType switch
		{
			Animations.Move => GetGroundAnimationSpeed(9f),
			Animations.Push => GetGroundAnimationSpeed(9f),
			Animations.Spin => GetGroundAnimationSpeed(5f),
			Animations.HammerSpin => GetGroundAnimationSpeed(5f),
			_ => 1f
		};

		string animationName = GetAnimationName(true, 6f);

		if (AnimationType == Animations.HammerSpin)
		{
			SetAnimation(animationName, Frame, speed);
			return;
		}
		
		SetAnimation(animationName, speed);
	}

	private string GetAnimationName(bool canDash, float runThreshold)
	{
		return (AnimationType == Animations.Move ? 
			GetMoveAnimation(canDash, runThreshold) : AnimationType).ToStringFast();
	}
	
	private Animations GetMoveAnimation(bool canDash, float runThreshold)
	{
		const float dashThreshold = 10f;
		
		float speed = Math.Abs(_data.GroundSpeed);

		if (speed < runThreshold) return Animations.Walk;
		return canDash && speed >= dashThreshold ? Animations.Dash : Animations.Run;
	}
	
	private float GetGroundAnimationSpeed(float speedLimit)
	{
		return MathF.Floor(Math.Clamp(Math.Abs(_data.GroundSpeed), 1f, speedLimit));
	}
}
