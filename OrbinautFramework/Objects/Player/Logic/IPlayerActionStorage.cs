namespace OrbinautFramework3.Objects.Player.Logic;

public interface IPlayerActionStorage
{
    ActionFsm.States Action { get; set; }
}
