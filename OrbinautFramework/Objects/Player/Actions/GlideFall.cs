using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Sprite;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Actions;

[FsmSourceGenerator.FsmState("Action")]
public readonly struct GlideFall(PlayerData data, IPlayerLogic logic)
{
    private readonly GlideCollisionLogic _collision = new(data, logic);
    
    public void Enter()
    {
        data.Sprite.Animation = Animations.GlideFall;
        data.Collision.Radius = data.Collision.RadiusNormal;
		
        data.ResetGravity();
    }
    
    public States LatePerform()
    {
        _collision.CollideWallsAndCeiling(out Angles.Quadrant moveQuadrant);

        if (moveQuadrant == Angles.Quadrant.Up || !_collision.CollideFloor()) return States.GlideFall;
        
        Land();
        return States.Default;
    }
    
    private void Land()
    {
        AudioPlayer.Sound.Play(SoundStorage.Land);
        logic.Land();
		
        if (Angles.GetQuadrant(data.Movement.Angle) != Angles.Quadrant.Down)
        {
            data.Movement.GroundSpeed.Value = data.Movement.Velocity.X;
            return;
        }
					
        data.Sprite.Animation = Animations.GlideLand;
        data.Movement.GroundLockTimer = 16f;
        data.Movement.GroundSpeed.Value = 0f;
        data.Movement.Velocity.X = 0f;
    }
}
