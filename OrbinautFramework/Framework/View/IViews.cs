using System;
using System.Collections.Generic;
using Godot;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Framework.View;

public interface IViews
{
    event Action<int> OnViewNumberChanged;
    
    byte Number { get; set; }
    ICamera BottomCamera { get; }
    ReadOnlySpan<ICamera> Cameras { get; }
    Dictionary<BaseObject, ICamera> TargetedCameras { get; }

    bool CheckRectInCameras(Rect2 rect);
    void UpdateBottomCamera(ICamera camera);
    ReadOnlySpan<ICamera> GetCamerasWithUpdatedRegions();
}