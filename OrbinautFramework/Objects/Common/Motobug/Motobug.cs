using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.ObjectBase.AbstractTypes;
using OrbinautFramework3.Framework.Tiles;

namespace OrbinautFramework3.Objects.Common.MotoBug;

public partial class MotoBug : InteractiveNode, IResetable
{
    public enum State : byte { Init, Wait, Move }
    
    [Export] private AnimatedSprite2D _sprite;

    public IMemento Memento { get; }
    
    private readonly AcceleratedVector2 _acceleratedVector2 = new();
    private readonly TileCollider _tileCollider = new()
    {
        TileBehaviour = Constants.TileBehaviours.Floor,
        LayerType = Constants.TileLayers.Main
    };
    
    private float _smokeTimer;
    private float _moveTimer;
    private State _state;
    
    public MotoBug()
    {
        Memento = new ResetMemento(this);
        Reset();
    }

    public void Reset()
    {
        _state = State.Init;
        _smokeTimer = 0f;
        _moveTimer = 0f;
        _acceleratedVector2.Vector = Vector2.Zero;
        Visible = false;
    }
    
    public override void _Process(double delta)
    {
        if (false) return; // TODO: obj_act_enemy

        switch (_state)
        {
            case State.Init: Init(); break;
            case State.Wait: Wait(); break;
            case State.Move: Move(); break;
        }
    }
    
    private void Init()
    {
        Vector2 position = Position;
        position.Y = _acceleratedVector2.CalculateNewPositionY(position.Y);
        _acceleratedVector2.SetAccelerationY(GravityType.Default);
        
        _tileCollider.Position = (Vector2I)position;
        int floorDistance = _tileCollider.FindDistance(0, 14, true, Constants.Direction.Positive);
        if (floorDistance < 0f)
        {
            position.Y += floorDistance;
            _acceleratedVector2.Y = 0f;
            _state = State.Wait;
            Scale = VectorUtilities.FlipX(Scale);
        }
        Position = position;
    }

    private void Wait()
    {
        _moveTimer -= Scene.Instance.Speed;
        if (_moveTimer >= 0f) return;
        
        _sprite.Play();
        _acceleratedVector2.X = MathF.Sign(Scale.X);
        _state = State.Move;
        Scale = VectorUtilities.FlipX(Scale);
        Visible = true;
    }
    
    private void Move()
    {
        Vector2 position = Position;
        position.X = _acceleratedVector2.CalculateNewPositionX(position.X);
        
        _tileCollider.Position = (Vector2I)position;
        int floorDistance = _tileCollider.FindDistance(0, 14, true, Constants.Direction.Positive);
        if (floorDistance is > 11 or < -8)
        {
            _state = State.Wait;
            _moveTimer = 59f;
            _acceleratedVector2.X = -_acceleratedVector2.X;
            _sprite.Frame = 0;
            _sprite.Stop();
            Position = position;
            return;
        }
        
        position.Y += floorDistance;
        Position = position;
        
        _smokeTimer -= Scene.Instance.Speed;
        if (_smokeTimer < 0f)
        {
            _smokeTimer = 15f;
            // TODO: instance_create(x + 19 * image_xscale, y, obj_motobug_smoke);
        }
    }
}
