using EnumToStringNameSourceGenerator;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Animations;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.ObjectBase.AbstractTypes;

namespace OrbinautFramework3.Objects.Common.Masher;

public partial class Masher : InteractiveNode, IResetable
{
    public enum JumpVelocity : sbyte { Sonic1 = -5, Sonic2 = -4 }
    
    [EnumToStringName] public enum Animation : byte { Jump, Chomp, Fall }
    
    [Export] private JumpVelocity _jumpVelocity = JumpVelocity.Sonic1;
    [Export] private AdvancedAnimatedSprite _sprite;

    public IMemento Memento { get; }
    
    private readonly AcceleratedValue _velocityY = new ();
    private float _velocityYDefault;
    
    private float _startYPosition;
    private Animation _previousAnimation = Animation.Fall;
    
    public Masher()
    {
        Memento = new ResetMemento(this);
        Reset();
    }
    
    public void Reset()
    {
        _velocityYDefault = (float)_jumpVelocity;
        _velocityY.Value = _velocityYDefault;
    }

    public override void _Ready() => _startYPosition = Position.Y;
    
    public override void _Process(double delta)
    {
        if (false) return; // TODO: obj_act_enemy

        Vector2 position = Position;
        position.Y += _velocityY;
        _velocityY.ResetInstantValue();
        _velocityY.AddAcceleration(0.09375f);

        if (position.Y >= _startYPosition)
        {
            position.Y = _startYPosition;
            _velocityY.Value = _velocityYDefault;
        }

        Animation currentAnimation;
        if (position.Y < _startYPosition - 192f)
        {
            currentAnimation = Animation.Chomp;
        }
        else if (_velocityY >= 0f)
        {
            currentAnimation = Animation.Fall;
        }
        else
        {
            currentAnimation = Animation.Jump;
        }

        if (_previousAnimation == currentAnimation) return;
        
        _previousAnimation = currentAnimation;
        _sprite.Play(currentAnimation.ToStringName());
    }
}