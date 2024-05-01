namespace OrbinautFramework3.Objects.Player;

public static class PlayerConstants
{
    public const byte MaxDropDashCharge = 22;
    public const float SkidSpeedThreshold = 4f;
    
    public static readonly uint[] ComboScoreValues = [10, 100, 200, 500, 1000, 10000];
}

public enum Types : byte
{
    None, Sonic, Tails, Knuckles, Amy
}
	
public enum SpawnTypes : byte
{
    Global, GlobalAI, Unique, None
}
	
public enum CpuStates : byte
{
    RespawnInit, Respawn, Main, Fly, Stuck
}
    
public enum PhysicsTypes : byte
{
    S1, CD, S2, S3, SK
}

public enum CpuBehaviours : byte
{
    S2, S3
}
    
public enum Actions : byte
{
    None,
    SpinDash,
    PeelOut,
    DropDash,
    DropDashCancel,
    Glide,
    GlideCancel,
    Climb,
    Flight,
    TwinAttack,
    TwinAttackCancel,
    Transform,
    HammerDash,
    HammerSpin,
    HammerSpinCancel,
    Carried
}

public enum GlideStates : byte
{
    Air, Ground, Fall
}
	
public enum ClimbStates : byte
{
    Normal, Ledge
}

public enum RestartStates : byte
{
    GameOver, ResetLevel, RestartStage, RestartGame
}

public enum DeathStates : byte
{
    Wait, Restart
}