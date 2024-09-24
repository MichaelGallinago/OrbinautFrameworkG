using System;
using System.Runtime.CompilerServices;
using Godot;

namespace OrbinautFrameworkG.Framework;

public struct AcceleratedValue(float value) : IEquatable<AcceleratedValue>
{
    private float _value = value;
    private float _instantValue = value;
    
    public bool IsAccelerated => !Mathf.IsEqualApprox(_value, _instantValue);
    
    public float ValueDelta
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            float speed = Scene.Instance.Speed;
            return ((speed - 1f) * _instantValue + (speed + 1f) * _value) * 0.5f;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddAcceleration(float acceleration) => _value += acceleration * Scene.Instance.Speed;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResetInstantValue() => _instantValue = _value;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetClamp(float min, float max)
    {
#if TOOLS
        if (min > max)
        {
            throw new ArgumentException("max should be no less than min");
        }
#endif
        if (_value < min)
        {
            _value = _instantValue = min;
        }
        else if (_value > max)
        {
            _value = _instantValue = max;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetMax(float value)
    {
        if (_value >= value) return;
        _value = _instantValue = value;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetMin(float value)
    {
        if (_value <= value) return;
        _value = _instantValue = value;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ApplyFriction(float friction)
    {
        int sign = Math.Sign(_value);
        AddAcceleration(-sign * friction);
		
        switch (sign)
        {
            case  1: SetMax(0f); break;
            case -1: SetMin(0f); break;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Limit(float value, Constants.Direction direction)
    {
        if (direction == Constants.Direction.Positive)
        {
            SetMin(value);
            return;
        }
        
        SetMax(value);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Modify(float modificator)
    {
        _instantValue += modificator;
        _value += modificator;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(AcceleratedValue other)
    {
        return _value.Equals(other._value) && _instantValue.Equals(other._instantValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object obj) => obj is AcceleratedValue other && Equals(other);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HashCode.Combine(_value, _instantValue);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(AcceleratedValue left, AcceleratedValue right) => left.Equals(right);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(AcceleratedValue left, AcceleratedValue right) => !(left == right);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator float(AcceleratedValue value) => value._value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator AcceleratedValue(float value) => new(value);
}
