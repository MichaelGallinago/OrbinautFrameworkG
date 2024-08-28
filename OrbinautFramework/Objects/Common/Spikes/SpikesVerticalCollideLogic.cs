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
        SolidType solidType = player.Data.Damage.IsInvincible ? SolidType.Full : SolidType.FullReset;
        player.ActSolid(spikes, solidType);
    }
}
