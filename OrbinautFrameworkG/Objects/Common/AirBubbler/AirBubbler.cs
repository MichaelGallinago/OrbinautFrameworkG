using Godot;
using OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;

namespace OrbinautFrameworkG.Objects.Common.AirBubbler;

public partial class AirBubbler : CullableNode
{
    private int _state;
    private byte _waitTime = SetDelay();
    private int _waitCycle;
    
    private static byte SetDelay() => (byte)(GD.Randi() & sbyte.MaxValue + 128);
}
