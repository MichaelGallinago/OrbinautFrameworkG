using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player.Characters.Logic.Base;

public class FlightLogic(PlayerData data)
{
    public virtual bool CheckAscendAllowed() => !data.Water.IsUnderwater;
    public virtual void OnStarted() {}
}
