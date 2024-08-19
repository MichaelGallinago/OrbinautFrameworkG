using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Sprite;
using OrbinautFramework3.Objects.Spawnable.Shield;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

[FsmSourceGenerator.FsmState("Action")]
public struct Jump(PlayerData data)
{
	public void Enter()
	{
		data.Movement.IsSpinning = true;
		data.Visual.Animation = Animations.Spin;
	}

	public States Perform()
	{
		if (data.Movement.IsGrounded) return States.Jump;

		if (!data.Input.Down.Abc)
		{
			data.Movement.Velocity.MaxY(data.Physics.MinimalJumpSpeed);
		}

		if (data.Movement.Velocity.Y < data.Physics.MinimalJumpSpeed) return States.Jump;
		if (CpuInputTimer == 0 && data.Id > 0) return States.Jump;

		if (Transform()) return States.Transform;

		if (data.Node.Type == PlayerNode.Types.Sonic)
		{
			return JumpSonic();
		}
		
		return data.Node.Type switch
		{
			_ when !data.Input.Press.Abc => States.Jump,
			PlayerNode.Types.Tails => States.Flight,
			PlayerNode.Types.Knuckles => States.GlideAir,
			PlayerNode.Types.Amy => States.HammerSpin,
			_ => States.Jump
		};
	}

	private bool Transform()
	{
		if (!data.Input.Press.C || data.Super.IsSuper) return false;
		if (SharedData.EmeraldCount != 7 || SharedData.PlayerRings < 50) return false;

		data.ResetState();
		data.Movement.IsCorePhysicsSkipped = true;
		return true;
	}

	private States JumpSonic()
	{
		if (SharedData.DropDash && !data.Input.Down.Abc)
		{
			if (data.Node.Shield.Type <= ShieldContainer.Types.Normal ||
			    data.Super.IsSuper || data.Item.InvincibilityTimer > 0f)
			{
				return States.DropDash;
			}
			return States.Jump;
		}

		PerformBarrierAbility();
		return States.Jump;
	}
	
	private void PerformBarrierAbility()
	{
		if (!data.Input.Press.Abc || data.Super.IsSuper) return;
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
		if (!SharedData.DoubleSpin) return;

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
		data.Movement.Velocity.Vector = new Vector2(0f, 8f);
		//TODO: update shield animation
		AudioPlayer.Sound.Play(SoundStorage.ShieldBubble2);
	}

	private void JumpFlameBarrier()
	{
		data.SetCameraDelayX(16f);

		data.Movement.IsAirLock = true;
		data.Movement.Velocity.Vector = new Vector2(8f * (float)data.Visual.Facing, 0f);

		//TODO: update shield animation
		if (data.Node.Shield.AnimationType == ShieldContainer.AnimationTypes.FireDash)
		{
			data.Node.Shield.Frame = 0;
		}
		else
		{
			data.Node.Shield.AnimationType = ShieldContainer.AnimationTypes.FireDash;
		}

		//TODO: check data.PlayerNode.ZIndex
		data.Node.ZIndex = -1;

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
