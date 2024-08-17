
using System;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Player;

public interface ICpuTarget : IPosition
{
    int ZIndex { get; }
    bool IsDead { get; }
    SolidBox OnObject { get; }
    Velocity Velocity { get; }
    Animations Animation { get; }
    AcceleratedValue GroundSpeed { get; }
    bool IsObjectInteractionEnabled { get; }
    ReadOnlySpan<DataRecord> RecordedData { get; }
}
