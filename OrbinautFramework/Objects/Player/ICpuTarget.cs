using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Player;

public interface ICpuTarget
{
    int ZIndex { get; }
    bool IsDead { get; }
    Vector2 Position { get; }
    Velocity Velocity { get; }
    OrbinautData OnObject { get; }
    Animations Animation { get; }
    AcceleratedValue GroundSpeed { get; }
    bool IsObjectInteractionEnabled { get; }
    ReadOnlySpan<DataRecord> RecordedData { get; }
    
    public bool IsCameraTarget(out ICamera camera);
}
