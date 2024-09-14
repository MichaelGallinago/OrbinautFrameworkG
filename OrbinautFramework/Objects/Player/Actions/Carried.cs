using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Sprite;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Actions;

[FsmSourceGenerator.FsmState("Action")]
public readonly struct Carried(PlayerData data, IPlayerLogic logic)
{
    public void Enter()
    {
        logic.ResetData();
        data.Sprite.Animation = Animations.Grab;
    }
    
    public void Exit(States nextState)
    {
        if (nextState != States.Jump) return;
        
        data.Movement.IsJumping = true;
        data.Collision.Radius = data.Collision.RadiusSpin;

        AcceleratedVector2 acceleratedVector2 = data.Movement.Velocity;
        acceleratedVector2.Y = data.Physics.MinimalJumpSpeed;
        if (data.Input.Down.Left)
        {
            acceleratedVector2.X = -2f;
        }
        else if (data.Input.Down.Right)
        {
            acceleratedVector2.X = 2f;
        }

        AudioPlayer.Sound.Play(SoundStorage.Jump);
    }
}
