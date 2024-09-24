using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Logic;
using OrbinautFrameworkG.Objects.Player.Sprite;
using static OrbinautFrameworkG.Objects.Player.ActionFsm;

namespace OrbinautFrameworkG.Objects.Player.Actions;

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

        AcceleratedVector2 acceleratedVector2 = data.Movement.Velocity; //TODO: AcceleratedVector2
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
