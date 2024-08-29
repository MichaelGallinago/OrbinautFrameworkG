using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Sprite;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Actions;

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

        data.State = PlayerStates.NoControl;
        
        data.Sprite.Animation = Animations.Transform;
        data.Node.Visible = true;
        
        data.Damage.InvincibilityTimer = 0f;
        data.Super.Timer = 1f;

        LatePerform();
    }
    
    public States LatePerform()
    {
        _timer -= Scene.Instance.Speed;
        if (_timer > 0f) return States.Transform;

        data.State = PlayerStates.Control;
        return States.Default;
    }
}
