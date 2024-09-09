using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player.Logic;

public class CarrierCpuLogic(IPlayerLogic logic, CarryData carryData) : CharacterCpuLogic(logic)
{
    public override bool CheckCarry() => carryData.Target != null || base.CheckCarry();
}
