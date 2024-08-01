using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Spawnable.Piece;

public partial class Piece(Texture2D texture, float waitTimer, float gravity, Vector2 speed = default) : OrbinautData
{
    public override void _Ready() => Culling = CullingType.Delete;
    
    public override void _Draw() => DrawTexture(texture, default);

    public override void _Process(double delta)
    {
        waitTimer -= Scene.Local.ProcessSpeed;
        if (waitTimer > 0f) return;
        
        //TODO: fix this
        float halfAcceleration = gravity * Scene.Local.ProcessSpeed * 0.5f;
        speed.Y += halfAcceleration;
        Position += speed * Scene.Local.ProcessSpeed;
        speed.Y += halfAcceleration;
    }
}