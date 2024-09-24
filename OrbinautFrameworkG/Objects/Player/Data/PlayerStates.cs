namespace OrbinautFrameworkG.Objects.Player.Data;

public enum PlayerStates : byte
{
    Control, Hurt, NoControl, Death, DebugMode, Respawn
}

public static class PlayerStatesUtilities
{
    public static bool IsObjectInteractable(this PlayerStates state)
    {
        return state is PlayerStates.Control or PlayerStates.Hurt or PlayerStates.NoControl;
    }
}
