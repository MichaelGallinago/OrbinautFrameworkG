using Godot;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Player.Data;
using static OrbinautFramework3.Framework.Constants;

namespace OrbinautFramework3.Objects.Common.Spikes;

[GlobalClass]
public partial class SpikesHorizontalCollideLogic : SpikesCollideLogic
{
    public override void CollideWithPlayer(ISolid spikes, IPlayer player)
    {
        player.ActSolid(spikes, SolidType.Full);
    }
}
