using Godot;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.Data;

public interface IPlayer : IPlayerLogic, IPlayerCpuTarget, IPlayerEditor, ICarryTarget
{
    Vector2 IPosition.Position
    {
        get => Data.Node.Position;
        set => Data.Node.Position = value;
    }
    
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
        Data.Death.State = Death.States.Wait;
    }
}
