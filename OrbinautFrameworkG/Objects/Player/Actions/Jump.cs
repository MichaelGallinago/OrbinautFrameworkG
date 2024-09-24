using Godot;
using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.StaticStorages;
using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Logic;
using OrbinautFrameworkG.Objects.Player.Sprite;
using OrbinautFrameworkG.Objects.Spawnable.Shield;
using static OrbinautFrameworkG.Objects.Player.ActionFsm;

namespace OrbinautFrameworkG.Objects.Player.Actions;

[FsmSourceGenerator.FsmState("Action")]
public readonly struct Jump(PlayerData data, IPlayerLogic logic)
{
	public void Enter()
	{
		data.Movement.IsSpinning = true;
		data.Sprite.Animation = Animations.Spin;
	}

	public States Perform()
	{
		if (data.Movement.IsGrounded) return States.Jump;

		if (!data.Input.Down.Aby)
		{
			data.Movement.Velocity.Y.SetMax(data.Physics.MinimalJumpSpeed);
		}

		if (data.Movement.Velocity.Y < data.Physics.MinimalJumpSpeed) return States.Jump;
		if (data.Cpu.InputTimer == 0f && data.Id > 0) return States.Jump;

		if (Transform()) return States.Transform;

		if (data.Node.Type == PlayerNode.Types.Sonic)
		{
			return JumpSonic();
		}

		if (!data.Input.Press.Aby) return States.Jump;
		
		return data.Node.Type switch
		{
			PlayerNode.Types.Tails => States.Flight,
			PlayerNode.Types.Knuckles => States.GlideAir,
			PlayerNode.Types.Amy => States.HammerSpin,
			_ => States.Jump
		};
	}

	public static States OnLand() => States.Default;

	private bool Transform()
	{
		if (!data.Input.Press.X || data.Super.IsSuper) return false;
		if (SaveData.EmeraldCount != 7 || SharedData.PlayerRings < 50) return false;

		logic.ResetData();
		data.Movement.IsCorePhysicsSkipped = true;
		return true;
	}

	private States JumpSonic()
	{
		if (OriginalDifferences.DropDash && !data.Input.Down.Aby)
		{
			if (data.Node.Shield.Type <= ShieldContainer.Types.Normal) return States.DropDash;
			if (data.Super.IsSuper || data.Item.InvincibilityTimer > 0f) return States.DropDash;
			return States.Jump;
		}

		PerformBarrierAbility();
		return States.Jump;
	}
	
	private void PerformBarrierAbility()
	{
		if (!data.Input.Press.Aby || data.Super.IsSuper) return;
		if (data.Node.Shield.State != ShieldContainer.States.None || data.Item.InvincibilityTimer > 0f) return;

		data.Node.Shield.State = ShieldContainer.States.Active;
		data.Movement.IsAirLock = false;

		switch (data.Node.Shield.Type)
		{
			case ShieldContainer.Types.None: JumpDoubleSpin(); break;
			case ShieldContainer.Types.Bubble: JumpWaterBarrier(); break;
			case ShieldContainer.Types.Fire: JumpFlameBarrier(); break;
			case ShieldContainer.Types.Lightning: JumpThunderBarrier(); break;
		}
	}

	private void JumpDoubleSpin()
	{
		if (!OriginalDifferences.DoubleSpin) return;

		data.Node.Shield.State = ShieldContainer.States.DoubleSpin;
		//TODO: obj_double_spin
		/*
		with obj_double_spin
		{
			if TargetPlayer == other.id
			{
				instance_destroy();
			}
		}
		*/

		//TODO: obj_double_spin
		//instance_create(x, y, obj_double_spin, { TargetPlayer: id });
		AudioPlayer.Sound.Play(SoundStorage.DoubleSpin);
	}

	private void JumpWaterBarrier()
	{
		data.Node.Shield.AnimationType = ShieldContainer.AnimationTypes.BubbleBounce;
		data.Movement.Velocity = new Vector2(0f, 8f);
		//TODO: update shield animation
		AudioPlayer.Sound.Play(SoundStorage.ShieldBubble2);
	}

	private void JumpFlameBarrier()
	{
		data.Node.SetCameraDelayX(16f);

		data.Movement.IsAirLock = true;
		data.Movement.Velocity = new Vector2(8f * (float)data.Visual.Facing, 0f);

		//TODO: update shield animation
		if (data.Node.Shield.AnimationType == ShieldContainer.AnimationTypes.FireDash)
		{
			data.Node.Shield.Frame = 0;
		}
		else
		{
			data.Node.Shield.AnimationType = ShieldContainer.AnimationTypes.FireDash;
		}

		//TODO: check data.Visual.ZIndex
		data.Visual.ZIndex = -1;

		AudioPlayer.Sound.Play(SoundStorage.ShieldFire2);
	}

	private void JumpThunderBarrier()
	{
		data.Node.Shield.State = ShieldContainer.States.Disabled;
		data.Movement.Velocity.Y = -5.5f;

		for (var i = 0; i < 4; i++)
		{
			//TODO: obj_barrier_sparkle
			//instance_create(x, y, obj_barrier_sparkle, { Sparkle_ID: i });
		}

		AudioPlayer.Sound.Play(SoundStorage.ShieldLightning2);
	}
}
