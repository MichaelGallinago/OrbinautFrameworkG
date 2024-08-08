namespace OrbinautFramework3.Objects.Player.Data;

public interface IPlayerData
{
    public int Id { get; set; }
    ActionFsm.States State { get; }
}
