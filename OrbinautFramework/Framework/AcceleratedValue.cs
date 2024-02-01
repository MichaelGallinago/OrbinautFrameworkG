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
    public static implicit operator AcceleratedValue(float value) => new() { Value = value };
    
    public float Acceleration
    {
        set => _value += value * FrameworkData.ProcessSpeed;
    }

    public bool IsAccelerated => !Mathf.IsEqualApprox(_value, _instantValue);
    
    public float Sum(float value)
    {
        return value + ((FrameworkData.ProcessSpeed - 1f) * _instantValue + 
                        (FrameworkData.ProcessSpeed + 1f) * _value) * 0.5f;
    }

    public float Add(float value)
    {
        float result = Sum(value);
        _instantValue = _value;
        return result;
    }
    
    public void Clamp(float min, float max)
    {
        if (min > max)
        {
            throw new ArgumentException("max should be no less than min");
        }
        
        if (_value < min)
        {
            Value = _instantValue = min;
        }
        else if (_value > max)
        {
            Value = _instantValue = max;
        }
    }
    
    public void Max(float value)
    {
        if (_value >= value) return;
        Value = value;
    }
    
    public void Min(float value)
    {
        if (_value <= value) return;
        Value = value;
    }
}