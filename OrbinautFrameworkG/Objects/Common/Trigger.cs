using Godot;
using OrbinautFrameworkG.Framework.SceneModule;

namespace OrbinautFrameworkG.Objects.Common;

public abstract partial class Trigger : Node2D
{
    protected Trigger()
    {
        Visible = false;
        Debug.Instance.SensorDebugToggled += ChangeVisibility;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Debug.Instance.SensorDebugToggled -= ChangeVisibility;
    }
    
    private void ChangeVisibility(Debug.SensorTypes type) => Visible = type != Debug.SensorTypes.None;
}
