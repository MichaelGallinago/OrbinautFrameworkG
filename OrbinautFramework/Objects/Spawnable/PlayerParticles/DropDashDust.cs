using Godot;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Spawnable.PlayerParticles;

public partial class DropDashDust : OrbinautNode
{
    [Export] private AnimatedSprite2D _sprite;
    
    //TODO: setup
    public override void _Ready() => _sprite.AnimationFinished += QueueFree;
}
