using Godot;
using OrbinautFrameworkG.Framework.DebugModule;

namespace OrbinautFrameworkG.Objects.Common;

public abstract partial class Trigger : Node2D
{
    protected Trigger()
    {
        Visible = false;
        //Debug.Instance.Overlay.SensorDebugToggled += ChangeVisibility;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        //Debug.Instance.Overlay.SensorDebugToggled -= ChangeVisibility;
    }
    
    private void ChangeVisibility(DebugOverlay.SensorTypes type) => Visible = type != DebugOverlay.SensorTypes.None;
}
