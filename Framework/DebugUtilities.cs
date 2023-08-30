using Godot;
using System;
using System.Diagnostics;

public static class DebugUtilities
{
    public static void CheckExecutionTime(Action action, string name = "")
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        action();
        stopwatch.Stop();
        GD.Print($"{name}:{stopwatch.ElapsedTicks}");
    }
}
