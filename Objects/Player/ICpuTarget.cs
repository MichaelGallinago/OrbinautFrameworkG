using System.Collections.Generic;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Player;

public interface ICpuTarget
{
    public const int CpuDelay = 16;
    
    bool IsDead { get; }
    Velocity Velocity { get; }
    Actions Action { get; }
    Vector2 Position { get; }
    AcceleratedValue GroundSpeed { get; }
    BaseObject OnObject { get; }
    bool ObjectInteraction { get; }
    
    List<RecordedData> RecordedData { protected get; init; }
    RecordedData FollowData => RecordedData.Count >= CpuDelay ? RecordedData[^CpuDelay] : default;
}