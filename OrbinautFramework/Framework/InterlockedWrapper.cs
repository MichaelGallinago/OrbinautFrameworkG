using System.Threading;

namespace OrbinautFramework3.Framework;

public static class InterlockedWrapper
{
    public static void SetIfGreater(ref int value, int newValue)
    {
        int currentValue;
        do
        {
            currentValue = value;
            if (newValue <= currentValue) return;
        } while (Interlocked.CompareExchange(ref value, newValue, currentValue) != currentValue);
    }
}