using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Sprite;
using OrbinautFramework3.Objects.Spawnable.Shield;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

[FsmSourceGenerator.FsmState("Action")]
public struct DropDash(PlayerData data)
{
	public const byte MaxCharge = 22;

	private float _charge = 0f;
	
    public States Perform()
    {
	    if (data.Movement.IsGrounded) return States.DropDash;
	    if (Cancel()) return States.Jump;
		
	    if (data.Input.Down.Abc)
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
    	if (Cancel()) return States.Default;
    	
    	if (_charge < MaxCharge) return States.Default;
    	
    	data.Node.Position += new Vector2(0f, data.Collision.Radius.Y - data.Collision.RadiusSpin.Y);
	    data.Collision.Radius = data.Collision.RadiusSpin;

	    SetGroundSpeed();
    	
	    data.Sprite.Animation = Animations.Spin;
	    data.Movement.IsSpinning = true;
    	
    	data.SetCameraDelayX(8f);
    		
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
	    if (data.IsCameraTarget(out ICamera camera))
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
		    AcceleratedValue groundSpeed = data.Movement.GroundSpeed;
		    groundSpeed.Value = MathF.Floor(groundSpeed / 4f) + force;
		    if (sign * groundSpeed <= limitSpeed) return;
		    groundSpeed.Value = limitSpeed;
		    return;
	    }
    	
	    data.Movement.GroundSpeed.Value = force;
	    if (Mathf.IsZeroApprox(data.Movement.Angle)) return;
    	
	    data.Movement.GroundSpeed.Value += MathF.Floor(data.Movement.GroundSpeed / 2f);
    }

    private bool Cancel()
    {
    	if (data.Node.Shield.Type <= ShieldContainer.Types.Normal) return false; 
	    return !data.Super.IsSuper && !(data.Item.InvincibilityTimer > 0f);
    }
}
