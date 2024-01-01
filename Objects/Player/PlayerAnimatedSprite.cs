using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Animations;

namespace OrbinautFramework3.Objects.Player;

public partial class PlayerAnimatedSprite : AdvancedAnimatedSprite
{
	public float AnimationTimer { get; set; }
	public Animations AnimationType { get; set; }
	public Animations AnimationBuffer { get; set; }
	public bool IsFrameChanged { get; private set; }

	private AnimationData _data;

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
			if (AnimationBuffer == Animations.None && AnimationTimer > 0f)
			{
				AnimationBuffer = AnimationType;
			}
		
			if (AnimationTimer < 0)
			{
				if (AnimationType == AnimationBuffer)
				{
					AnimationType = Animations.Move;
				}
			
				AnimationTimer = 0;
				AnimationBuffer = Animations.None;
			}
			else if (AnimationBuffer != Animations.None)
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
			case Player.Types.Knuckles: AnimateKnuckles(); break;
			case Player.Types.Amy: AnimateAmy(); break;
			
			case Player.Types.None:
			case Player.Types.Global:
			case Player.Types.GlobalAI: break;
		}
		
		IsFrameChanged = false;
	}

	private void AnimateSuperSonic()
	{
		string animationName = GetSuperSonicAnimation();

		if (animationName == null) return;

		float speed = AnimationType switch
		{
			Animations.Move => GetGroundAnimationSpeed(9f),
			Animations.Push => GetGroundAnimationSpeed(9f),
			Animations.Spin => GetGroundAnimationSpeed(5f),
			_ => 1f
		};
		
		SetAnimation(animationName, speed);

		if (AnimationType != Animations.Move || animationName != "super_sonic_walk") return;

		if (FrameworkData.Time % 4d > 1d) return;
		int frameCount = SpriteFrames.GetFrameCount(Animation);
		SetFrameAndProgress((Frame + frameCount / 2) % frameCount, FrameProgress);
	}
	
	private string GetSuperSonicAnimation() => AnimationType switch
	{
		// Base animations
		Animations.Idle => "super_sonic_idle",
		Animations.Duck => "super_sonic_duck",
		Animations.LookUp => "super_sonic_lookup",
		Animations.Move => Math.Abs(_data.GroundSpeed) >= 8 ? "super_sonic_run" : "super_sonic_walk",
		Animations.Push => "super_sonic_push",
		Animations.Spin => "super_sonic_spin",
		Animations.SpinDash => "sonic_spin_dash",
		Animations.Hurt => "super_sonic_hurt",
		Animations.Death => "super_sonic_death",
		Animations.Drown => "super_sonic_drown",
		Animations.Balance => "super_sonic_balance",
		Animations.Breathe => "super_sonic_breathe",
		Animations.Bounce => "super_sonic_bounce",
		Animations.Skid => "super_sonic_skid",
		Animations.Grab => "super_sonic_grab",

		// Unique animations
		Animations.Transform => "super_sonic_transform",
		Animations.DropDash => "sonic_drop_dash",
		Animations.BalancePanic => "sonic_balance_panic",
		Animations.BalanceTurn => "sonic_balance_turn",

		_ => null
	};
	
	private void AnimateSonic()
	{
		string animationName = GetSonicAnimation();

		if (animationName == null) return;

		float speed = AnimationType switch
		{
			Animations.Move => GetGroundAnimationSpeed(9f),
			Animations.Push => GetGroundAnimationSpeed(9f),
			Animations.Spin => GetGroundAnimationSpeed(5f),
			_ => 1f
		};
		
		SetAnimation(animationName, speed);
	}
	
	private string GetSonicAnimation() => AnimationType switch
	{
		// Base animations
		Animations.Idle => "sonic_idle",
		Animations.Duck => "sonic_duck",
		Animations.LookUp => "sonic_lookup",
		Animations.Move => GetSonicMoveAnimation(),
		Animations.Push => "sonic_push",
		Animations.Spin => "sonic_spin",
		Animations.SpinDash => "sonic_spin_dash",
		Animations.Hurt => "sonic_hurt",
		Animations.Death => "sonic_death",
		Animations.Drown => "sonic_drown",
		Animations.Balance => "sonic_balance",
		Animations.Breathe => "sonic_breathe",
		Animations.Bounce => "sonic_bounce",
		Animations.Skid => "sonic_skid",
		Animations.Grab => "sonic_grab",

		// Unique animations
		Animations.DropDash => "sonic_drop_dash",
		Animations.BalanceFlip => "sonic_balance_flip",
		Animations.BalancePanic => "sonic_balance_panic",
		Animations.BalanceTurn => "sonic_balance_turn",

		_ => null
	};

	private string GetSonicMoveAnimation()
	{
		float speed = Math.Abs(_data.GroundSpeed);

		if (speed < 6f) return "sonic_walk";
		return SharedData.PeelOut && speed >= 10f ? "sonic_dash" : "sonic_run";
	}

	private void AnimateTails()
	{
		string animationName = GetTailsAnimation();

		if (animationName == null) return;

		float speed = AnimationType switch
		{
			Animations.Move => GetGroundAnimationSpeed(9f),
			Animations.Push => GetGroundAnimationSpeed(9f),
			Animations.Swim => _data.Speed.Y < 0f ? 1f : 0.5f,
			_ => 1f
		};
		
		SetAnimation(animationName, speed);
		
		if (AnimationType != Animations.FlyLift) return;
		Frame = _data.Speed.Y < 0 ? 1 : 0;
	}
	
	private string GetTailsAnimation() => AnimationType switch
	{
		// Base animations
		Animations.Idle => "tails_idle",
		Animations.Duck => "tails_duck",
		Animations.LookUp => "tails_lookup",
		Animations.Move => GetTailsMoveAnimation(),
		Animations.Push => "tails_push",
		Animations.Spin => "tails_spin",
		Animations.SpinDash => "tails_spin_dash",
		Animations.Hurt => "tails_hurt",
		Animations.Death => "tails_death",
		Animations.Drown => "tails_drown",
		Animations.Balance => "tails_balance",
		Animations.Breathe => "tails_breathe",
		Animations.Bounce => "tails_bounce",
		Animations.Skid => "tails_skid",
		Animations.Grab => "tails_grab",

		// Unique animations
		Animations.Fly => "tails_fly",
		Animations.FlyLift => "tails_fly_lift",
		Animations.FlyTired => "tails_fly_tired",
		Animations.Swim => "tails_swim",
		Animations.SwimTired => "tails_swim_tired",

		_ => null
	};
	
	private string GetTailsMoveAnimation()
	{
		float speed = Math.Abs(_data.GroundSpeed);

		if (speed < 6f) return "tails_walk";
		return speed >= 10f ? "tails_dash" : "tails_run";
	}
	
	private void AnimateKnuckles()
	{
		string animationName = GetKnucklesAnimation();

		if (animationName == null) return;

		float speed = AnimationType switch
		{
			Animations.Move => GetGroundAnimationSpeed(9f),
			Animations.Push => GetGroundAnimationSpeed(9f),
			Animations.Spin => GetGroundAnimationSpeed(5f),
			_ => 1f
		};

		if (AnimationType != Animations.GlideFall)
		{
			SetAnimation(animationName, speed);
			return;
		}
		
		SetAnimation(animationName, (int)_data.ActionValue, speed);
	}

	private string GetKnucklesAnimation() => AnimationType switch
	{
		// Base animations
		Animations.Idle => "knuckles_idle",
		Animations.Duck => "knuckles_duck",
		Animations.LookUp => "knuckles_lookup",
		Animations.Move => GetKnucklesMoveAnimation(),
		Animations.Push => "knuckles_push",
		Animations.Spin => "knuckles_spin",
		Animations.SpinDash => "knuckles_spin_dash",
		Animations.Hurt => "knuckles_hurt",
		Animations.Death => "knuckles_death",
		Animations.Drown => "knuckles_drown",
		//TODO : knuckles animations
		Animations.Balance => null, //"knuckles_balance",
		Animations.Breathe => "knuckles_breathe",
		Animations.Bounce => "knuckles_bounce",
		Animations.Skid => null, //"knuckles_skid",
		Animations.Grab => "knuckles_grab",

		// Unique animations
		Animations.GlideAir => "knuckles_glide",
		Animations.GlideFall => "knuckles_fall",
		Animations.GlideGround => "knuckles_slide",
		Animations.GlideLand => "knuckles_land",
		Animations.ClimbWall => "knuckles_climb",
		Animations.ClimbLedge => "knuckles_climb_ledge",
		Animations.BalanceFlip => null, //"knuckles_balance_flip",

		_ => null
	};

	private string GetKnucklesMoveAnimation() => Math.Abs(_data.GroundSpeed) < 6f ? "knuckles_walk" : "knuckles_run";
	
	private void AnimateAmy()
	{
		string animationName = GetAmyAnimation();

		if (animationName == null) return;

		float speed = AnimationType switch
		{
			Animations.Move => GetGroundAnimationSpeed(9f),
			Animations.Push => GetGroundAnimationSpeed(9f),
			Animations.Spin => GetGroundAnimationSpeed(5f),
			Animations.HammerSpin => GetGroundAnimationSpeed(5f),
			_ => 1f
		};

		if (AnimationType != Animations.HammerSpin)
		{
			SetAnimation(animationName, speed);
			return;
		}
		
		SetAnimation(animationName, Frame, speed);
	}
	
	private string GetAmyAnimation() => AnimationType switch
	{
		// Base animations
		Animations.Idle => "amy_idle",
		Animations.Duck => "amy_duck",
		Animations.LookUp => "amy_lookup",
		Animations.Move => GetAmyMoveAnimation(),
		Animations.Push => "amy_push",
		Animations.Spin => "amy_spin",
		Animations.SpinDash => "amy_spin_dash",
		Animations.Hurt => "amy_hurt",
		Animations.Death => "amy_death",
		Animations.Drown => "amy_drown",
		Animations.Balance => "amy_balance",
		Animations.Breathe => "amy_breathe",
		Animations.Bounce => "amy_bounce",
		Animations.Skid => "amy_skid",
		Animations.Grab => "amy_grab",

		// Unique animations
		Animations.HammerSpin => "amy_spin_hammer",
		Animations.HammerRush => "amy_run_hammer",

		_ => null
	};

	private string GetAmyMoveAnimation()
	{
		float speed = Math.Abs(_data.GroundSpeed);

		if (speed < 6f) return "amy_walk";
		return speed >= 10f ? "amy_dash" : "amy_run";
	}
	
	private float GetGroundAnimationSpeed(float maximalDuration)
	{
		return 1f / Mathf.Floor(Math.Max(1f, maximalDuration - Math.Abs(_data.GroundSpeed)));
	}
}
