using EnumToStringNameSourceGenerator;
using Godot;
using OrbinautFrameworkG.Framework.Animations;
using OrbinautFrameworkG.Framework.MathUtilities;
using OrbinautFrameworkG.Framework.ObjectBase;
using OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;

namespace OrbinautFrameworkG.Objects.Badniks.Masher;

public partial class Masher : InteractiveNode, IResetable
{
    public enum JumpVelocity : sbyte { Sonic1 = -5, Sonic2 = -4 }
    
    [EnumToStringName] public enum Animation : byte { Jump, Chomp, Fall }
    
    [Export] private JumpVelocity _jumpVelocity = JumpVelocity.Sonic1;
    [Export] private AdvancedAnimatedSprite _sprite;

    public IMemento Memento { get; }
    
    private AcceleratedValue _velocityY;
    private float _velocityYDefault;
    
    private float _startYPosition;
    private Animation _previousAnimation;
    
    public Masher()
    {
        Memento = new ResetMemento(this);
        Reset();
    }
    
    public void Reset()
    {
        _velocityYDefault = (float)_jumpVelocity;
        _velocityY = _velocityYDefault;
        _previousAnimation = Animation.Fall;
    }

    public override void _Ready() => _startYPosition = Position.Y;
    
    public override void _Process(double delta)
    {
        if (false) return; // TODO: obj_act_enemy

        Vector2 position = Position;
        position.Y += _velocityY.ValueDelta;
        _velocityY.ResetInstantValue();
        _velocityY.AddAcceleration(0.09375f);

        if (position.Y >= _startYPosition)
        {
            position.Y = _startYPosition;
            _velocityY = _velocityYDefault;
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