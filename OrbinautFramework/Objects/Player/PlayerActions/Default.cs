using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

[FsmSourceGenerator.FsmState("Action")]
public struct Default(PlayerData data)
{
    public void Perform()
    {
        throw new System.NotImplementedException();
    }
}