using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Player.Data;

public interface IPlayer : IPlayerLogic, ICpuTarget, IEditor
{
    PlayerData Data { get; }
    
    Vector2 IPosition.Position
    {
        get => Data.Node.Position;
        set => Data.Node.Position = value;
    }
    
    Constants.Direction IEditor.Facing => Data.Visual.Facing;

    void IEditor.OnEnableEditMode()
    {
        ResetGravity();
        ResetState();
        Action = ActionFsm.States.Default;
        //ResetZIndex(); //TODO: ResetZIndex

        Data.Node.Visible = true;
        Data.Collision.IsObjectInteractionEnabled = false;
    }

    void IEditor.OnDisableEditMode()
    {
        Data.Movement.Velocity.Vector = Vector2.Zero;
        Data.Movement.GroundSpeed.Value = 0f;
        Data.Visual.Animation = Animations.Move;
        Data.Collision.IsObjectInteractionEnabled = true;
        Data.Death.State = Modules.Death.States.Wait;
    }
    
    int ICpuTarget.ZIndex => Data.Node.ZIndex;
    bool ICpuTarget.IsDead => Data.Death.IsDead;
    Velocity ICpuTarget.Velocity => Data.Movement.Velocity;
    SolidBox ICpuTarget.OnObject => Data.Collision.OnObject;
    Animations ICpuTarget.Animation => Data.Visual.Animation;
    AcceleratedValue ICpuTarget.GroundSpeed => Data.Movement.GroundSpeed;
    ReadOnlySpan<DataRecord> ICpuTarget.RecordedData => Recorder.RecordedData;
    bool ICpuTarget.IsObjectInteractionEnabled => Data.Collision.IsObjectInteractionEnabled;
}
