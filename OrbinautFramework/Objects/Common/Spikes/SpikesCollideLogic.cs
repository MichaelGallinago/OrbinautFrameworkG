using Godot;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Common.Spikes;

[GlobalClass]
public abstract partial class SpikesCollideLogic : Resource
{
    public abstract void CollideWithPlayer(ISolid spikes, IPlayer playerNode);
}
