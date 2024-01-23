using System.Collections.Generic;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Player;

public interface ICpuTarget
{
    public const int CpuDelay = 16;
    
    int ZIndex { get; }
    bool IsDead { get; }
    Speed Speed { get; }
    Actions Action { get; }
    Vector2 Position { get; }
    AcceleratedValue GroundSpeed { get; }
    BaseObject OnObject { get; }
    bool ObjectInteraction { get; }
    
    List<RecordedData> RecordedData { protected get; init; }
    RecordedData FollowData => RecordedData[^CpuDelay];
}