using Godot;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Player.Data;

public interface IPlayerData : IPosition
{
    int Id { get; }
    IPlayerNode Node { get; }

    Vector2 IPosition.Position
    {
        get => Node.Position;
        set => Node.Position = value;
    }
}
