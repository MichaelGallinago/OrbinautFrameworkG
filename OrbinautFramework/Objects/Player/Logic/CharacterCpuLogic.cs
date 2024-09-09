namespace OrbinautFramework3.Objects.Player.Logic;

public class CharacterCpuLogic(IPlayerLogic logic)
{
    public virtual bool CheckCarry() => logic.Action == ActionFsm.States.Carried;
}
