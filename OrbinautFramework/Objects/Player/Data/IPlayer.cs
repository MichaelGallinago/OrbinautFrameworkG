using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.Data;

public interface IPlayer : IPlayerLogic, IPlayerEditor, IPlayerCarryTarget
{
    void IEditor.OnEnableDebugMode()
    {
        Data.Node.Visible = true;
        Data.State = PlayerStates.DebugMode;
        
        if (Data.Node.IsCameraTarget(out ICamera camera))
        {
            camera.IsMovementAllowed = true;
        }
        
        if (AudioPlayer.Music.IsPlaying(MusicStorage.Drowning) || !AudioPlayer.Music.IsAnyPlaying())
        {
            Data.ResetMusic();
        }
    }

    void IEditor.OnDisableDebugMode()
    {
        if (Scene.Instance.State == Scene.States.StopObjects)
        {
            Scene.Instance.State = Scene.States.Normal;
        }
        
        MovementData movement = Data.Movement;
        movement.IsAirLock = false;
        movement.GroundSpeed.Value = 0f;
        movement.Velocity.Vector = Vector2.Zero;
        
        Data.State = PlayerStates.Control;
        Data.Sprite.Animation = Animations.Move;
        Data.Water.IsUnderwater = false;

        Data.Node.Position = (Vector2I)Data.Node.Position;
        
        ResetGravity();
        ResetData();
        Action = ActionFsm.States.Default;
        //TODO: obj_reset_priority();
    }
}
