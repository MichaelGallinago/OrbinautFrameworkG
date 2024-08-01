namespace OrbinautFramework3.Objects.Player.PlayerActions;

public struct None : IAction
{
    public Player Player { private get; init; }

    public void Perform() {}
}
