using OrbinautFrameworkG.Objects.Player.Data;

namespace OrbinautFrameworkG.Objects.Player.Characters.Logic.Carrier;

public class FlightLogic(PlayerData data, CarryData carryData) : Base.FlightLogic(data)
{
    public override bool CheckAscendAllowed() => carryData.Target == null || base.CheckAscendAllowed();
    public override void OnStarted() => carryData.Target = null;
}
