using Godot;
using System;

public struct RecordedData
{
    public Vector2 Position;
    public Buttons InputPress;
    public Buttons InputDown;
    public bool IsPushing;
    public Constants.Direction Direction;

    public RecordedData(Vector2 position, Buttons inputPress, 
        Buttons inputDown, bool isPushing, Constants.Direction direction)
    {
        Position = position;
        InputPress = inputPress;
        InputDown = inputDown;
        IsPushing = isPushing;
        Direction = direction;
    }
}
