using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player.Logic;

public class CarrierFlightLogic(PlayerData data, CarryData carryData) : CharacterFlightLogic(data)
{
    public override bool CheckAscendAllowed() => carryData.Target == null || base.CheckAscendAllowed();
    public override void OnStarted() => carryData.Target = null;
}
