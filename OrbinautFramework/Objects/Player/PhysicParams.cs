using System.Collections.Generic;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player;

public struct PhysicParams(
    float acceleration, float accelerationGlide, float accelerationAir, float accelerationTop, float accelerationClimb,
    float deceleration, float decelerationRoll, float friction, float frictionRoll, 
    float minimalJumpSpeed, float jumpSpeed)
{
	private enum Type : byte
    {
        Default, SuperSonic, Super, Underwater, UnderwaterSuperSonic, UnderwaterSuper
    }
    
    public float Acceleration { get; private set; } = acceleration;
    public float AccelerationGlide { get; private set; }  = accelerationGlide;
    public float AccelerationAir { get; private set; }  = accelerationAir;
    public float AccelerationTop { get; private set; }  = accelerationTop;
    public float AccelerationClimb { get; private set; }  = accelerationClimb;
    public float Deceleration { get; }  = deceleration;
    public float DecelerationRoll { get; private set; }  = decelerationRoll;
    public float Friction { get; private set; }  = friction;
    public float FrictionRoll { get; private set; }  = frictionRoll;
    public float MinimalJumpSpeed { get; private set; }  = minimalJumpSpeed;
    public float JumpSpeed { get; private set; }  = jumpSpeed;

    private static readonly Dictionary<Type, PhysicParams> ParamsMap = new()
    {
	    { Type.Default, new PhysicParams(
		    0.046875f,
		    0.015625f,
		    0.09375f,
		    6f,
		    1f,
		    0.5f,
		    0.125f,
		    0.046875f,
		    0.0234375f,
		    -4f,
		    -6.5f
		)},
	    
	    { Type.SuperSonic, new PhysicParams(
		    0.1875f,
		    0.015625f,
		    0.375f,
		    10f,
		    2f,
		    1f,
		    0.125f,
		    0.046875f,
		    0.09375f,
		    -4f,
		    -8f
	    )},
	    
	    { Type.Super, new PhysicParams(
		    0.09375f,
		    0.046875f,
		    0.1875f,
		    8f,
		    2f,
		    0.75f,
		    0.125f,
		    0.046875f,
		    0.0234375f,
		    -4f,
		    -6.5f
		)},
	    
	    { Type.Underwater, new PhysicParams(
		    0.0234375f,
		    0.015625f,
		    0.046875f,
		    3f,
		    1f,
		    0.25f,
		    0.125f,
		    0.0234375f,
		    0.0234375f,
		    -2f,
		    -3.5f
	    )},
	    
	    { Type.UnderwaterSuperSonic, new PhysicParams(
		    0.09375f,
		    0.015625f,
		    0.1875f,
		    5f,
		    2f,
		    0.5f,
		    0.125f,
		    0.046875f,
		    0.046875f,
		    -2f,
		    -3.5f
		)},
	    
	    { Type.UnderwaterSuper, new PhysicParams(
		    0.046875f,
		    0.046875f,
		    0.09375f,
		    4f,
		    2f,
		    0.375f,
		    0.125f,
		    0.046875f,
		    0.0234375f,
		    -2f,
		    -3.5f
	    )}
    };

    public static PhysicParams Get(bool isUnderwater, bool isSuper, Types playerType, float itemSpeedTimer)
    {
	    PhysicParams physicParams = ParamsMap[GetType(isUnderwater, isSuper, playerType)];
	    
	    if (playerType == Types.Knuckles)
	    {
		    physicParams.JumpSpeed += 0.5f;
	    }
	    
	    if (itemSpeedTimer > 0 && !isUnderwater)
	    {
		    physicParams.Acceleration = 0.09375f;
		    physicParams.AccelerationAir = 0.1875f;
		    physicParams.Friction = 0.09375f;
		    physicParams.FrictionRoll = 0.046875f;
		    physicParams.AccelerationTop = 12f;
	    }
	    
	    if (SharedData.PlayerPhysics >= PhysicsTypes.SK)
	    {
		    if (isSuper)
		    {
			    physicParams.FrictionRoll = 0.0234375f;
		    }
	    }
	    else if (playerType == Types.Tails)
	    {
		    physicParams.DecelerationRoll = physicParams.Deceleration / 4;
	    }
	    
	    return physicParams;
    }

    private static Type GetType(bool isUnderwater, bool isSuper, Types playerType)
    {
	    byte type = 0;
	    
	    if (isUnderwater)
	    {
		    type += 3;
	    }

	    if (!isSuper) return (Type)type;

	    return (Type)(playerType == Types.Sonic ? type + 2 : type + 1);
    }
}
