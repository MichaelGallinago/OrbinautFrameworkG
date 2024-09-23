using System;
using OrbinautFramework3.Framework.StaticStorages;

namespace OrbinautFramework3.Framework;

public static class DeltaTimeUtilities
{
    public static float CalculateSpeed(double deltaTime)
    {
        return Settings.TargetFps is <= Constants.BaseFrameRate and > 0 ? 
            1f : Math.Min(1f, (float)(deltaTime * Constants.BaseFrameRate));
    }
}