using Godot;
using OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;
using OrbinautFrameworkG.Framework.SceneModule;

namespace OrbinautFrameworkG.Objects.Spawnable.Piece;

public partial class Piece(Texture2D texture, float waitTimer, float gravity, Vector2 speed = default) : CullableNode
{
    private float _waitTimer = waitTimer;
    private Vector2 _speed = speed;

    public override void _Process(double delta)
    {
        _waitTimer -= Scene.Instance.Speed;
        if (_waitTimer > 0f) return;

        Fall();
    }
    
    public override void _Draw() => DrawTexture(texture, default);

    private void Fall()
    {
        //TODO: fix this
        float halfAcceleration = gravity * Scene.Instance.Speed * 0.5f;
        _speed.Y += halfAcceleration;
        Position += _speed * Scene.Instance.Speed;
        _speed.Y += halfAcceleration;
    }
}