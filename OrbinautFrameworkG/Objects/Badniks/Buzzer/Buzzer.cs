using System;
using Godot;
using OrbinautFrameworkG.Framework.Animations;
using OrbinautFrameworkG.Framework.MathUtilities;
using OrbinautFrameworkG.Framework.ObjectBase;
using OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;
using OrbinautFrameworkG.Framework.SceneModule;
using OrbinautFrameworkG.Objects.Player.Data;

namespace OrbinautFrameworkG.Objects.Badniks.Buzzer;

public partial class Buzzer : InteractiveNode, IResetable
{
    public enum State : byte { Roam, Shoot }
    
    private const float MoveDuration = 512f;
    
    [Export] private AdvancedAnimatedSprite _sprite;
    
    public IMemento Memento { get; }
    
    private State _state;
    private float _moveTimer;
    private float _turnDelay;
    private float _shotTimer;
    private bool _shootingFlag;
    private float _velocityX;
    
    public Buzzer()
    {
        Memento = new ResetMemento(this);
        Reset();
    }
    
    public void Reset()
    {
        _state = State.Roam;
        _moveTimer = MoveDuration;
        _turnDelay = 0f;
        _shotTimer = 0f;
        _shootingFlag = true;
        _velocityX = -MathF.Sign(Scale.X);
    }
    
    public override void _Process(double delta)
    {
        if (false) return; // TODO: obj_act_enemy

        switch (_state)
        {
            case State.Roam: Roam(); break;
            case State.Shoot: Wait(); break;
        }
    }

    private void Roam()
    {
        Vector2 position = Position;
        Vector2 scale = Scale;
        if (_shootingFlag)
        {
            IPlayer player = Scene.Instance.Players.Values[Scene.Instance.Frame % Scene.Instance.Players.Count];
            int distanceX = (int)position.X - (int)player.Position.X;
            int absDistanceX = Math.Abs(distanceX);

            if (absDistanceX is >= 40 and <= 48 && Math.Sign(distanceX) == MathF.Sign(scale.X))
            {
                _state = State.Shoot;
                _shootingFlag = false;
                _shotTimer = 50f;
                _sprite.Frame = 1;
            }
        }
        
        float previousTurnDelay = _turnDelay;
        _turnDelay -= Scene.Instance.Speed;
        if (_turnDelay >= 0f)
        {
            if (previousTurnDelay >= 15f && _turnDelay <= 15f)
            {
                _moveTimer = MoveDuration;
                _shootingFlag = true;
                _velocityX = -_velocityX;
                Scale = VectorUtilities.FlipX(scale);
            }
            return;
        }
        
        _moveTimer -= Scene.Instance.Speed;
        if (_moveTimer > 0f)
        {
            position.X += _velocityX;
        }
        else
        {
            _turnDelay = 30f;
        }
        
        Position = position;
    }

    private void Wait()
    {
        float previousShotTimer = _shotTimer;
        _shotTimer -= Scene.Instance.Speed;
        if (_shotTimer < 0f)
        {
            _state = State.Roam;
            _sprite.Frame = 0;
        }
        else if (previousShotTimer >= 20f && _shotTimer <= 20f)
        {
            /*TODO: instance_create(x + 5 * image_xscale, y + 26, obj_buzzer_projectile, 
            { 
                VelocityX: -1.5 * image_xscale, 
                VelocityY: 1.5, 
                image_xscale: image_xscale 
            });*/
        }
    }
}