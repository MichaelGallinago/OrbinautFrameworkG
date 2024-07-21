using Godot;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Spawnable.PlayerParticles;

public partial class DropDashDust : BaseObject
{
    [Export] private AnimatedSprite2D _sprite;
    
    public override void _Ready()
    {
        //TODO: setup
        _sprite.AnimationFinished += QueueFree;
    }
}