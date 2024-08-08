using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player;

public interface IPlayerCameraTarget : IPosition
{
    IPlayerNode PlayerNode { get; }
    
    public Actions.Types ActionType { get; }
    DeathData Death { get; }
    PhysicsData Physics { get; }
    CollisionData Collision { get; }
    VisualData Visual { get; }

    Vector2 IPosition.Position
    {
        get => PlayerNode.Position;
        set => PlayerNode.Position = value;
    }
}
