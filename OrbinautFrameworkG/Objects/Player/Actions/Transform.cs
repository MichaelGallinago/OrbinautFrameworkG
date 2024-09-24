using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Logic;
using OrbinautFrameworkG.Objects.Player.Sprite;
using static OrbinautFrameworkG.Objects.Player.ActionFsm;

namespace OrbinautFrameworkG.Objects.Player.Actions;

[FsmSourceGenerator.FsmState("Action")]
public struct Transform(PlayerData data, IPlayerLogic logic)
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

        data.State = PlayerStates.NoControl;
        
        data.Sprite.Animation = Animations.Transform;
        data.Visual.Visible = true;
        
        data.Damage.InvincibilityTimer = 0f;
        data.Super.Timer = 1f;

        LatePerform();
    }
    
    public States LatePerform()
    {
        _timer -= Scene.Instance.Speed;
        if (_timer > 0f) return States.Transform;

        data.State = PlayerStates.Control;
        logic.ResetData();
        //TODO: obj_star_super
        //instance_create(x, y, obj_star_super, { TargetPlayer: id });
        return States.Default;
    }
}
