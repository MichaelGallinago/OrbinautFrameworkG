using Meziantou.Framework.Annotations;

[assembly: FastEnumToString(typeof(OrbinautFramework3.Objects.Player.Animations),
    IsPublic = true, ExtensionMethodNamespace = "OrbinautFramework3.Objects.Player.Extensions")]

namespace OrbinautFramework3.Objects.Player;

public enum Animations : byte
{
    None,
    Idle,
    Spin,
    SpinDash,
    Push,
    Duck,
    LookUp,
    Hurt,
    Death,
    Drown,
    Skid,
    Grab,
    Balance,
    Transform,
    Bounce,
    Breathe,
    
    Move,
    Walk,
    Run,
    Dash,
    
    GlideAir,
    GlideFall,
    GlideGround,
    GlideLand,
    ClimbWall,
    ClimbLedge,
    
    DropDash,
    BalanceFlip,
    BalancePanic,
    BalanceTurn,
    
    HammerSpin,
    HammerDash,
    
    Fly,
    FlyTired,
    FlyCarry,
    FlyCarryTired,
    
    Swim,
    SwimTired,
    SwimCarry,
    SwimLift
}
