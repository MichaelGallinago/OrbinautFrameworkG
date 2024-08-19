using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player.Logic;

public interface IPlayerDataStorage
{
    PlayerData Data { get; }
}