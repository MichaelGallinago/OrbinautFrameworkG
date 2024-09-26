using Godot;
using OrbinautFrameworkG.Framework.ObjectBase;

namespace OrbinautFrameworkG.Objects.Player.Logic;

public interface IPlayerNodePosition : IPosition, IPlayerDataStorage
{
    Vector2 IPosition.Position
    {
        get => Data.Node.Position;
        set => Data.Node.Position = value;
    }
}
