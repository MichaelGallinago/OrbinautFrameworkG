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

public class Landing(PlayerData data, IPlayerLogic logic, Action landAction)
{
	public event Action Landed = landAction;
	
	private readonly Action _landAction = landAction;
	
	public void Land()
	{
		MovementData movement = data.Movement;
		data.ResetGravity();
		movement.IsGrounded = true;
		
		switch (logic.Action)
		{
			case States.SpinDash or States.HammerDash: return;
			case States.Dash: _landAction(); return;
		}
		
		if (WaterBarrierBounce()) return;
		
		if (data.State == PlayerStates.Hurt)
		{
			movement.GroundSpeed.Value = 0f;
		}
		
		movement.IsAirLock = false;
		movement.IsJumping = false;
		movement.IsSpinning = false;

		VisualData visual = data.Visual;
		visual.SetPushBy = null;
		visual.Angle = movement.Angle;
		data.Sprite.Animation = Animations.Move;
		
		data.State = PlayerStates.Control;
		data.Cpu.State = CpuLogic.States.Main;
		data.Item.ComboCounter = 0;

		CollisionData collision = data.Collision;
		collision.TileBehaviour = Constants.TileBehaviours.Floor;
		
		data.Node.Shield.State = ShieldContainer.States.None;

		Landed();
		
		if (movement.IsSpinning) return;
		movement.Position.Y += collision.Radius.Y - collision.RadiusNormal.Y;
		collision.Radius = collision.RadiusNormal;
	}
    
	private bool WaterBarrierBounce()
	{
		ShieldContainer shield = data.Node.Shield;
		if (shield.State != ShieldContainer.States.Active || shield.Type != ShieldContainer.Types.Bubble) return false;
		
		MovementData movement = data.Movement;
		float force = data.Water.IsUnderwater ? -4f : -7.5f;
		float radians = Mathf.DegToRad(movement.Angle);
		movement.Velocity.Vector = new Vector2(MathF.Sin(radians), MathF.Cos(radians)) * force;
	    
		shield.State = ShieldContainer.States.None;
		data.Collision.OnObject = null;
		movement.IsGrounded = false;
	    
		//TODO: replace animation
		shield.AnimationType = ShieldContainer.AnimationTypes.BubbleBounce;
	    
		AudioPlayer.Sound.Play(SoundStorage.ShieldBubble2);
		
		return true;
	}
}
