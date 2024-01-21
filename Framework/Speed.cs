using System;
using Godot;

namespace OrbinautFramework3.Framework;

public class Speed
{
    private Vector2 _speed;
    private Vector2 _instantVector;
    
    public static implicit operator Vector2(Speed speed) => speed._speed;

    public float X
    {
        set => _speed.X = _instantVector.X = value;
        get => _speed.X;
    }

    public float Y
    {
        set => _speed.Y = _instantVector.Y = value;
        get => _speed.Y;
    }

    public Vector2 Vector
    {
        set => _speed = _instantVector = value;
        get => _speed;
    }
    
    public float AccelerationX
    {
        set => _speed.X += value * FrameworkData.ProcessSpeed;
    }
    
    public float AccelerationY
    {
        set => _speed.Y += value * FrameworkData.ProcessSpeed;
    }
    
    public Vector2 Acceleration
    {
        set => _speed += value * FrameworkData.ProcessSpeed;
    }

    public void Clamp(Vector2 min, Vector2 max)
    {
        ClampX(min.X, max.X);
        ClampY(min.Y, max.Y);
    }

    public void Min(Vector2 value)
    {
        MinX(value.X);
        MinY(value.Y);
    }
    
    public void Max(Vector2 value)
    {
        MaxX(value.X);
        MaxY(value.Y);
    }

    public Vector2 CalculateNewPosition(Vector2 position)
    {
        return position + ((FrameworkData.ProcessSpeed - 1f) * _instantVector + 
                           (FrameworkData.ProcessSpeed + 1f) * _speed) * 0.5f;
    }

    public void SetDirectionalValue(AcceleratedValue value, float angle)
    {
        float radians = Mathf.DegToRad(angle);
        _speed = value * new Vector2(MathF.Cos(radians), -Mathf.Sin(radians));
        //TODO: check IsAccelerated
        //if (value.IsAccelerated) return;
        _instantVector = _speed;
    }

    public void ClampX(float min, float max) => ClampAxis(ref _speed.X, ref _instantVector.X, min, max);
    public void ClampY(float min, float max) => ClampAxis(ref _speed.Y, ref _instantVector.Y, min, max);
    public void MinX(float value) => MinAxis(ref _speed.Y, ref _instantVector.Y, value);
    public void MaxX(float value) => MaxAxis(ref _speed.X, ref _instantVector.X, value);
    public void MinY(float value) => MinAxis(ref _speed.Y, ref _instantVector.Y, value);
    public void MaxY(float value) => MaxAxis(ref _speed.Y, ref _instantVector.Y, value);

    private static void ClampAxis(ref float axis, ref float instantValue, float min, float max)
    {
        if (min > max)
        {
            throw new ArgumentException("max should be no less than min");
        }
        
        if (axis < min)
        {
            axis = instantValue = min;
        }
        else if (axis > max)
        {
            axis = instantValue = max;
        }
    }

    private static void MaxAxis(ref float axis, ref float instantValue, float value)
    {
        if (axis >= value) return;
        axis = value;
        instantValue = value;
    }
    
    private static void MinAxis(ref float axis, ref float instantValue, float value)
    {
        if (axis <= value) return;
        axis = value;
        instantValue = value;
    }
}
