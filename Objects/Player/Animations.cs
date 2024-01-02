using Meziantou.Framework.Annotations;

[assembly: FastEnumToString(typeof(OrbinautFramework3.Objects.Player.Animations),
    IsPublic = true, ExtensionMethodNamespace = "OrbinautFramework3.Objects.Player.Extensions")]

namespace OrbinautFramework3.Objects.Player;

public enum Animations : byte
{
    None,
    Idle,
    Move,
    Walk,
    Run,
    Dash,
    Spin,
    SpinDash,
    Push,
    Duck,
    LookUp,
    Fly,
    FlyTired,
    Swim,
    SwimTired,
    Hurt,
    Death,
    Drown,
    Skid,
    Grab,
    Balance,
    Transform,
    Bounce,
    Breathe,
    
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
    HammerRush,
    
    FlyLift,
    SwimLift,
}
