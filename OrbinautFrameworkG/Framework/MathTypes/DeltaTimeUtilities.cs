using System;
using OrbinautFrameworkG.Framework.StaticStorages;

namespace OrbinautFrameworkG.Framework.MathTypes;

public static class DeltaTimeUtilities
{
    public static float CalculateSpeed(double deltaTime)
    {
        return Settings.TargetFps is <= Constants.BaseFrameRate and > 0 ? 
            1f : Math.Min(1f, (float)(deltaTime * Constants.BaseFrameRate));
    }
}