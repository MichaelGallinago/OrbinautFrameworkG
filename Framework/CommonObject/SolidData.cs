using Godot;
using Godot.Collections;

public partial class SolidData : GodotObject
{
    [Export] public bool NoBalance;
    [Export] public Vector2I Radius;
    [Export] public Vector2I Offset;
    [Export] public Array<short> HeightMap;
    [Export] public Array<Constants.TouchState> TouchStates;
}
