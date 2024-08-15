using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player;

public interface IPlayerEditor : IEditor
{
    Vector2 IEditor.Position
    {
        get => Node.Position; 
        set => Node.Position = value;
    }
    
    Constants.Direction IEditor.Facing => Visual.Facing;

    void IEditor.OnEnableEditMode()
    {
        ResetGravity();
        ResetState();
        //ResetZIndex(); //TODO: ResetZIndex

        Node.Visible = true;
        Collision.IsObjectInteractionEnabled = false;
    }

    void IEditor.OnDisableEditMode()
    {
        Movement.Velocity.Vector = Vector2.Zero;
        Movement.GroundSpeed.Value = 0f;
        Visual.Animation = Animations.Move;
        Collision.IsObjectInteractionEnabled = true;
        Death.State = Modules.Death.States.Wait;
    }
    
    DeathData Death { get; }
    IPlayerNode Node { get; }
    VisualData Visual { get; }
    MovementData Movement { get; }
    CollisionData Collision { get; } 

    void ResetGravity();
    void ResetState();
}