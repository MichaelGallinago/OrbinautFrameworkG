using System;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

[FsmSourceGenerator.FsmState("Action")]
public struct GlideGround(PlayerData data)
{
    private float _dustTimer;
    
    public void Perform()
    {
        UpdateGroundVelocityX();
        
        if (StopSliding()) return;
        SpawnDustParticles();
    }
    
    private void UpdateGroundVelocityX()
    {
        if (!data.Input.Down.Abc)
        {
            data.Movement.Velocity.X = 0f;
            return;
        }
		
        const float slideFriction = -0.09375f;
		
        float speedX = data.Movement.Velocity.X;
        data.Movement.Velocity.AccelerationX = Math.Sign(data.Movement.Velocity.X) * slideFriction;
        switch (speedX)
        {
            case > 0f: data.Movement.Velocity.MaxX(0f); break;
            case < 0f: data.Movement.Velocity.MinX(0f); break;
        }
    }

    private bool StopSliding()
    {
        if (data.Movement.Velocity.X != 0f) return false;
        
        Land();
        data.Visual.OverrideFrame = 1;
			
        data.Visual.Animation = Animations.GlideGround;
        data.Movement.GroundLockTimer = 16f;
        data.Movement.GroundSpeed.Value = 0f;
			
        return true;
    }

    private void SpawnDustParticles()
    {
        if (_dustTimer % 4f < Scene.Instance.ProcessSpeed)
        {
            //TODO: obj_dust_skid
            //instance_create(x, y + data.Collision.Radius.Y, obj_dust_skid);
        }
				
        if (_dustTimer > 0f && _dustTimer % 8f < Scene.Instance.ProcessSpeed)
        {
            AudioPlayer.Sound.Play(SoundStorage.Slide);
        }
					
        _dustTimer += Scene.Instance.ProcessSpeed;
    }
}