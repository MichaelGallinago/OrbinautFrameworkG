using System;
using System.Runtime.CompilerServices;
using Godot;

namespace OrbinautFramework3.Framework;

public struct AcceleratedValue(float value)
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
    public static implicit operator float(AcceleratedValue value) => value._value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator AcceleratedValue(float value) => new(value);
    
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
}
