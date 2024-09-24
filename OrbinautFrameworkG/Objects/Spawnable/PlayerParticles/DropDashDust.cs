using Godot;
using OrbinautFrameworkG.Framework.ObjectBase;
using AbstractTypes_OrbinautNode = OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes.OrbinautNode;
using OrbinautNode = OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes.OrbinautNode;

namespace OrbinautFrameworkG.Objects.Spawnable.PlayerParticles;

public partial class DropDashDust : AbstractTypes_OrbinautNode
{
    [Export] private AnimatedSprite2D _sprite;
    
    //TODO: setup
    public override void _Ready() => _sprite.AnimationFinished += QueueFree;
}
