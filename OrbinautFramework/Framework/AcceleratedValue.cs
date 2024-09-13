using System;
using Godot;

namespace OrbinautFramework3.Framework;

public class AcceleratedValue
{
    public float Value
    {
        set => _value = _instantValue = value;
        get => _value;
    }

    private float _value;
    private float _instantValue;
    
    public static implicit operator float(AcceleratedValue value) => value.Value;
    
    public void AddAcceleration(float acceleration) => _value += acceleration * Scene.Instance.Speed;
    
    public bool IsAccelerated => !Mathf.IsEqualApprox(_value, _instantValue);
    
    public static float operator+ (float value, AcceleratedValue acceleratedValue) => acceleratedValue.Sum(value);
    public static float operator+ (AcceleratedValue acceleratedValue, float value) => acceleratedValue.Sum(value);
    
    public void ResetInstantValue() => _instantValue = _value;
    
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
    
    public void SetMax(float value)
    {
        if (_value >= value) return;
        Value = value;
    }
    
    public void SetMin(float value)
    {
        if (_value <= value) return;
        Value = value;
    }
    
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
    
    private float Sum(float value)
    {
        float speed = Scene.Instance.Speed;
        return value + ((speed - 1f) * _instantValue + (speed + 1f) * _value) * 0.5f;
    }
}
