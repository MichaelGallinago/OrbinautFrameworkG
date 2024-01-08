using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Input;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Player;

public struct RecordedData(
    Vector2 position,
    Buttons inputPress,
    Buttons inputDown,
    BaseObject pushingObject,
    Constants.Direction facing)
{
    public Vector2 Position = position;
    public Buttons InputPress = inputPress;
    public Buttons InputDown = inputDown;
    public readonly BaseObject PushingObject = pushingObject;
    public readonly Constants.Direction Facing = facing;
}