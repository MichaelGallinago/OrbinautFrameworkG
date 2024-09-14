using System;
using System.Runtime.CompilerServices;
using Godot;

namespace OrbinautFramework3.Framework;

public class AcceleratedValue
{
    public float Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _value = _instantValue = value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _value;
    }
    
    private float _value;
    private float _instantValue;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator float(AcceleratedValue value) => value.Value;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddAcceleration(float acceleration) => _value += acceleration * Scene.Instance.Speed;
    
    public bool IsAccelerated => !Mathf.IsEqualApprox(_value, _instantValue);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float operator+ (float value, AcceleratedValue acceleratedValue) => acceleratedValue.Sum(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float operator+ (AcceleratedValue acceleratedValue, float value) => acceleratedValue.Sum(value);
    
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
            Value = _instantValue = min;
        }
        else if (_value > max)
        {
            Value = _instantValue = max;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetMax(float value)
    {
        if (_value >= value) return;
        Value = value;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetMin(float value)
    {
        if (_value <= value) return;
        Value = value;
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
    private float Sum(float value)
    {
        float speed = Scene.Instance.Speed;
        return value + ((speed - 1f) * _instantValue + (speed + 1f) * _value) * 0.5f;
    }
}
