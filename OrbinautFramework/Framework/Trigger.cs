using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Framework;

public abstract partial class Trigger : BaseObject
{
    protected Trigger()
    {
        Visible = false;
        SharedData.SensorDebugToggled += debugType => Visible = debugType != SharedData.SensorDebugTypes.None;
    }
}
