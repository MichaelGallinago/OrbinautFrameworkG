using Meziantou.Framework.Annotations;
using OrbinautFramework3.Objects.Player.Sprite;

[assembly: FastEnumToString(typeof(Animations),
    ExtensionMethodNamespace = "OrbinautFramework3.Objects.Player.Extensions")]

namespace OrbinautFramework3.Objects.Player.Sprite;

public enum Animations : byte
{
    None,
    Push,
    Duck,
    LookUp,
    Hurt,
    Death,
    Drown,
    Skid,
    Grab,
    Transform,
    Bounce,
    Breathe,
    Flip,

    Idle,
    Wait,
    Move,
    Walk,
    Run,
    
    Spin,
    SpinDash,
    Dash,
    DropDash,
    
    GlideAir,
    GlideFall,
    GlideGround,
    GlideLand,
    ClimbWall,
    ClimbLedge,
    
    Balance,
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
