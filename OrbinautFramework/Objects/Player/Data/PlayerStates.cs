namespace OrbinautFramework3.Objects.Player.Data;

public enum PlayerStates : byte
{
    Control, Hurt, NoControl, Death, DebugMode
}

public static class PlayerStatesUtilities
{
    public static bool IsObjectInteractable(this PlayerStates state)
    {
        return state is PlayerStates.Control or PlayerStates.Hurt;
    }
}
