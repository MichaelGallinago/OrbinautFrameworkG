using OrbinautFrameworkG.Objects.Player.Logic;

namespace OrbinautFrameworkG.Objects.Player.Characters.Logic.Base;

public class CpuLogic(IPlayerLogic logic)
{
    public virtual bool CheckCarry() => logic.Action == ActionFsm.States.Carried;
}
