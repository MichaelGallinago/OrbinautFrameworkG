using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player.Logic;

public class CharacterFlightLogic(PlayerData data)
{
    public virtual bool CheckAscendAllowed() => !data.Water.IsUnderwater;
    public virtual void OnStarted() {}
}
