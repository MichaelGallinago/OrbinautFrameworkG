using Godot;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Framework;

public interface ICullable
{
    public enum Types : byte { None, NoBounds, Reset, ResetX, ResetY, Delete, Pause }
    
    public Types CullingType { get; }
    public Vector2 Position { get; }
    public IMemento Memento => null;
    
    public void SetProcess(bool enable);
    public void QueueFree();
    public void Hide();
}
