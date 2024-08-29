using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.Data;

public interface IPlayer : IPlayerLogic, IPlayerEditor, ICarryTarget
{
    Vector2 IPosition.Position
    {
        get => Data.Node.Position;
        set => Data.Node.Position = value;
    }
    
    void IEditor.OnEnableEditMode()
    {
        Data.State = PlayerStates.DebugMode;
        
        if (Data.IsCameraTarget(out ICamera camera))
        {
            camera.IsMovementAllowed = true;
        }
        
        MovementData movement = Data.Movement;
        movement.Velocity.Vector = Vector2.Zero;
        movement.GroundSpeed.Value = 0f;
        movement.IsAirLock = false;
        
        Data.Water.IsUnderwater = false;
        Data.Sprite.Animation = Animations.Move;
        
        ResetGravity();
        ResetData();
        Action = ActionFsm.States.Default;
        
        if (AudioPlayer.Music.IsPlaying(MusicStorage.Drowning))
        {
            Data.ResetMusic();
        }
        
        Data.Node.Visible = true;
        Data.Node.ZIndex = (int)Constants.ZIndexes.AboveForeground; //TODO: RENDERER_DEPTH_HIGHEST
    }

    void IEditor.OnDisableEditMode()
    {
        if (Scene.Instance.State == Scene.States.StopObjects)
        {
            Scene.Instance.State = Scene.States.Normal;
        }
        
        Data.State = PlayerStates.Control;
        //TODO: obj_reset_priority();
    }
}
