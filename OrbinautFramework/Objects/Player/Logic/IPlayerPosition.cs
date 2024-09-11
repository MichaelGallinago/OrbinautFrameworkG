using Godot;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Player.Logic;

public interface IPlayerPosition : IPosition, IPlayerDataStorage
{
    Vector2 IPosition.Position
    {
        get => Data.Movement.Position; 
        set => Data.Movement.Position = value;
    }
}
