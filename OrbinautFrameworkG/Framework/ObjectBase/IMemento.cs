using Godot;

namespace OrbinautFrameworkG.Framework.ObjectBase;

public interface IMemento
{
    Vector2 Position { get; }

    void Reset();
}
