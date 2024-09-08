using System;
using Godot;

namespace OrbinautFramework3.Framework;

[GlobalClass, Tool]
public partial class LimitedSize : Resource
{
    private static readonly Vector2I SizeLimit = new(ushort.MaxValue, ushort.MaxValue);
    
    [Export] public Vector2I Vector
    {
        get => _vector;
        set => _vector = value.Clamp(Vector2I.Zero, SizeLimit);
    }
    private Vector2I _vector = SizeLimit;

    public int X
    {
        get => _vector.X;
        set => _vector.X = Math.Clamp(value, 0, SizeLimit.X);
    }
    
    public int Y
    {
        get => _vector.Y;
        set => _vector.Y = Math.Clamp(value, 0, SizeLimit.Y);
    }

    public static implicit operator Vector2I(LimitedSize size) => size.Vector;
}
