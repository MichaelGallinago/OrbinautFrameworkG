using System.Runtime.CompilerServices;
using Godot;

namespace OrbinautFrameworkG.Framework.MathUtilities;

public static class VectorUtilities
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 FlipX(Vector2 vector)
    {
        vector.X = -vector.X;
        return vector;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 FlipY(Vector2 vector)
    {
        vector.Y = -vector.Y;
        return vector;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 AddX(Vector2 vector, float value)
    {
        vector.X += value;
        return vector;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 AddY(Vector2 vector, float value)
    {
        vector.Y += value;
        return vector;
    }
}