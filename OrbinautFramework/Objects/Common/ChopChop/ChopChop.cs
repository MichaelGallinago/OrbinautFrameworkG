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
    public enum State { Roam, Wait, Charge }
    
    private const float MoveDuration = 512f;
    private const float BubbleDuration = 80f;
    
    [Export] private AdvancedAnimatedSprite _sprite;

    public IMemento Memento { get; }
    
    private Vector2 _velocity;
    private float _moveTimer;
    private float _bubbleTimer;
    private State _state;

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
            Vector2 scale = Scale;
            scale.X = -scale.X;
            Scale = scale;
        }
        
        Position = Position with { X = Position.X + _velocity.X * Scene.Instance.Speed };

        StartWaiting();
    }

    private void StartWaiting()
    {
        IPlayer player = null; // TODO: instance_nearest(x, y, obj_player)
        Vector2I dist = (Vector2I)Position - (Vector2I)player.Position;
        int absDistX = Math.Abs(dist.X);

        if (absDistX is < 32 or >= 100 || Math.Abs(dist.Y) >= 32f) return;
        if (MathF.Sign(_velocity.X) == Math.Sign(dist.X)) return;
        
        _state = State.Wait;
        _moveTimer = 16f;
        _velocity.X = 0f;
        _sprite.SetAnimation("Idle", 1);
    }

    private void Wait()
    {
        _moveTimer -= Scene.Instance.Speed;
        if (_moveTimer >= 0f) return;
        
        IPlayer player = null; // TODO: instance_nearest(x, y, obj_player)
        Vector2I dist = (Vector2I)Position - (Vector2I)player.Position;
        
        _velocity.X = MathF.Sign(dist.X) * -2;

        if (MathF.Abs(dist.X) >= 16f)
        {
            _velocity.Y = 0.5f;
        }

        _state = State.Charge;
        Vector2 scale = Scale;
        scale.X = MathF.Sign(_velocity.X);
        Scale = scale;
    }
    
    private void Charge()
    {
        Position += _velocity * Scene.Instance.Speed;
    }
}
