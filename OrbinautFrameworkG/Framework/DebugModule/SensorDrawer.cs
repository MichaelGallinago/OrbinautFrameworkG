using System.Collections.Generic;
using Godot;

namespace OrbinautFrameworkG.Framework.DebugModule;

public readonly struct SensorDrawer<T>(Node2D canvas, List<Rect2> sensors) where T : ISensorDrawLogic, new()
{
    private readonly T _drawLogic = new T(canvas);
    
    private void DrawSensors()
    {
        foreach (Rect2 sensor in sensors)
        { 
            _drawLogic.DrawSensor(sensor);
        }
    }
}
