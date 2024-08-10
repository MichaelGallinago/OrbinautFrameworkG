using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Spawnable.Shield;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Physics.StateChangers;

public struct Landing(PlayerData data)
{
	public event Action LandHandler;
	
    public void Land()
    {
	    data.Physics.ResetGravity(data.Water.IsUnderwater);
    	
    	data.Physics.IsGrounded = true;
    
    	switch (data.State)
    	{
    		case States.Flight:
    			AudioPlayer.Sound.Stop(SoundStorage.Flight);
    			AudioPlayer.Sound.Stop(SoundStorage.Flight2);
    			break;
    		
    		case States.SpinDash or States.Dash:
    			if (data.State == States.Dash)
    			{
    				data.Physics.GroundSpeed.Value = ActionValue2;
    			}
    			return;
    	}
    
    	if (WaterBarrierBounce()) return;
    	SetAnimation();
    
    	if (data.Damage.IsHurt)
    	{
    		data.Physics.GroundSpeed.Value = 0f;
    	}
    
    	data.Physics.IsAirLock = false;
    	data.Physics.IsSpinning	= false;
    	data.Physics.IsJumping = false;
    	data.Visual.SetPushBy = null;
    	data.Damage.IsHurt = false;
    
    	data.PlayerNode.Shield.State = ShieldContainer.States.None;
    	data.Item.ComboCounter = 0;
    	data.Collision.TileBehaviour = Constants.TileBehaviours.Floor;
    
    	CpuState = CpuStates.Main;

    	LandHandler?.Invoke();
    
    	if (data.State != States.HammerDash)
    	{
    		data.State = States.None;
    	}
    	else
    	{
    		data.Physics.GroundSpeed.Value = 6 * (int)data.Visual.Facing;
    	}

    	if (data.Physics.IsSpinning) return;
    	data.PlayerNode.Position += new Vector2(0f, data.Collision.Radius.Y - data.Collision.RadiusNormal.Y);
    	
    	data.Collision.Radius = data.Collision.RadiusNormal;
    }
    
    private bool WaterBarrierBounce()
    {
	    if (data.PlayerNode.Shield.State != ShieldContainer.States.Active || 
	        SharedData.PlayerShield != ShieldContainer.Types.Bubble) return false;
		
	    float force = data.Water.IsUnderwater ? -4f : -7.5f;
	    float radians = Mathf.DegToRad(data.Rotation.Angle);
	    data.Physics.Velocity.Vector = new Vector2(MathF.Sin(radians), MathF.Cos(radians)) * force;
	    
	    data.PlayerNode.Shield.State = ShieldContainer.States.None;
	    data.Collision.OnObject = null;
	    data.Physics.IsGrounded = false;
	    
	    //TODO: replace animation
	    data.PlayerNode.Shield.AnimationType = ShieldContainer.AnimationTypes.BubbleBounce;
	    
	    AudioPlayer.Sound.Play(SoundStorage.ShieldBubble2);
		
	    return true;
    }

    private void SetAnimation()
    {
	    if (data.Collision.OnObject != null)
	    {
		    data.Visual.Animation = Animations.Move;
		    return;
	    }
		
	    if (data.Visual.Animation is 
	        Animations.Idle or Animations.Duck or Animations.HammerDash or Animations.GlideGround) return;
	    
	    data.Visual.Animation = Animations.Move;
    }
}