using System;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Logic;

namespace OrbinautFramework3.Objects.Player.Data;

public class PhysicsData
{
	private enum Types : byte
    {
        None,
	    Default,        SuperSonic,           Super,
        SpeedUpDefault, SpeedUpSuperSonic,    SpeedUpSuper,
        Underwater,     UnderwaterSuperSonic, UnderwaterSuper
    }
    
    public float Acceleration { get; private set; }
    public float AccelerationGlide { get; private set; }
    public float AccelerationAir { get; private set; }
    public float AccelerationTop { get; private set; }
    public float AccelerationClimb { get; private set; }
    public float Deceleration { get; private set; }
    public float DecelerationRoll { get; private set; }
    public float Friction { get; private set; }
    public float FrictionRoll { get; private set; }
    public float MinimalJumpSpeed { get; private set; }
    public float JumpSpeed { get; private set; }

    private Types _type = Types.None;
    
    public void Update(bool isUnderwater, bool isSuper, PlayerNode.Types playerType, float itemSpeedTimer)
    {
	    Types type = GetType(itemSpeedTimer > 0f, isUnderwater, isSuper, playerType);
	    
	    if (_type == type) return;
	    _type = type;
	    
	    SetParams(type);
	    
	    if (playerType == PlayerNode.Types.Knuckles)
	    {
		    JumpSpeed += 0.5f;
	    }
	    
	    if (SharedData.PhysicsType < PhysicsCore.Types.SK)
	    {
		    if (playerType == PlayerNode.Types.Tails)
		    {
			    DecelerationRoll = Deceleration / 4f;
		    }
	    }
	    else if (isSuper)
	    {
		    FrictionRoll = 0.0234375f;
	    }
    }

    private static Types GetType(bool isSpeedUp, bool isUnderwater, bool isSuper, PlayerNode.Types playerType)
    {
	    byte type = 0;
	    
	    if (isUnderwater)
	    {
		    type += 6;
	    }
	    else if (isSpeedUp)
	    {
		    type += 3;
	    }

	    if (!isSuper) return (Types)type;

	    return (Types)(playerType == PlayerNode.Types.Sonic ? type + 2 : type + 1);
    }
    
	private void SetParams(Types types)
	{
		switch(types)
		{
			case Types.Default:
				Acceleration = 0.046875f;
				AccelerationGlide = 0.015625f;
				AccelerationAir = 0.09375f;
				AccelerationTop = 6f;
				AccelerationClimb = 1f;
				Deceleration = 0.5f;
				DecelerationRoll = 0.125f;
				Friction = 0.046875f;
				FrictionRoll = 0.0234375f;
				MinimalJumpSpeed = -4f;
				JumpSpeed = -6.5f;
				break;
		     
            case Types.SuperSonic:
				Acceleration = 0.1875f;
				AccelerationGlide = 0.015625f;
				AccelerationAir = 0.375f;
				AccelerationTop = 10f;
				AccelerationClimb = 2f;
				Deceleration = 1f;
				DecelerationRoll = 0.125f;
				Friction = 0.046875f;
				FrictionRoll = 0.09375f;
				MinimalJumpSpeed = -4f;
				JumpSpeed = -8f;
				break;

			case Types.Super:
				Acceleration = 0.09375f;
				AccelerationGlide = 0.046875f;
				AccelerationAir = 0.1875f;
				AccelerationTop = 8f;
				AccelerationClimb = 2f;
				Deceleration = 0.75f;
				DecelerationRoll = 0.125f;
				Friction = 0.046875f;
				FrictionRoll = 0.0234375f;
				MinimalJumpSpeed = -4f;
				JumpSpeed = -6.5f;
				break;
			
            case Types.SpeedUpDefault:
				Acceleration = 0.09375f;
				AccelerationGlide = 0.015625f;
				AccelerationAir = 0.1875f;
				AccelerationTop = 12f;
				AccelerationClimb = 1f;
				Deceleration = 0.5f;
				DecelerationRoll =  0.125f;
				Friction = 0.09375f;
				FrictionRoll = 0.046875f;
				MinimalJumpSpeed = -4f;
				JumpSpeed = -6.5f;
	            break;
	            
            case Types.SpeedUpSuperSonic:
				Acceleration = 0.09375f;
				AccelerationGlide = 0.015625f;
				AccelerationAir = 0.1875f;
				AccelerationTop = 12f;
				AccelerationClimb = 2f;
				Deceleration = 1f;
				DecelerationRoll = 0.125f;
				Friction = 0.09375f;
				FrictionRoll = 0.046875f;
				MinimalJumpSpeed = -4f;
				JumpSpeed = -8f;
	            break;
	            
            case Types.SpeedUpSuper:
				Acceleration = 0.09375f;
				AccelerationGlide = 0.046875f;
				AccelerationAir = 0.1875f;
				AccelerationTop = 12f;
				AccelerationClimb = 2f;
				Deceleration = 0.75f;
				DecelerationRoll = 0.125f;
				Friction = 0.09375f;
				FrictionRoll = 0.046875f;
				MinimalJumpSpeed = -4f;
				JumpSpeed = -6.5f;
	            break;
            
            case Types.Underwater:
				Acceleration = 0.0234375f;
	            AccelerationGlide = 0.015625f;
	            AccelerationAir = 0.046875f;
	            AccelerationTop = 3f;
	            AccelerationClimb = 1f;
	            Deceleration = 0.25f;
	            DecelerationRoll = 0.125f;
	            Friction = 0.0234375f;
	            FrictionRoll = 0.0234375f;
	            MinimalJumpSpeed = -2f;
	            JumpSpeed = -3.5f;
	            break;
	            
            case Types.UnderwaterSuperSonic:
				Acceleration = 0.09375f;
				AccelerationGlide = 0.015625f;
				AccelerationAir = 0.1875f;
				AccelerationTop = 5f;
				AccelerationClimb = 2f;
				Deceleration = 0.5f;
				DecelerationRoll = 0.125f;
				Friction = 0.046875f;
				FrictionRoll = 0.046875f;
				MinimalJumpSpeed = -2f;
				JumpSpeed = -3.5f;
	            break;
	            
            case Types.UnderwaterSuper:
				Acceleration = 0.046875f;
	            AccelerationGlide = 0.046875f;
	            AccelerationAir = 0.09375f;
	            AccelerationTop = 4f;
	            AccelerationClimb = 2f;
	            Deceleration = 0.375f;
	            DecelerationRoll = 0.125f;
	            Friction = 0.046875f;
	            FrictionRoll = 0.0234375f;
	            MinimalJumpSpeed = -2f;
	            JumpSpeed = -3.5f;
	            break;
            
			default:
				throw new ArgumentOutOfRangeException(nameof(types), types, null);
		}
    }
}
