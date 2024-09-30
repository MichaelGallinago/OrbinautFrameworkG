using Godot;
using OrbinautFrameworkG.Framework.MultiTypeDelegate;

namespace OrbinautFrameworkG.Framework.ObjectBase;

public interface IPreviousPosition : ITypeDelegate, IPosition
{
    void ITypeDelegate.Invoke() => PreviousPosition = Position;
    
    Vector2 PreviousPosition { get; set; }
}
