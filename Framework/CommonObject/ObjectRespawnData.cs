using Godot;
using System;

public class ObjectRespawnData
{
    public bool IsVisible { get; }
    public Vector2 Position { get; }
    public Vector2 Scale { get; }
    public int ZIndex { get; }

    public ObjectRespawnData(Vector2 position, Vector2 scale, bool isVisible, int zIndex)
    {
        Position = position;
        Scale = scale;
        IsVisible = isVisible;
        ZIndex = zIndex;
    }
}
