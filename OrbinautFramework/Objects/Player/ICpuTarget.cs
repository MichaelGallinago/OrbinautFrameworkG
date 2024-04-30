using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.View;

namespace OrbinautFramework3.Objects.Player;

public interface ICpuTarget
{
    bool IsDead { get; }
    Velocity Velocity { get; }
    Actions Action { get; }
    Vector2 Position { get; }
    AcceleratedValue GroundSpeed { get; }
    BaseObject OnObject { get; }
    bool IsObjectInteractionEnabled { get; }
    ICamera Camera { get; }
    int ZIndex { get; }
    
    ReadOnlySpan<DataRecord> RecordedData { get; }
    DataRecord GetFollowDataRecord(int cpuDelay) => RecordedData[cpuDelay];
}