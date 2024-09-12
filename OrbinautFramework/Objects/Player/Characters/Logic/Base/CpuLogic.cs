using OrbinautFramework3.Objects.Player.Logic;

namespace OrbinautFramework3.Objects.Player.Characters.Logic.Base;

public class CpuLogic(IPlayerLogic logic)
{
    public virtual bool CheckCarry() => logic.Action == ActionFsm.States.Carried;
}
