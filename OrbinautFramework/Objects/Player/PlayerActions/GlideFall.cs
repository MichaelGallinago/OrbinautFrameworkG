using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

[FsmSourceGenerator.FsmState("Action")]
public struct GlideFall(PlayerData data)
{
    public void Enter()
    {
        data.Sprite.Animation = Animations.GlideFall;
        data.Collision.Radius = data.Collision.RadiusNormal;
		
        data.ResetGravity();
    }
}
