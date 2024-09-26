using Godot;
using OrbinautFrameworkG.Framework.ObjectBase;
using OrbinautFrameworkG.Framework.StaticStorages;
using OrbinautFrameworkG.Framework.View;
using OrbinautFrameworkG.Objects.Player.Data;

namespace OrbinautFrameworkG.Framework;

public static class Node2DUtilities
{
    public static bool IsInstanceValid(this GodotObject godotObject) => GodotObject.IsInstanceValid(godotObject);
    public static bool IsCameraTarget(this IPosition node, out ICamera camera)
    {
        return Views.Instance.TargetedCameras.TryGetValue(node, out camera);
    }
    
    public static void SetCameraDelayX(this IPosition node, float delay)
    {
        if (!OriginalDifferences.CdCamera && node.IsCameraTarget(out ICamera camera))
        {
            camera.SetCameraDelayX(delay);
        }
    }

    //TODO: check this
    public static bool IsInCamera(this PlayerData data, out ICamera camera)
    {
        if (data.Node.IsCameraTarget(out camera)) return true;
        
        if (data.Cpu.Target != null)
        {
            if (data.Cpu.Target.Data.Node.IsCameraTarget(out camera)) return true;
        }

        return SceneModule.Scene.Instance.Players.FirstOrDefault().Data.Node.IsCameraTarget(out camera);
    }
}
