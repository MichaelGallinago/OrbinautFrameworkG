using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Animations;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.ObjectBase.AbstractTypes;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Common.ChopChop;

public partial class ChopChop : InteractiveNode, IResetable
{
    public enum State : byte { Roam, Wait, Charge }
    
    private const float MoveDuration = 512f;
    private const float BubbleDuration = 80f;
    
    [Export] private AdvancedAnimatedSprite _sprite;

    public IMemento Memento { get; }
    
    private Vector2 _velocity;
    private float _moveTimer;
    private float _bubbleTimer;
    private State _state;

    private static readonly StringName IdleAnimation = "Idle";
    
    public ChopChop()
    {
        Memento = new ResetMemento(this);
        Reset();
    }
    
    public void Reset()
    {
        _state = State.Roam;
        _moveTimer = 0f;
        _bubbleTimer = 0f;
        _velocity = new Vector2(0.25f * MathF.Sign(Scale.X), 0);
    }

    public override void _Process(double delta)
    {
        if (false) return; // TODO: obj_act_enemy

        switch (_state)
        {
            case State.Roam: Roam(); break;
            case State.Wait: Wait(); break;
            case State.Charge: Charge(); break;
        }
    }
    
    private void Roam()
    {
        _bubbleTimer -= Scene.Instance.Speed;
        if (_bubbleTimer <= 0f)
        {
            _bubbleTimer = BubbleDuration;
            // TODO: instance_create(x + 20 * image_xscale, y + 6, obj_bubble, { BubbleType: BUBBLE_TYPE_SMALL })
        }
        
        _moveTimer -= Scene.Instance.Speed;
        if (_moveTimer <= 0f)
        {
            _moveTimer = MoveDuration;
            _velocity.X = -_velocity.X;
            Scale = VectorUtilities.FlipX(Scale);
        }
        
        Vector2 position = Position;
        position.X += _velocity.X * Scene.Instance.Speed;
        Position = position;

        StartWaiting(position);
    }

    private void StartWaiting(Vector2 position)
    {
        IPlayer player = Scene.Instance.Players.FindNearest(position);
        Vector2I distance = (Vector2I)position - (Vector2I)player.Position;
        int absDistanceX = Math.Abs(distance.X);

        if (absDistanceX is < 32 or >= 100 || Math.Abs(distance.Y) >= 32f) return;
        if (MathF.Sign(_velocity.X) == Math.Sign(distance.X)) return;
        
        _state = State.Wait;
        _moveTimer = 16f;
        _velocity.X = 0f;
        _sprite.PlayAnimation(IdleAnimation, 1);
    }

    private void Wait()
    {
        _moveTimer -= Scene.Instance.Speed;
        if (_moveTimer >= 0f) return;
        
        Vector2 position = Position;
        IPlayer player = Scene.Instance.Players.FindNearest(position);
        Vector2I distance = (Vector2I)position - (Vector2I)player.Position;
        
        _velocity.X = MathF.Sign(distance.X) * -2;

        if (MathF.Abs(distance.X) >= 16f)
        {
            _velocity.Y = 0.5f;
        }

        _state = State.Charge;
        Vector2 scale = Scale;
        scale.X = MathF.Sign(_velocity.X);
        Scale = scale;
    }
    
    private void Charge() => Position += _velocity * Scene.Instance.Speed;
}
