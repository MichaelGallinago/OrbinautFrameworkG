using Godot;
using OrbinautFrameworkG.Framework.ObjectBase;
using OrbinautFrameworkG.Framework;
using AbstractTypes_CullableNode = OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes.CullableNode;
using CullableNode = OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes.CullableNode;

namespace OrbinautFrameworkG.Objects.Spawnable.Piece;

public partial class Piece(Texture2D texture, float waitTimer, float gravity, Vector2 speed = default) : AbstractTypes_CullableNode
{
    public override void _Process(double delta)
    {
        waitTimer -= Scene.Instance.Speed;
        if (waitTimer > 0f) return;

        Fall();
    }
    
    public override void _Draw() => DrawTexture(texture, default);

    private void Fall()
    {
        //TODO: fix this
        float halfAcceleration = gravity * Scene.Instance.Speed * 0.5f;
        speed.Y += halfAcceleration;
        Position += speed * Scene.Instance.Speed;
        speed.Y += halfAcceleration;
    }
}