using Godot;
using OrbinautFrameworkG.Framework.ObjectBase;
using OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;
using OrbinautFrameworkG.Objects.Player.Data;
using static OrbinautFrameworkG.Framework.StaticStorages.Constants;

namespace OrbinautFrameworkG.Objects.Common.Spikes;

[GlobalClass]
public partial class SpikesVerticalCollideLogic : SpikesCollideLogic
{
    public override void CollideWithPlayer(ISolid spikes, IPlayer player)
    {
        AttachType attachType = player.Data.Damage.IsInvincible ? AttachType.Default : AttachType.ResetPlayer;
        player.ActSolid(spikes, SolidType.Full, attachType);
    }
}
