using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

public struct Default : IAction
{
    public PlayerData Data { private get; init; }
    
    public void Perform()
    {
        throw new System.NotImplementedException();
    }
}