using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Objects.Player.Data;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Logic;

public readonly struct CarryTargetLogic(PlayerData data, IPlayerLogic logic)
{
    public void OnAttached(ICarrier carrier)
    {
    	Vector2 previousPosition = carrier.TargetPosition;
    	
    	if (data.Input.Press.Aby)
    	{
    		Jump(carrier);
    		return;
    	}
    	
    	if (logic.Action != States.Carried || carrier.State != States.Flight || 
	        !data.Node.Position.IsEqualApprox(previousPosition))
    	{
    		carrier.Target = null;
    		carrier.Cooldown = 60f;
		    logic.Action = States.Default;
    		return;
    	}
    	
    	AttachToCarrier(carrier);
    }
    
    public void AttachToCarrier(ICarrier carrier)
    {
	    data.Visual.Facing = carrier.Facing;
    	data.Movement.Velocity.Vector = carrier.Velocity.Vector;
	    
	    IPlayerNode node = data.Node;
	    node.Position = carrier.Position + new Vector2(0f, 28f);
	    node.Scale = new Vector2(Math.Abs(node.Scale.X) * (float)carrier.Facing, node.Scale.Y);
    	
    	carrier.TargetPosition = node.Position;
    }

    private void Jump(ICarrier carrier)
    {
	    carrier.Target = null;
	    carrier.Cooldown = 18f;
	    
	    logic.Action = States.Jump;
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
		    
	    AudioPlayer.Sound.Play(SoundStorage.Jump);
    }
}
