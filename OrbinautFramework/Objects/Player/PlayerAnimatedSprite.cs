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
	
	private IAnimatedPlayer _player;
	private int _spriteFramesIndex;

	public override void _Ready()
	{
		base._Ready();
#if TOOLS
		if (Engine.IsEditorHint()) return;
#endif
		AnimationFinished += OnAnimationFinished;
	}

	public void Animate(IAnimatedPlayer player)
	{
		_player = player;
		
		UpdateSpriteFrames();

		switch (player.Type)
		{
			case Types.Sonic: AnimateSonic(SonicType, SonicSpeed); break;
			case Types.Tails: AnimateTails(TailsType, TailsSpeed); break;
			case Types.Knuckles: AnimateKnuckles(KnucklesType, KnucklesSpeed); break;
			case Types.Amy: AnimateAmy(AmyType, SonicSpeed); break;
		}
		
		UpdateScale();
		player.IsAnimationFrameChanged = false;
		OverrideFrame();
	}

	public int GetAnimationFrameCount(Animations animation, Types playerType)
	{
		return _spriteFrames[(int)playerType].GetFrameCount(animation.ToStringFast());
	}

	private void OverrideFrame()
	{
		if (_player.OverrideAnimationFrame == null) return;
		Frame = (int)_player.OverrideAnimationFrame;
		_player.OverrideAnimationFrame = null;
	}

	private void OnAnimationFinished()
	{
		_player.Animation = _player.Animation switch
		{
			Animations.Bounce or Animations.Breathe or Animations.Flip or Animations.Transform => Animations.Move,
			Animations.Skid when 
				_player.Input.Down is { Left: false, Right: false } || 
			    Math.Abs(_player.GroundSpeed) < PlayerConstants.SkidSpeedThreshold 
					=> Animations.Move,
			_ => _player.Animation
		};
	}

	private void UpdateScale()
	{
		if (_player.Animation == Animations.Spin && !_player.IsAnimationFrameChanged) return;
		Scale = new Vector2(Math.Abs(Scale.X) * (float)_player.Facing, Scale.Y);
	}

	private void UpdateSpriteFrames()
	{
		int index = _player.Type == Types.Sonic && _player.SuperTimer > 0f ? 5 : (int)_player.Type;
		if (_spriteFramesIndex == index) return;
		SpriteFrames = _spriteFrames[_spriteFramesIndex = index];
	}
	
	private void AnimateSonic(Animations type, float speed)
	{
		SetAnimationType(type, speed);

		if (_player.SuperTimer <= 0f || type != Animations.Walk) return;

		if (Scene.Local.Time % 4d >= 2d) return;
		int frameCount = SpriteFrames.GetFrameCount(Animation);
		SetFrameAndProgress((Frame + frameCount / 2) % frameCount, FrameProgress);
	}
	
	private float SonicSpeed => _player.Animation switch
	{
		Animations.Move => GetGroundAnimationSpeed(9f),
		Animations.Push => GetGroundAnimationSpeed(9f),
		Animations.Spin => GetGroundAnimationSpeed(5f),
		_ => 1f
	};
	
	private Animations SonicType => _player.Animation switch
	{
		Animations.Move => _player.SuperTimer > 0f ? 
			GetMoveAnimation(false, 8f) :
			GetMoveAnimation(SharedData.Dash, 6f),
		_ => _player.Animation
	};
	
	private void AnimateTails(Animations type, float speed)
	{
		SetAnimationType(type, speed);
		
		if (type != Animations.FlyCarry) return;
		Frame = _player.Velocity.Y < 0f ? 1 : 0;
	}
	
	private float TailsSpeed => _player.Animation switch
	{
		Animations.Move => GetGroundAnimationSpeed(9f),
		Animations.Push => GetGroundAnimationSpeed(9f),
		Animations.Swim => _player.Velocity.Y < 0f ? 1f : 0.5f,
		_ => 1f
	};
	
	private Animations TailsType => _player.Animation switch
	{
		Animations.Fly => _player.CarryTarget == null ? Animations.Fly : Animations.FlyCarry,
		Animations.FlyTired => _player.CarryTarget == null ? Animations.FlyTired : Animations.FlyCarryTired,
		Animations.Move => GetMoveAnimation(true, 6f),
		_ => _player.Animation
	};
	
	private void AnimateKnuckles(Animations type, float speed)
	{
		if (_player.Animation == Animations.GlideFall)
		{
			SetAnimation(type.ToStringFast(), (int)_player.ActionValue, speed);
			return;
		}
		
		SetAnimationType(type, speed);
	}
	
	private float KnucklesSpeed => _player.Animation switch
	{
		Animations.Move => GetGroundAnimationSpeed(9f),
		Animations.Push => GetGroundAnimationSpeed(9f),
		Animations.Spin => GetGroundAnimationSpeed(5f),
		_ => 1f
	};
	
	private Animations KnucklesType => _player.Animation switch
	{
		Animations.Move => GetMoveAnimation(false, 6f),
		_ => _player.Animation
	};
	
	private void AnimateAmy(Animations type, float speed)
	{
		if (_player.Animation == Animations.HammerSpin)
		{
			SetAnimation(type.ToStringFast(), Frame, speed);
			return;
		}

		SetAnimationType(type, speed);
	}
	
	private float AmySpeed => _player.Animation switch
	{
		Animations.Move => GetGroundAnimationSpeed(9f),
		Animations.Push => GetGroundAnimationSpeed(9f),
		Animations.Spin => GetGroundAnimationSpeed(5f),
		Animations.HammerSpin => GetGroundAnimationSpeed(5f),
		_ => 1f
	};

	private Animations AmyType => _player.Animation switch
	{
		Animations.Move => GetMoveAnimation(true, 6f),
		_ => _player.Animation
	};
	
	private Animations GetMoveAnimation(bool canDash, float runThreshold)
	{
		const float dashThreshold = 10f;
		
		float speed = Math.Abs(_player.GroundSpeed);

		if (speed < runThreshold) return Animations.Walk;
		return canDash && speed >= dashThreshold ? Animations.Dash : Animations.Run;
	}
	
	private float GetGroundAnimationSpeed(float speedBound)
	{
		return 1f / MathF.Floor(Math.Max(1f, speedBound - Math.Abs(_player.GroundSpeed)));
	}

	private void SetAnimationType(Animations type, float speed) => SetAnimation(type.ToStringFast(), speed);
}
