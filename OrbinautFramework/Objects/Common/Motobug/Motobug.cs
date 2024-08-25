using Godot;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.ObjectBase.AbstractTypes;

namespace OrbinautFramework3.Objects.Common.Motobug;

public partial class Motobug : InteractiveNode, IResetable
{
    [Export] private AnimatedSprite2D _sprite;

    public IMemento Memento { get; }
    
    private Vector2 _velocity;
    private float _smokeTimer;
    private float _moveTimer;

    public Motobug()
    {
        Memento = new ResetMemento(this);
        Reset();
    }

    public void Reset()
    {
        Visible = false;
        _smokeTimer = 0f;
        _moveTimer = 0f;
        _velocity = Vector2.Zero;
    }
}
