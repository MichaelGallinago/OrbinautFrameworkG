using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player.Logic;

public class TailsFlightLogic(PlayerData data, CarryData carryData) : CharacterFlightLogic(data)
{
    public override bool CheckAscendAllowed() => carryData.Target == null || base.CheckAscendAllowed();
}
