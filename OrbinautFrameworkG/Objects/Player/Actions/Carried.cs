using Godot;
using OrbinautFrameworkG.Audio.Player;
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
        
        var velocity = new Vector2(0, data.Physics.MinimalJumpSpeed);
        if (data.Input.Down.Left)
        {
            velocity.X = -2f;
        }
        else if (data.Input.Down.Right)
        {
            velocity.X = 2f;
        }
        
        data.Movement.Velocity = velocity;
        
        AudioPlayer.Sound.Play(SoundStorage.Jump);
    }
}
