using System;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Objects.Player.Physics.StateChangers;

public struct Landing
{
	public event Action LandHandler;
	
    public void Land()
    {
    	ResetGravity();
    	
    	IsGrounded = true;
    
    	switch (Action)
    	{
    		case Actions.Flight:
    			AudioPlayer.Sound.Stop(SoundStorage.Flight);
    			AudioPlayer.Sound.Stop(SoundStorage.Flight2);
    			break;
    		
    		case Actions.SpinDash or Actions.Dash:
    			if (Action == Actions.Dash)
    			{
    				GroundSpeed.Value = ActionValue2;
    			}
    			return;
    	}
    
    	if (WaterBarrierBounce()) return;
    	SetAnimation();
    
    	if (IsHurt)
    	{
    		GroundSpeed.Value = 0f;
    	}
    
    	IsAirLock = false;
    	IsSpinning	= false;
    	IsJumping = false;
    	SetPushAnimationBy = null;
    	IsHurt = false;
    
    	Shield.State = ShieldContainer.States.None;
    	ComboCounter = 0;
    	TileBehaviour = Constants.TileBehaviours.Floor;
    
    	CpuState = CpuStates.Main;

    	LandHandler?.Invoke();
    
    	if (Action != Actions.HammerDash)
    	{
    		Action = Actions.None;
    	}
    	else
    	{
    		GroundSpeed.Value = 6 * (int)Facing;
    	}

    	if (IsSpinning) return;
    	Position += new Vector2(0f, Radius.Y - RadiusNormal.Y);
    	
    	Radius = RadiusNormal;
    }
    
    private bool WaterBarrierBounce()
    {
	    if (Shield.State != ShieldContainer.States.Active || 
	        SharedData.PlayerShield != ShieldContainer.Types.Bubble) return false;
		
	    float force = IsUnderwater ? -4f : -7.5f;
	    float radians = Mathf.DegToRad(Angle);
	    Velocity.Vector = new Vector2(MathF.Sin(radians), MathF.Cos(radians)) * force;

	    Shield.State = ShieldContainer.States.None;
	    OnObject = null;
	    IsGrounded = false;

	    //TODO: replace animation
	    Shield.AnimationType = ShieldContainer.AnimationTypes.BubbleBounce;
			
	    AudioPlayer.Sound.Play(SoundStorage.ShieldBubble2);
		
	    return true;
    }

    private void SetAnimation()
    {
	    if (OnObject != null)
	    {
		    Animation = Animations.Move;
		    return;
	    }
		
	    if (Animation is Animations.Idle or Animations.Duck or Animations.HammerDash or Animations.GlideGround) return;
	    Animation = Animations.Move;
    }
}