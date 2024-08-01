using Godot;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Common.AirBubbler;

public partial class AirBubbler : OrbinautData
{
    private int _state;
    private byte _waitTime;
    private int _waitCycle;
    
    public AirBubbler()
    {
        // Variables
        _state = 0;
        _waitTime = SetDelay();
        _waitCycle = 0;
    }
    
    private static byte SetDelay()
    {
        return (byte)(GD.Randi() & sbyte.MaxValue + 128);
    }
}