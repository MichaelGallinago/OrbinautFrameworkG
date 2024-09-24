using Godot;
using OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;

namespace OrbinautFrameworkG.Objects.Spawnable.PlayerParticles;

public partial class DropDashDust : OrbinautNode
{
    [Export] private AnimatedSprite2D _sprite;
    
    //TODO: setup
    public override void _Ready() => _sprite.AnimationFinished += QueueFree;
}
