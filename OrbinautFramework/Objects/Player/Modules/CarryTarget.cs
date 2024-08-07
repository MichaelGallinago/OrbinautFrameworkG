using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Physics;
using OrbinautFramework3.Objects.Player.PlayerActions;

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
		    
		    data.IsSpinning = true;
		    data.IsJumping = true;
		    data.Action.Type = Actions.Types.Default;
		    data.Visual = Animations.Spin;
    		Radius = RadiusSpin;
    		Velocity.Vector = new Vector2(0f, PhysicParams.MinimalJumpSpeed);
    				
    		if (Input.Down.Left)
    		{
    			Velocity.X = -2f;
    		}
    		else if (Input.Down.Right)
    		{
    			Velocity.X = 2f;
    		}
    		
    		AudioPlayer.Sound.Play(SoundStorage.Jump);
    		return;
    	}
    	
    	if (Action != Actions.Carried || carrier.Action != Actions.Flight || !Position.IsEqualApprox(previousPosition))
    	{
    		carrier.CarryTarget = null;
    		carrier.CarryTimer = 60f;
    		Action = Actions.None;
    		return;
    	}
    	
    	AttachToCarrier(carrier);
    }
    
    public void AttachToCarrier(ICarrier carrier)
    {
    	Facing = carrier.Facing;
    	Velocity.Vector = carrier.Velocity.Vector;
    	Position = carrier.Position + new Vector2(0f, 28f);
    	Scale = new Vector2(Math.Abs(Scale.X) * (float)carrier.Facing, Scale.Y);
    	
    	carrier.CarryTargetPosition = Position;
    }
}
