using Godot;
using OrbinautFrameworkG.Framework.ObjectBase;
using OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;
using OrbinautFrameworkG.Objects.Player.Data;

namespace OrbinautFrameworkG.Objects.Common.Spikes;

[GlobalClass]
public abstract partial class SpikesCollideLogic : Resource
{
    public abstract void CollideWithPlayer(ISolid spikes, IPlayer playerNode);
}
