using Godot;
using OrbinautFrameworkG.Framework.ObjectBase;
using AbstractTypes_CullableNode = OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes.CullableNode;
using CullableNode = OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes.CullableNode;

namespace OrbinautFrameworkG.Objects.Common.AirBubbler;

public partial class AirBubbler : AbstractTypes_CullableNode
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