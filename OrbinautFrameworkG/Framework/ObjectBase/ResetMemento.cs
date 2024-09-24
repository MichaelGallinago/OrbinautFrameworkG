using Godot;

namespace OrbinautFrameworkG.Framework.ObjectBase;

public class ResetMemento(IResetable originator) : IMemento
{
    public Vector2 Position { get; } = originator.Position;
    
    public void Reset()
    {
        originator.Position = Position;
        originator.Reset();
    }
}
