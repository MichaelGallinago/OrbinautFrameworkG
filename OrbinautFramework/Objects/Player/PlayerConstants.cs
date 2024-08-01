namespace OrbinautFramework3.Objects.Player;

public static class PlayerConstants
{
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
    
public enum PhysicsTypes : byte
{
    S1, CD, S2, S3, SK
}

public enum RestartStates : byte
{
    GameOver, ResetLevel, RestartStage, RestartGame
}

public enum DeathStates : byte
{
    Wait, Restart
}
