using System;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Player.Data;

public interface IPlayerCpuTarget : ICpuTarget
{
    IPlayerNode PlayerNode { get; }
    DeathData Death { get; }
    MovementData Movement { get; }
    VisualData Visual { get; }
    CollisionData Collision { get; }
    
    int ICpuTarget.ZIndex => PlayerNode.ZIndex;
    bool ICpuTarget.IsDead => Death.IsDead;
    Velocity ICpuTarget.Velocity => Movement.Velocity;
    Animations ICpuTarget.Animation => Visual.Animation;
    OrbinautNode ICpuTarget.OnObject => Collision.OnObject;
    AcceleratedValue ICpuTarget.GroundSpeed => Movement.GroundSpeed;
    bool ICpuTarget.IsObjectInteractionEnabled => Collision.IsObjectInteractionEnabled;
    ReadOnlySpan<DataRecord> ICpuTarget.RecordedData => ;
}