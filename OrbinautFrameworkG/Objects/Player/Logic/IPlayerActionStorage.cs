namespace OrbinautFrameworkG.Objects.Player.Logic;

public interface IPlayerActionStorage
{
    ActionFsm.States Action { get; set; }
}
