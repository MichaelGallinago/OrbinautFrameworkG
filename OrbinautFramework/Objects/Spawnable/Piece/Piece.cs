using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using Scene = OrbinautFramework3.Scenes.Scene;

namespace OrbinautFramework3.Objects.Spawnable.Piece;

public partial class Piece(Texture2D texture, float waitTimer, float gravity, Vector2 speed = default) : BaseObject
{
    public override void _Ready() => Culling = CullingType.Delete;
    
    public override void _Draw() => DrawTexture(texture, default);

    public override void _Process(double delta)
    {
        waitTimer -= Scene.Speed;
        if (waitTimer > 0f) return;
        
        //TODO: fix this
        float halfAcceleration = gravity * Scene.Speed * 0.5f;
        speed.Y += halfAcceleration;
        Position += speed * Scene.Speed;
        speed.Y += halfAcceleration;
    }
}