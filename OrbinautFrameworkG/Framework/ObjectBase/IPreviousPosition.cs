using Godot;
using OrbinautFrameworkG.Framework.MultiTypeDelegate;

namespace OrbinautFrameworkG.Framework.ObjectBase;

public interface IPreviousPosition : ITypeDelegate
{
    void ITypeDelegate.Invoke() => PreviousPosition = Position;
    
    Vector2 PreviousPosition { get; set; }
    Vector2 Position { get; }
}
