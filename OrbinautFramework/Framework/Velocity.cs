using System;
using Godot;

namespace OrbinautFramework3.Framework;

public class Velocity
{
    private Vector2 _velocity;
    private Vector2 _instantVector;
    
    public static implicit operator Vector2(Velocity velocity) => velocity._velocity;

    public float X
    {
        set => _velocity.X = _instantVector.X = value;
        get => _velocity.X;
    }

    public float Y
    {
        set => _velocity.Y = _instantVector.Y = value;
        get => _velocity.Y;
    }

    public Vector2 Vector
    {
        set => _velocity = _instantVector = value;
        get => _velocity;
    }

    public float AccelerationX
    {
        set => _velocity.X += value * Scene.Instance.ProcessSpeed;
    }
    
    public float AccelerationY
    {
        set => _velocity.Y += value * Scene.Instance.ProcessSpeed;
    }

    public Vector2 Acceleration
    {
        set => _velocity += value * Scene.Instance.ProcessSpeed;
    }

    public void Modify(Vector2 modificator)
    {
        _instantVector += modificator;
        _velocity += modificator;
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

    public static Vector2 GetFromSpeedWithAngle(float speed, float angle)
    {
        (float sine, float cosine) = MathF.SinCos(Mathf.DegToRad(angle));
        return speed * new Vector2(sine, cosine);
    }

    public Vector2 CalculateNewPosition(Vector2 position)
    {
        return position + ((Scene.Instance.ProcessSpeed - 1f) * _instantVector + 
                           (Scene.Instance.ProcessSpeed + 1f) * _velocity) * 0.5f;
    }

    public void SetDirectionalValue(AcceleratedValue value, float angle)
    {
        float radians = Mathf.DegToRad(angle);
        _velocity = value * new Vector2(MathF.Cos(radians), -Mathf.Sin(radians));
        //TODO: check IsAccelerated
        //if (value.IsAccelerated) return;
        _instantVector = _velocity;
    }

    public void ClampX(float min, float max) => ClampAxis(ref _velocity.X, ref _instantVector.X, min, max);
    public void ClampY(float min, float max) => ClampAxis(ref _velocity.Y, ref _instantVector.Y, min, max);
    public void MinX(float value) => MinAxis(ref _velocity.X, ref _instantVector.X, value);
    public void MaxX(float value) => MaxAxis(ref _velocity.X, ref _instantVector.X, value);
    public void MinY(float value) => MinAxis(ref _velocity.Y, ref _instantVector.Y, value);
    public void MaxY(float value) => MaxAxis(ref _velocity.Y, ref _instantVector.Y, value);
    
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
