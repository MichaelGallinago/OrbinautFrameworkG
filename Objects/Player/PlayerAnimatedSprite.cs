using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Animations;
using OrbinautFramework3.Objects.Player.Extensions;

namespace OrbinautFramework3.Objects.Player;

[Tool]
public partial class PlayerAnimatedSprite : AdvancedAnimatedSprite
{
	[Export] private Godot.Collections.Array<AdvancedSpriteFrames> _spriteFrames;
	
	public float AnimationTimer { get; set; }
	public Animations AnimationType { get; set; }
	public bool IsFrameChanged { get; private set; }

	private PlayerAnimationData _data;
	private Animations _animationBuffer;
	private int _spriteFramesIndex;

	public override void _Ready()
	{
		base._Ready();
		FrameChanged += () => IsFrameChanged = true;
	}
	
	public void Animate(PlayerAnimationData data)
	{
		_data = data;
		
		UpdateAnimationBuffer();
		UpdateScale();
		UpdateSpriteFrames();

		switch (data.Type)
		{
			case Types.Sonic: AnimateSonic(SonicType, SonicSpeed); break;
			case Types.Tails: AnimateTails(TailsType, TailsSpeed); break;
			case Types.Knuckles: AnimateKnuckles(KnucklesType, KnucklesSpeed); break;
			case Types.Amy: AnimateAmy(AmyType, SonicSpeed); break;
		}
		
		IsFrameChanged = false;
	}

	private void UpdateAnimationBuffer()
	{
		if (!FrameworkData.UpdateObjects) return;
		
		if (_animationBuffer == Animations.None && AnimationTimer > 0f)
		{
			_animationBuffer = AnimationType;
		}
		
		if (AnimationTimer < 0f)
		{
			if (AnimationType == _animationBuffer)
			{
				AnimationType = Animations.Move;
			}
			
			AnimationTimer = 0f;
			_animationBuffer = Animations.None;
		}
		else if (_animationBuffer != Animations.None)
		{
			AnimationTimer--;
		}
	}

	private void UpdateScale()
	{
		if (AnimationType == Animations.Spin && !IsFrameChanged) return;
		Scale = new Vector2(Math.Abs(Scale.X) * (float)_data.Facing, Scale.Y);
	}

	private void UpdateSpriteFrames()
	{
		int index = _data.Type == Types.Sonic && _data.IsSuper ? 5 : (int)_data.Type;
		if (_spriteFramesIndex == index) return;
		SpriteFrames = _spriteFrames[_spriteFramesIndex = index];
	}
	
	private void AnimateSonic(Animations type, float speed)
	{
		SetAnimationType(type, speed);

		if (!_data.IsSuper || type != Animations.Walk) return;

		if (FrameworkData.Time % 4d > 1d) return;
		int frameCount = SpriteFrames.GetFrameCount(Animation);
		SetFrameAndProgress((Frame + frameCount / 2) % frameCount, FrameProgress);
	}
	
	private float SonicSpeed => AnimationType switch
	{
		Animations.Move => GetGroundAnimationSpeed(9f),
		Animations.Push => GetGroundAnimationSpeed(9f),
		Animations.Spin => GetGroundAnimationSpeed(5f),
		_ => 1f
	};
	
	private Animations SonicType => AnimationType switch
	{
		Animations.Move => _data.IsSuper ? 
			GetMoveAnimation(false, 8f) :
			GetMoveAnimation(SharedData.PeelOut, 6f),
		_ => AnimationType
	};
	
	private void AnimateTails(Animations type, float speed)
	{
		SetAnimationType(type, speed);
		
		if (type != Animations.FlyCarry) return;
		Frame = _data.Speed.Y < 0 ? 1 : 0;
	}
	
	private float TailsSpeed => AnimationType switch
	{
		Animations.Move => GetGroundAnimationSpeed(9f),
		Animations.Push => GetGroundAnimationSpeed(9f),
		Animations.Swim => _data.Speed.Y < 0f ? 1f : 0.5f,
		_ => 1f
	};
	
	private Animations TailsType => AnimationType switch
	{
		Animations.Fly => _data.CarryTarget == null ? Animations.FlyCarry : Animations.Fly,
		Animations.FlyTired => _data.CarryTarget == null ? Animations.FlyCarryTired : Animations.FlyTired,
		Animations.Move => GetMoveAnimation(true, 6f),
		_ => AnimationType
	};
	
	private void AnimateKnuckles(Animations type, float speed)
	{
		if (AnimationType == Animations.GlideFall)
		{
			SetAnimation(type.ToStringFast(), (int)_data.ActionValue, speed);
			return;
		}
		
		SetAnimationType(type, speed);
	}
	
	private float KnucklesSpeed => AnimationType switch
	{
		Animations.Move => GetGroundAnimationSpeed(9f),
		Animations.Push => GetGroundAnimationSpeed(9f),
		Animations.Spin => GetGroundAnimationSpeed(5f),
		_ => 1f
	};
	
	private Animations KnucklesType => AnimationType switch
	{
		Animations.Move => GetMoveAnimation(false, 6f),
		_ => AnimationType
	};
	
	private void AnimateAmy(Animations type, float speed)
	{
		if (AnimationType == Animations.HammerSpin)
		{
			SetAnimation(type.ToStringFast(), Frame, speed);
			return;
		}

		SetAnimationType(type, speed);
	}
	
	private float AmySpeed => AnimationType switch
	{
		Animations.Move => GetGroundAnimationSpeed(9f),
		Animations.Push => GetGroundAnimationSpeed(9f),
		Animations.Spin => GetGroundAnimationSpeed(5f),
		Animations.HammerSpin => GetGroundAnimationSpeed(5f),
		_ => 1f
	};

	private Animations AmyType => AnimationType switch
	{
		Animations.Move => GetMoveAnimation(true, 6f),
		_ => AnimationType
	};
	
	private Animations GetMoveAnimation(bool canDash, float runThreshold)
	{
		const float dashThreshold = 10f;
		
		float speed = Math.Abs(_data.GroundSpeed);

		if (speed < runThreshold) return Animations.Walk;
		return canDash && speed >= dashThreshold ? Animations.Dash : Animations.Run;
	}
	
	private float GetGroundAnimationSpeed(float speedBound)
	{
		return 1f / MathF.Floor(Math.Max(1f, speedBound - Math.Abs(_data.GroundSpeed)));
	}

	private void SetAnimationType(Animations type, float speed) => SetAnimation(type.ToStringFast(), speed);
}
