using Godot;
using System;

public class ObjectRespawnData
{
    public enum BehaviourType : byte
    {
        Active,
        Reset,
        Pause,
        Delete,
        Unique
    }
    
    public BehaviourType Behaviour { get; set; }
    public bool IsVisible { get; }
    public Vector2 Position { get; }
    public Vector2 Scale { get; }
    public int ZIndex { get; }

    public ObjectRespawnData(Vector2 position, Vector2 scale, bool isVisible, int zIndex)
    {
        Behaviour = BehaviourType.Active;
        Position = position;
        Scale = scale;
        IsVisible = isVisible;
        ZIndex = zIndex;
    }
}
