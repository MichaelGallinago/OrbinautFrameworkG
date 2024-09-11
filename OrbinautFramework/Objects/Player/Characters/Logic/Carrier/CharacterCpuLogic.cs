using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;

namespace OrbinautFramework3.Objects.Player.Characters.Logic.Carrier;

public class CpuLogic(IPlayerLogic logic, CarryData carryData) : Base.CpuLogic(logic)
{
    public override bool CheckCarry() => carryData.Target != null || base.CheckCarry();
}
