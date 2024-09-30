using Godot;
using OrbinautFrameworkG.Framework.SceneModule;

namespace OrbinautFrameworkG.Objects.Common;

public abstract partial class Trigger : Node2D
{
    protected Trigger()
    {
        Visible = false;
        Debug.SensorDebugToggled += ChangeVisibility;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Debug.SensorDebugToggled -= ChangeVisibility;
    }
    
    private void ChangeVisibility(Debug.SensorTypes type) => Visible = type != Debug.SensorTypes.None;
}
