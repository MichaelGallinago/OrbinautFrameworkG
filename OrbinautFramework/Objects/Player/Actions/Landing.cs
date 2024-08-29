using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Sprite;
using OrbinautFramework3.Objects.Spawnable.Shield;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Actions;

public readonly struct Landing(PlayerData data, PlayerLogic logic, Action landAction)
{
	public void Land()
	{
		data.ResetGravity();
		data.Movement.IsGrounded = true;
		
		switch (logic.Action)
		{
			case States.SpinDash or States.HammerDash: return;
			case States.Dash: landAction(); return;
		}
		
		if (WaterBarrierBounce()) return;
		
		if (data.State == PlayerStates.Hurt)
		{
			data.Movement.GroundSpeed.Value = 0f;
		}
		
		data.Movement.IsAirLock = false;
		data.Movement.IsJumping = false;
		data.Movement.IsSpinning = false;
		
		data.Visual.SetPushBy = null;
		data.Sprite.Animation = Animations.Move;
		
		data.State = PlayerStates.Control;
		data.Cpu.State = CpuLogic.States.Main;
		data.Node.Shield.State = ShieldContainer.States.None;
		data.Item.ComboCounter = 0;
		data.Collision.TileBehaviour = Constants.TileBehaviours.Floor;
		
		landAction();
		
		if (data.Movement.IsSpinning) return;
		data.Node.Position += new Vector2(0f, data.Collision.Radius.Y - data.Collision.RadiusNormal.Y);
		data.Collision.Radius = data.Collision.RadiusNormal;
	}
    
	private bool WaterBarrierBounce()
	{
		if (data.Node.Shield.State != ShieldContainer.States.Active) return false;
		if (SharedData.PlayerShield != ShieldContainer.Types.Bubble) return false;
		
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
}
