using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

public interface IAction
{
    PlayerData Data { init; }
}
