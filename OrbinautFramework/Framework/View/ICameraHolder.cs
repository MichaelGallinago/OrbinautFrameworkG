using Godot;

namespace OrbinautFramework3.Framework.View;

public interface ICameraHolder
{
    public ICamera Camera { get; set; }
    public Vector2 Position { get; }
}