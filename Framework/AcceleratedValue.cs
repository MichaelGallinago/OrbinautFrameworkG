using System;

namespace OrbinautFramework3.Framework;

public struct AcceleratedValue
{
    public float Value
    {
        set => _value = _instantValue = value;
        get => _value;
    }

    private float _value;
    private float _instantValue;
    
    public static implicit operator float(AcceleratedValue value) => value.Value;
    
    public float Acceleration
    {
        set => _value += value * FrameworkData.ProcessSpeed;
    }
    
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
    
    private void Clamp(float min, float max)
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
    
    private void Max(float value)
    {
        if (_value >= value) return;
        Value = value;
    }
    
    private void Min(float value)
    {
        if (_value <= value) return;
        Value = value;
    }
}