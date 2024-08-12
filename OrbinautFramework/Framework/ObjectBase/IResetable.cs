using Godot;

namespace OrbinautFramework3.Framework.ObjectBase;

public interface IResetable
{
    Vector2 Position { get; set; }
    void Reset();
}
