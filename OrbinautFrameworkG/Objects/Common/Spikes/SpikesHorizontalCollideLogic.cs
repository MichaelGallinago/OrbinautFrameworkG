using Godot;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.ObjectBase;
using OrbinautFrameworkG.Objects.Player.Data;
using static OrbinautFrameworkG.Framework.Constants;

namespace OrbinautFrameworkG.Objects.Common.Spikes;

[GlobalClass]
public partial class SpikesHorizontalCollideLogic : SpikesCollideLogic
{
    public override void CollideWithPlayer(ISolid spikes, IPlayer player)
    {
        player.ActSolid(spikes, Constants.SolidType.Full);
    }
}
