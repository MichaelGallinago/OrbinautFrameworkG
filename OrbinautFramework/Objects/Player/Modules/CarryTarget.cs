using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Objects.Player.Data;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Modules;

public struct CarryTarget(PlayerData data)
{
    public void OnAttached(ICarrier carrier)
    {
    	Vector2 previousPosition = carrier.CarryTargetPosition;
    	
    	if (data.Input.Press.Abc)
    	{
    		carrier.CarryTarget = null;
    		carrier.CarryTimer = 18f;
		    
		    data.Movement.IsSpinning = true;
		    data.Movement.IsJumping = true;
		    data.State = States.Default;
		    data.Visual.Animation = Animations.Spin;
		    data.Collision.Radius = data.Collision.RadiusSpin;
		    data.Movement.Velocity.Vector = new Vector2(0f, data.Physics.MinimalJumpSpeed);
    				
    		if (data.Input.Down.Left)
    		{
			    data.Movement.Velocity.X = -2f;
    		}
    		else if (data.Input.Down.Right)
    		{
			    data.Movement.Velocity.X = 2f;
    		}
    		
    		AudioPlayer.Sound.Play(SoundStorage.Jump);
    		return;
    	}
    	
    	if (data.State != States.Carried || carrier.State != States.Flight || 
	        !data.PlayerNode.Position.IsEqualApprox(previousPosition))
    	{
    		carrier.CarryTarget = null;
    		carrier.CarryTimer = 60f;
		    data.State = States.Default;
    		return;
    	}
    	
    	AttachToCarrier(carrier);
    }
    
    public void AttachToCarrier(ICarrier carrier)
    {
	    data.Visual.Facing = carrier.Facing;
    	data.Movement.Velocity.Vector = carrier.Velocity.Vector;
	    
	    IPlayerNode player = data.PlayerNode;
	    player.Position = carrier.Position + new Vector2(0f, 28f);
	    player.Scale = new Vector2(Math.Abs(player.Scale.X) * (float)carrier.Facing, player.Scale.Y);
    	
    	carrier.CarryTargetPosition = player.Position;
    }
}
