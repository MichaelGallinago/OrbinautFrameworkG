using System;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.Logic;

public interface IPlayerCpuTarget : ICpuTarget, IPlayerDataStorage, IRecorderStorage
{
    int ICpuTarget.ZIndex => Data.Node.ZIndex;
    bool ICpuTarget.IsDead => Data.Death.IsDead;
    Velocity ICpuTarget.Velocity => Data.Movement.Velocity;
    ISolid ICpuTarget.OnObject => Data.Collision.OnObject;
    Animations ICpuTarget.Animation => Data.Sprite.Animation;
    AcceleratedValue ICpuTarget.GroundSpeed => Data.Movement.GroundSpeed;
    ReadOnlySpan<DataRecord> ICpuTarget.RecordedData => Recorder.RecordedData;
    bool ICpuTarget.IsObjectInteractionEnabled => Data.Collision.IsObjectInteractionEnabled;
}
