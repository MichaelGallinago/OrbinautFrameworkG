using System;

namespace OrbinautFramework3.Framework;

public struct Timer(float resetTime, Action onReset)
{
    public event Action ResetEventHandler = onReset;
    private float _time;

    public void Update(float value)
    {
        _time += value;
        if (_time < resetTime) return;
        _time %= _time;
        
        ResetEventHandler.Invoke();
    }

    public void Finish() => _time = 0f;
}
