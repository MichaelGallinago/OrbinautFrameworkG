using Godot;
using OrbinautFrameworkG.Framework.StaticStorages;

namespace OrbinautFrameworkG.Framework;

public abstract partial class Trigger : Node2D
{
    protected Trigger()
    {
        Visible = false;
        //TODO: fix memory leak!!!!
        SharedData.SensorDebugToggled += debugType => Visible = debugType != SharedData.SensorDebugTypes.None;
    }
}
