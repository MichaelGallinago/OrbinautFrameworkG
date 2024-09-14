using System;
using System.Runtime.CompilerServices;
using Godot;

namespace OrbinautFramework3.Framework;

public class AcceleratedVector2
{
    private const int AxisX = 0;
    private const int AxisY = 1;
    
    private Vector2 _velocity;
    private Vector2 _instantVector;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector2(AcceleratedVector2 acceleratedVector2) => acceleratedVector2._velocity;

    public float X
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _velocity.X = _instantVector.X = value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _velocity.X;
    }

    public float Y
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _velocity.Y = _instantVector.Y = value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _velocity.Y;
    }

    public Vector2 Vector
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _velocity = _instantVector = value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _velocity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetAccelerationX(float acceleration) => _velocity.X += acceleration * Scene.Instance.Speed;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetAccelerationY(float acceleration) => _velocity.Y += acceleration * Scene.Instance.Speed;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetAcceleration(Vector2 acceleration) => _velocity += acceleration * Scene.Instance.Speed;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Modify(Vector2 modificator)
    {
        _instantVector += modificator;
        _velocity += modificator;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clamp(Vector2 min, Vector2 max)
    {
        SetClampX(min.X, max.X);
        SetClampY(min.Y, max.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Min(Vector2 value)
    {
        SetMinX(value.X);
        SetMinY(value.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Max(Vector2 value)
    {
        SetMaxX(value.X);
        SetMaxY(value.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 GetFromSpeedWithAngle(float speed, float angle)
    {
        float radians = Mathf.DegToRad(angle);
        float sine = MathF.Sin(radians);
        float cosine = MathF.Cos(radians);
        return speed * new Vector2(sine, cosine);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 CalculateNewPosition(Vector2 position)
    {
        float speed = Scene.Instance.Speed;
        return position + ((speed - 1f) * _instantVector + (speed + 1f) * _velocity) * 0.5f;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float CalculateNewPositionX(float positionX)
    {
        float speed = Scene.Instance.Speed;
        return positionX + ((speed - 1f) * _instantVector.X + (speed + 1f) * _velocity.X) * 0.5f;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float CalculateNewPositionY(float positionY)
    {
        float speed = Scene.Instance.Speed;
        return positionY + ((speed - 1f) * _instantVector.Y + (speed + 1f) * _velocity.Y) * 0.5f;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetDirectionalValue(AcceleratedValue value, float angle)
    {
        float radians = Mathf.DegToRad(angle);
        _velocity = value * new Vector2(MathF.Cos(radians), -Mathf.Sin(radians));
        //TODO: check IsAccelerated
        //if (value.IsAccelerated) return;
        _instantVector = _velocity;
    }

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResetInstantVelocity() => _instantVector = _velocity;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetClampX(float min, float max) => SetClampAxis(AxisX, min, max);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetClampY(float min, float max) => SetClampAxis(AxisY, min, max);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetMinX(float value) => SetMinAxis(AxisX, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetMaxX(float value) => SetMaxAxis(AxisX, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetMinY(float value) => SetMinAxis(AxisY, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetMaxY(float value) => SetMaxAxis(AxisY, value);

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LimitX(float value, Constants.Direction direction)
    {
        if (direction == Constants.Direction.Positive)
        {
            SetMinX(value);
            return;
        }
        
        SetMaxX(value);
    }
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LimitY(float value, Constants.Direction direction)
    {
        if (direction == Constants.Direction.Positive)
        {
            SetMinY(value);
            return;
        }
        
        SetMaxY(value);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetClampAxis(byte index, float min, float max)
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

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetMaxAxis(int index, float value)
    {
        if (_velocity[index] >= value) return;
        _velocity[index] = value;
        _instantVector[index] = value;
    }
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetMinAxis(int index, float value)
    {
        if (_velocity[index] <= value) return;
        _velocity[index] = value;
        _instantVector[index] = value;
    }
}
