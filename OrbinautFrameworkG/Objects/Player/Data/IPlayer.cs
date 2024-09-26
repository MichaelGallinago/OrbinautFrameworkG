using Godot;
using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.SceneModule;
using OrbinautFrameworkG.Framework.View;
using OrbinautFrameworkG.Objects.Player.Logic;
using OrbinautFrameworkG.Objects.Player.Sprite;

namespace OrbinautFrameworkG.Objects.Player.Data;

public interface IPlayer : IPlayerEditor, IPlayerCarryTarget
{
    void IEditor.OnEnableDebugMode()
    {
        Data.Visual.Visible = true;
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
        movement.GroundSpeed = 0f;
        movement.Velocity = Vector2.Zero;
        
        Data.State = PlayerStates.Control;
        Data.Sprite.Animation = Animations.Move;
        Data.Water.IsUnderwater = false;

        Data.Movement.Position = (Vector2I)Data.Movement.Position;
        
        ResetGravity();
        ResetData();
        Action = ActionFsm.States.Default;
        //TODO: obj_reset_priority();
    }
}
