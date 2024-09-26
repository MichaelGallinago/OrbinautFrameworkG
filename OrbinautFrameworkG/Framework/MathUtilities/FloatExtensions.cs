using System;
using System.Runtime.CompilerServices;

namespace OrbinautFrameworkG.Framework.MathUtilities;

public static class FloatExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float MoveToward(this float from, float to, float delta)
    {
        return Math.Abs(to - from) <= delta ? to : from + Math.Sign(to - from) * delta;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float MoveTowardChecked(this float from, float to, float delta, out bool isFinished)
    {
        if (Math.Abs(to - from) <= delta)
        {
            isFinished = true;
            return to;
        }

        isFinished = false;
        return from + Math.Sign(to - from) * delta;
    }
}
