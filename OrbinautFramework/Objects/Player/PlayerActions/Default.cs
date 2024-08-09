using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

[FsmSourceGenerator.FsmState("Action")]
public struct Default
{
    public PlayerData Data { private get; init; }
    
    public void Perform()
    {
        throw new System.NotImplementedException();
    }
}