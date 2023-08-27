using Godot;
using System;

public struct ObjectRespawnData
{
    public Constants.ProcessType ProcessType;
    public bool IsVisible;
    public Vector2 Position;
    public Vector2 Scale;
    public int Depth;

    public ObjectRespawnData(Vector2 position, Vector2 scale, bool isVisible, int depth)
    {
        ProcessType = Constants.ProcessType.Active;
        Position = position;
        Scale = scale;
        IsVisible = isVisible;
        Depth = depth;
    }
}
