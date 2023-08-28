using Godot;
using System.Collections.Generic;

public struct SolidData
{
    public bool NoBalance;
    public Vector2I Radius;
    public Vector2I Offset;
    public short[] HeightMap;
    public List<Constants.TouchState> TouchStates;

    public SolidData()
    {
        NoBalance = false;
        Radius = default;
        Offset = default;
        HeightMap = null;
        TouchStates = new List<Constants.TouchState>();
    }
}
