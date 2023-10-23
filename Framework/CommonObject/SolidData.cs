using System.Collections.Generic;
using Godot;

namespace OrbinautFramework3.Framework.CommonObject;

public class SolidData
{
    public bool NoBalance { get; set; }
    public Vector2I Radius { get; set; }
    public Vector2I Offset { get; set; }
    public short[] HeightMap { get; set; }
    public List<Constants.TouchState> TouchStates { get; set; }
}