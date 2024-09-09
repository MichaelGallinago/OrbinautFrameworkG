using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Actions;

[FsmSourceGenerator.FsmState("Action")]
public readonly struct Carried(PlayerData data, IPlayerLogic logic)
{
    public States OnAttached(ICarrier carrier)
    {
        if (data.Input.Press.Aby)
        {
            carrier.Target = null;
            carrier.Cooldown = 18f;
            Jump();
            return States.Jump;
        }
    	
        var previousPosition = (Vector2I)carrier.TargetPosition;
        if ((Vector2I)data.Node.Position != previousPosition)
        {
            carrier.Target = null;
            carrier.Cooldown = 60f;
            logic.Action = States.Default;
        }
        else
        {
            
        }
    }
    
    private void Jump()
    {
        data.Movement.IsJumping = true;
        data.Collision.Radius = data.Collision.RadiusSpin;
        
        Velocity velocity = data.Movement.Velocity;
        velocity.Y = data.Physics.MinimalJumpSpeed;
        if (data.Input.Down.Left)
        {
            velocity.X = -2f;
        }
        else if (data.Input.Down.Right)
        {
            velocity.X = 2f;
        }
        
        AudioPlayer.Sound.Play(SoundStorage.Jump);
    }
}
