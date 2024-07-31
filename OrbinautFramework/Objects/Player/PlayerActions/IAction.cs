namespace OrbinautFramework3.Objects.Player.PlayerActions;

public interface IAction
{
    Player Player { init; }
    
    void Perform();
}