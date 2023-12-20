using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Input;

namespace OrbinautFramework3.Objects.Player;

public struct RecordedData
{
    public Vector2 Position;
    public Buttons InputPress;
    public Buttons InputDown;
    public bool IsPushing;
    public Constants.DirectionSign DirectionSign;

    public RecordedData(Vector2 position, Buttons inputPress, 
        Buttons inputDown, bool isPushing, Constants.DirectionSign directionSign)
    {
        Position = position;
        InputPress = inputPress;
        InputDown = inputDown;
        IsPushing = isPushing;
        DirectionSign = directionSign;
    }
}