using System;
using Godot;
using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.SceneModule;
using OrbinautFrameworkG.Framework.View;
using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Sprite;
using OrbinautFrameworkG.Objects.Spawnable.Shield;
using static OrbinautFrameworkG.Objects.Player.ActionFsm;

namespace OrbinautFrameworkG.Objects.Player.Actions;

[FsmSourceGenerator.FsmState("Action")]
public struct DropDash(PlayerData data)
{
	public const byte MaxCharge = 22;

	private float _charge = 0f;
	
    public States Perform()
    {
	    if (data.Movement.IsGrounded) return States.DropDash;
	    if (Cancel()) return States.Jump;
		
	    if (data.Input.Down.Aby)
	    {
		    Charge();
		    return States.DropDash;
	    }

	    if (_charge >= MaxCharge)
	    {
		    data.Sprite.Animation = Animations.Spin;
		    return States.None;
	    }
	    
	    _charge = 0f; 
	    return States.DropDash;
    }
    
    public States OnLand()
    {
    	if (_charge < MaxCharge) return States.Default;
    	
    	data.Movement.Position.Y += data.Collision.Radius.Y - data.Collision.RadiusSpin.Y;
	    data.Collision.Radius = data.Collision.RadiusSpin;

	    SetGroundSpeed();
    	
	    data.Sprite.Animation = Animations.Spin;
	    data.Movement.IsSpinning = true;
    	
    	data.Node.SetCameraDelayX(8f);
	    
    	//TODO: obj_dust_dropdash
    	//instance_create(x, y + Radius.Y, obj_dust_dropdash, { image_xscale: Facing });
    	AudioPlayer.Sound.Stop(SoundStorage.Charge3);
    	AudioPlayer.Sound.Play(SoundStorage.Release);

	    return States.Default;
    }

    private void Charge()
    {
	    data.Movement.IsAirLock = false;		
	    _charge += Scene.Instance.Speed;
			
	    if (_charge < MaxCharge || data.Sprite.Animation == Animations.DropDash) return;
			
	    AudioPlayer.Sound.Play(SoundStorage.Charge3);
	    data.Sprite.Animation = Animations.DropDash;
    }

    private void SetGroundSpeed()
    {
	    if (!data.Super.IsSuper)
	    {
		    UpdateGroundSpeed(12f, 8f);
		    return;
	    }
	    
	    UpdateGroundSpeed(13f, 12f);
	    if (data.Node.IsCameraTarget(out ICamera camera))
	    {
		    camera.SetShakeTimer(6f);
	    }
    }
    
    private void UpdateGroundSpeed(float limitSpeed, float force)
    {
	    var sign = (float)data.Visual.Facing;
	    limitSpeed *= sign;
	    force *= sign;
	    
	    if (data.Movement.Velocity.X * sign >= 0f)
	    {
		    float groundSpeed = data.Movement.GroundSpeed;
		    groundSpeed = MathF.Floor(groundSpeed / 4f) + force;
		    data.Movement.GroundSpeed = sign * groundSpeed <= limitSpeed ? groundSpeed : limitSpeed;
		    return;
	    }
    	
	    data.Movement.GroundSpeed = force;
	    if (Mathf.IsZeroApprox(data.Movement.Angle)) return;
    	
	    data.Movement.GroundSpeed += MathF.Floor(data.Movement.GroundSpeed / 2f);
    }

    private bool Cancel()
    {
    	if (data.Node.Shield.Type <= ShieldContainer.Types.Normal) return false; 
	    return !data.Super.IsSuper && data.Item.InvincibilityTimer <= 0f;
    }
}
