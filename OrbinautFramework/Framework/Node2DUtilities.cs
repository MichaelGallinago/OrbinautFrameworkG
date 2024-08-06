using Godot;
using OrbinautFramework3.Framework.View;

namespace OrbinautFramework3.Framework;

public static class Node2DUtilities
{
    public static bool IsCameraTarget(this Node2D node, out ICamera camera)
    {
        return Views.Instance.TargetedCameras.TryGetValue(node, out camera);
    }
    
    public static void SetCameraDelayX(this Node2D node, float delay)
    {
        if (!SharedData.CdCamera && node.IsCameraTarget(out ICamera camera))
        {
            camera.SetCameraDelayX(delay);
        }
    }
}
