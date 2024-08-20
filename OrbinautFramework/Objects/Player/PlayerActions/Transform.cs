using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Sprite;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

[FsmSourceGenerator.FsmState("Action")]
public struct Transform(PlayerData data)
{
#if S3_PHYSICS || SK_PHYSICS
    private float _timer = 26f;
#else
    private float _timer = 36f;
#endif
    
    public void Enter()
    {
        AudioPlayer.Sound.Play(SoundStorage.Transform);
        AudioPlayer.Music.Play(MusicStorage.Super);
        
        //TODO: instance_create obj_star_super
        //instance_create(x, y, obj_star_super, { TargetPlayer: id });
        
        data.Movement.IsControlRoutineEnabled = false;
        data.Collision.IsObjectInteractionEnabled = false;			
        data.Damage.InvincibilityTimer = 0f;
        data.Super.Timer = 1f;
        data.Visual.Animation = Animations.Transform;
        data.Node.Visible = true;

        LatePerform();
    }
    
    public States LatePerform()
    {
        _timer -= Scene.Instance.ProcessSpeed;
        if (_timer > 0f) return States.Transform;
        
        data.Collision.IsObjectInteractionEnabled = true;
        data.Movement.IsControlRoutineEnabled = true;
        return States.Default;
    }
}
