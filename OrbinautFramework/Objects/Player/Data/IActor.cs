namespace OrbinautFramework3.Objects.Player.Data;

public interface IActor
{
    public int Id { get; set; }
    public Actions.Types ActionType { get; set; }
}
