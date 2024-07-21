using System;
using System.Runtime.CompilerServices;

namespace OrbinautFramework3.Framework;

public static class MathfExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float MoveToward(this float from, float to, float delta)
    {
        return Math.Abs(to - from) <= delta ? to : from + Math.Sign(to - from) * delta;
    }
}