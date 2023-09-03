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
    public int Depth { get; }

    public ObjectRespawnData(Vector2 position, Vector2 scale, bool isVisible, int depth)
    {
        Behaviour = BehaviourType.Active;
        Position = position;
        Scale = scale;
        IsVisible = isVisible;
        Depth = depth;
    }
}
