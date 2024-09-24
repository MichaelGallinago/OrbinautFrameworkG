using OrbinautFrameworkG.Objects.Player.Data;

namespace OrbinautFrameworkG.Objects.Player.Characters.Logic.Base;

public class FlightLogic(PlayerData data)
{
    public virtual bool CheckAscendAllowed() => !data.Water.IsUnderwater;
    public virtual void OnStarted() {}
}
