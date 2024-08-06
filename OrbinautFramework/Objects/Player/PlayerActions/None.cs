using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

public struct None : IAction
{
    public PlayerData Data { private get; init; }

    public void Perform() {}
}
