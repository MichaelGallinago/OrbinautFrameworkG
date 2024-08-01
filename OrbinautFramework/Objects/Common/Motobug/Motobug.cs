using Godot;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Common.Motobug;

public partial class Motobug : OrbinautData
{
    [Export] private AnimatedSprite2D _sprite;
    private Vector2 _velocity;
    private float _smokeTimer;
    private float _moveTimer;

    public override void _Ready()
    {
        base._Ready();
        SetHitBox(20, 14);
    }

    protected override void Init()
    {
        Visible = false;
        _smokeTimer = 0f;
        _moveTimer = 0f;
        _velocity = Vector2.Zero;
    }
}