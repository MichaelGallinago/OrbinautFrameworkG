using System;

public static class PlayerConstants
{
    public const byte CpuDelay = 16;
    
    // TODO: PlayerCount
    // byte PlayerCount = instance_number(global.player_obj)
    public enum Type : byte
    {
        Sonic,
        Tails,
        Knuckles,
        Amy,
        Global,
        GlobalAI
    }
    
    public enum State : byte
    {
        Normal,
        Spin,
        Jump,
        SpinDash,
        PeelOut,
        DropDash,
        Flight,
        HammerRush,
        HammerSpin,
        Carried
    }

    public enum CpuState : byte
    {
        Main,
        Fly,
        Stuck
    }
    
    
    public enum PhysicsType : byte
    {
        S1,
        CD,
        S2,
        S3,
        SK
    }
    
    public enum Action : byte
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
        HammerRush,
        HammerSpin,
        Carried,
        ObjectControl
    }

    public enum GlideState : sbyte
    {
        Left = -1,
        Right = 1,
        Ground = 2
    }
    
    public enum Animation : byte
    {
        Idle,
        Move,
        Spin,
        DropDash,
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
        Glide,
        GlideFall,
        GlideGround,
        ClimbWall,
        ClimbLedge,
        Skid,
        Balance,
        BalanceFlip,
        BalancePanic,
        BalanceTurn,
        Bounce,
        Transform,
        Breathe,
        HammerSpin,
        HammerRush,
        FlyLift,
        SwimLift,
        Grab
    }

    public enum RestartState : byte
    {
        GameOver,
        ResetLevel,
        RestartStage,
        RestartGame
    }
}
