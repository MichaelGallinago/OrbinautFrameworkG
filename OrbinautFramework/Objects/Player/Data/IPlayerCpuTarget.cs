using System;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautNode = OrbinautFramework3.Framework.ObjectBase.AbstractTypes.OrbinautNode;

namespace OrbinautFramework3.Objects.Player.Data;

public interface IPlayerCpuTarget : ICpuTarget
{
    DeathData Death { get; }
    IPlayerNode Node { get; }
    VisualData Visual { get; }
    MovementData Movement { get; }
    CollisionData Collision { get; }
    
    int ICpuTarget.ZIndex => Node.ZIndex;
    bool ICpuTarget.IsDead => Death.IsDead;
    Velocity ICpuTarget.Velocity => Movement.Velocity;
    Animations ICpuTarget.Animation => Visual.Animation;
    SolidBox ICpuTarget.OnObject => Collision.OnObject;
    AcceleratedValue ICpuTarget.GroundSpeed => Movement.GroundSpeed;
    bool ICpuTarget.IsObjectInteractionEnabled => Collision.IsObjectInteractionEnabled;
    ReadOnlySpan<DataRecord> ICpuTarget.RecordedData => ;
}