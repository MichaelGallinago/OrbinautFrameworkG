using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Spawnable.Piece;

public partial class Piece(Texture2D texture, float waitTimer, float gravity, Vector2 speed = default) : CullableNode
{
    public override void _Process(double delta)
    {
        waitTimer -= Scene.Instance.ProcessSpeed;
        if (waitTimer > 0f) return;

        Fall();
    }
    
    public override void _Draw() => DrawTexture(texture, default);

    private void Fall()
    {
        //TODO: fix this
        float halfAcceleration = gravity * Scene.Instance.ProcessSpeed * 0.5f;
        speed.Y += halfAcceleration;
        Position += speed * Scene.Instance.ProcessSpeed;
        speed.Y += halfAcceleration;
    }
}