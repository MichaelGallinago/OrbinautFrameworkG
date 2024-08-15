using Godot;
using System;
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
		    
		    data.State = States.Jump;
		    data.Collision.Radius = data.Collision.RadiusSpin;

		    var velocityX = 0f;
		    if (data.Input.Down.Left)
		    {
			    velocityX = -2f;
		    }
		    else if (data.Input.Down.Right)
		    {
			    velocityX = 2f;
		    }
		    
		    data.Movement.Velocity.Vector = new Vector2(velocityX, data.Physics.MinimalJumpSpeed);
    		return;
    	}
    	
    	if (data.State != States.Carried || carrier.State != States.Flight || 
	        !data.Node.Position.IsEqualApprox(previousPosition))
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
	    
	    IPlayerNode player = data.Node;
	    player.Position = carrier.Position + new Vector2(0f, 28f);
	    player.Scale = new Vector2(Math.Abs(player.Scale.X) * (float)carrier.Facing, player.Scale.Y);
    	
    	carrier.CarryTargetPosition = player.Position;
    }
}
