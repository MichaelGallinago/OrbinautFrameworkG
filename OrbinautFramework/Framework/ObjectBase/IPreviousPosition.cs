using Godot;
using OrbinautFramework3.Framework.MultiTypeDelegate;

namespace OrbinautFramework3.Framework.ObjectBase;

public interface IPreviousPosition : ITypeDelegate
{
    void ITypeDelegate.Invoke() => PreviousPosition = Position;
    
    Vector2 PreviousPosition { get; set; }
    Vector2 Position { get; }
}
