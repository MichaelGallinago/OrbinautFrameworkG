using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Spawnable.Piece;

public partial class Piece(Texture2D texture, float waitTimer, float gravity, Vector2 speed = default) : BaseObject
{
    public override void _Ready() => SetBehaviour(BehaviourType.Delete);
    
    public override void _Draw() => DrawTexture(texture, default);

    public override void _Process(double delta)
    {
        waitTimer -= FrameworkData.ProcessSpeed;
        if (waitTimer > 0f) return;
        
        //TODO: fix this
        float halfAcceleration = gravity * FrameworkData.ProcessSpeed * 0.5f;
        speed.Y += halfAcceleration;
        Position += speed * FrameworkData.ProcessSpeed;
        speed.Y += halfAcceleration;
    }
}