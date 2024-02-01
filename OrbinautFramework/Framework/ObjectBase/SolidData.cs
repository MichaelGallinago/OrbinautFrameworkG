using Godot;

namespace OrbinautFramework3.Framework.ObjectBase;

public class SolidData
{
    public bool NoBalance { get; set; }
    public Vector2I Radius { get; set; }
    public Vector2I Offset { get; set; }
    public short[] HeightMap { get; set; }
}