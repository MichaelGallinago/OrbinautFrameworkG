using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player;

public interface IPlayerCameraTarget : IPosition
{
    DeathData Death { get; }
    VisualData Visual { get; }
    MovementData Movement { get; }
    IPlayerNode PlayerNode { get; }
    ActionFsm.States State { get; }
    CollisionData Collision { get; }
    
    Vector2 IPosition.Position
    {
        get => PlayerNode.Position;
        set => PlayerNode.Position = value;
    }
}
