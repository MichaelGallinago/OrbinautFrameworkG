using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Spawnable.Shield;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

public static class LandStateSwitcher
{
	[FsmSourceGenerator.FsmStateSwitcher("Action")]
	public static States Land(States currentState, PlayerData data)
	{
		data.ResetGravity();
		data.Movement.IsGrounded = true;
    
		if (currentState is States.SpinDash or States.Dash) return currentState;
		if (WaterBarrierBounce(data)) return currentState;
		
		SetAnimation(data);
    
		if (data.Damage.IsHurt)
		{
			data.Movement.GroundSpeed.Value = 0f;
		}
    
		data.Movement.IsAirLock = false;
		data.Movement.IsSpinning = false;
		data.Visual.SetPushBy = null;
		data.Damage.IsHurt = false;
    
		data.Node.Shield.State = ShieldContainer.States.None;
		data.Item.ComboCounter = 0;
		data.Collision.TileBehaviour = Constants.TileBehaviours.Floor;
    
		CpuState = CpuStates.Main;

		LandHandler?.Invoke();
    
		if (data.State != States.HammerDash)
		{
			data.State = States.Default;
		}
		else if (Math.Sign(data.Movement.GroundSpeed) != (int)data.Visual.Facing)
		{
			data.Movement.GroundSpeed.Value = -data.Movement.GroundSpeed;
		}

		if (data.Movement.IsSpinning) return;
		data.Node.Position += new Vector2(0f, data.Collision.Radius.Y - data.Collision.RadiusNormal.Y);
    	
		data.Collision.Radius = data.Collision.RadiusNormal;
	}
    
	private static bool WaterBarrierBounce(PlayerData data)
	{
		if (data.Node.Shield.State != ShieldContainer.States.Active || 
		    SharedData.PlayerShield != ShieldContainer.Types.Bubble) return false;
		
		float force = data.Water.IsUnderwater ? -4f : -7.5f;
		float radians = Mathf.DegToRad(data.Movement.Angle);
		data.Movement.Velocity.Vector = new Vector2(MathF.Sin(radians), MathF.Cos(radians)) * force;
	    
		data.Node.Shield.State = ShieldContainer.States.None;
		data.Collision.OnObject = null;
		data.Movement.IsGrounded = false;
	    
		//TODO: replace animation
		data.Node.Shield.AnimationType = ShieldContainer.AnimationTypes.BubbleBounce;
	    
		AudioPlayer.Sound.Play(SoundStorage.ShieldBubble2);
		
		return true;
	}

	private static void SetAnimation(PlayerData data)
	{
		if (data.Visual.Animation == Animations.HammerDash) return;
		data.Visual.Animation = Animations.Move;
	}
}