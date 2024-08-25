using Godot;

namespace OrbinautFramework3.Framework.ObjectBase;

public interface IMemento
{
    Vector2 Position { get; }

    void Reset();
}
