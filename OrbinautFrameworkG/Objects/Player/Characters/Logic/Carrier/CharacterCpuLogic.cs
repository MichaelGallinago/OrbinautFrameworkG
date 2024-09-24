using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Logic;

namespace OrbinautFrameworkG.Objects.Player.Characters.Logic.Carrier;

public class CpuLogic(IPlayerLogic logic, CarryData carryData) : Base.CpuLogic(logic)
{
    public override bool CheckCarry() => carryData.Target != null || base.CheckCarry();
}
