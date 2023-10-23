using System;
using System.Diagnostics;
using Godot;

namespace OrbinautFramework3.Framework;

public static class DebugUtilities
{
    public static void CheckExecutionTime(Action action, string name = "")
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        action();
        stopwatch.Stop();
        GD.Print($"{name}:{stopwatch.ElapsedTicks} ({stopwatch.Elapsed})");
    }
}