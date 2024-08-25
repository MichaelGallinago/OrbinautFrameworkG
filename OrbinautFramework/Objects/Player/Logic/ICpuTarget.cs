using System;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.Logic;

public interface ICpuTarget : IPosition
{
    int ZIndex { get; }
    bool IsDead { get; }
    ISolid OnObject { get; }
    Velocity Velocity { get; }
    Animations Animation { get; }
    AcceleratedValue GroundSpeed { get; }
    bool IsObjectInteractionEnabled { get; }
    ReadOnlySpan<DataRecord> RecordedData { get; }
}
