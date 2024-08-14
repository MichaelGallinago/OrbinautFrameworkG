using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Modules;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

[FsmSourceGenerator.FsmState("Action")]
public struct Transform(PlayerData data)
{
    private float _timer = SharedData.PhysicsType >= PhysicsCore.Types.S3 ? 26f : 36f;
    
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
    }
    
    public void Perform()
    {
        
    }
}