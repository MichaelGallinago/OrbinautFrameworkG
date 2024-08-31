using Godot;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Player.Data;
using static OrbinautFramework3.Framework.Constants;

namespace OrbinautFramework3.Objects.Common.Spikes;

[GlobalClass]
public partial class SpikesVerticalCollideLogic : SpikesCollideLogic
{
    public override void CollideWithPlayer(ISolid spikes, IPlayer player)
    {
        AttachType attachType = player.Data.Damage.IsInvincible ? AttachType.Default : AttachType.ResetPlayer;
        player.ActSolid(spikes, SolidType.Full, attachType);
    }
}
