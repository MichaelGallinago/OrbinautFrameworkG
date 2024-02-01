using Godot;

namespace OrbinautFramework3.Framework.ObjectBase;

public class ObjectRespawnData(Vector2 position, Vector2 scale, bool isVisible, int zIndex)
{
    public bool IsVisible { get; } = isVisible;
    public Vector2 Position { get; } = position;
    public Vector2 Scale { get; } = scale;
    public int ZIndex { get; } = zIndex;
}