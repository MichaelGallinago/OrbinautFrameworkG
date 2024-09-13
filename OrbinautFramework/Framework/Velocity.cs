using System;
using Godot;

namespace OrbinautFramework3.Framework;

public class Velocity
{
    private const byte AxisX = 0;
    private const byte AxisY = 0;
    
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
        set => _velocity.X += value * Scene.Instance.Speed;
    }
    
    public float AccelerationY
    {
        set => _velocity.Y += value * Scene.Instance.Speed;
    }

    public Vector2 Acceleration
    {
        set => _velocity += value * Scene.Instance.Speed;
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
        float radians = Mathf.DegToRad(angle);
        float sine = MathF.Sin(radians);
        float cosine = MathF.Cos(radians);
        return speed * new Vector2(sine, cosine);
    }

    public Vector2 CalculateNewPosition(Vector2 position)
    {
        return position + ((Scene.Instance.Speed - 1f) * _instantVector + 
                           (Scene.Instance.Speed + 1f) * _velocity) * 0.5f;
    }
    public float CalculateNewPositionX(float positionX)
    {
        return positionX + ((Scene.Instance.Speed - 1f) * _instantVector.X + 
                           (Scene.Instance.Speed + 1f) * _velocity.X) * 0.5f;
    }
    public float CalculateNewPositionY(float positionY)
    {
        return positionY + ((Scene.Instance.Speed - 1f) * _instantVector.Y + 
                           (Scene.Instance.Speed + 1f) * _velocity.Y) * 0.5f;
    }

    public void SetDirectionalValue(AcceleratedValue value, float angle)
    {
        float radians = Mathf.DegToRad(angle);
        _velocity = value * new Vector2(MathF.Cos(radians), -Mathf.Sin(radians));
        //TODO: check IsAccelerated
        //if (value.IsAccelerated) return;
        _instantVector = _velocity;
    }

    public void ResetInstantVelocity() => _instantVector = _velocity;

    public void ClampX(float min, float max) => ClampAxis(AxisX, min, max);
    public void ClampY(float min, float max) => ClampAxis(AxisY, min, max);
    public void MinX(float value) => MinAxis(AxisX, value);
    public void MaxX(float value) => MaxAxis(AxisX, value);
    public void MinY(float value) => MinAxis(AxisY, value);
    public void MaxY(float value) => MaxAxis(AxisY, value);

    public void LimitX(float value, Constants.Direction direction)
    {
        if (direction == Constants.Direction.Positive)
        {
            MinX(value);
            return;
        }
        
        MaxX(value);
    }
    
    public void LimitY(float value, Constants.Direction direction)
    {
        if (direction == Constants.Direction.Positive)
        {
            MinY(value);
            return;
        }
        
        MaxY(value);
    }
    
    private void ClampAxis(byte index, float min, float max)
    {
#if TOOLS
        if (min > max)
        {
            throw new ArgumentException("max should be no less than min");
        }
#endif
        
        if (_velocity[index] < min)
        {
            _velocity[index] = _instantVector[index] = min;
        }
        else if (_velocity[index] > max)
        {
            _velocity[index] = _instantVector[index] = max;
        }
    }

    private void MaxAxis(byte index, float value)
    {
        if (_velocity[index] >= value) return;
        _velocity[index] = value;
        _instantVector[index] = value;
    }
    
    private void MinAxis(byte index, float value)
    {
        if (_velocity[index] <= value) return;
        _velocity[index] = value;
        _instantVector[index] = value;
    }
}
