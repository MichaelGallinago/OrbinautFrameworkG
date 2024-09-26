using System;
using System.Runtime.CompilerServices;
using Godot;

namespace OrbinautFrameworkG.Framework.MathTypes;

public struct AcceleratedVector2
{
    public AcceleratedValue X;
    public AcceleratedValue Y;
    
    public Vector2 ValueDelta => new(X.ValueDelta, Y.ValueDelta);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector2(AcceleratedVector2 vector) => new(vector.X, vector.Y);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator AcceleratedVector2(Vector2 vector) => new(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AcceleratedVector2(Vector2 vector) 
    {
        X = vector.X;
        Y = vector.Y;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AcceleratedVector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResetInstanceValue()
    {
        X.ResetInstantValue();
        Y.ResetInstantValue();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddAcceleration(Vector2 acceleration)
    {
        X.AddAcceleration(acceleration.X);
        Y.AddAcceleration(acceleration.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Modify(Vector2 modificator)
    {
        X.Modify(modificator.X);
        Y.Modify(modificator.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clamp(Vector2 min, Vector2 max)
    {
        X.SetClamp(min.X, max.X);
        Y.SetClamp(min.Y, max.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Min(Vector2 value)
    {
        X.SetMin(value.X);
        Y.SetMin(value.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Max(Vector2 value)
    {
        X.SetMax(value.X);
        Y.SetMax(value.Y);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetDirectionalValue(AcceleratedValue value, float angle)
    {
        float floatValue = value;
        float radians = Mathf.DegToRad(angle);
        X = floatValue * MathF.Cos(radians);
        Y = floatValue * -Mathf.Sin(radians);
        //TODO: check IsAccelerated
        //if (value.IsAccelerated) return; 
        ResetInstanceValue();
    }
}
