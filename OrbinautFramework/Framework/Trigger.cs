using Godot;

namespace OrbinautFramework3.Framework;

public abstract partial class Trigger : Node2D
{
    protected Trigger()
    {
        Visible = false;
        //TODO: fix memory leak!!!!
        SharedData.SensorDebugToggled += debugType => Visible = debugType != SharedData.SensorDebugTypes.None;
    }
}
