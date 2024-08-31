using Godot;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Framework;

public interface ICullable
{
    public enum Types : byte { None, PauseOnly, Active, Remove, Disable, OriginDisable, Respawn, OriginRespawn }
    
    public Types CullingType { get; set; }
    public Vector2 Position { get; }
    public IMemento Memento => null;
    
    public void SetProcess(bool enable);
    public void QueueFree();
    public void Hide();
}
