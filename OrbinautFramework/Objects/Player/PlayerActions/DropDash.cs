using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Spawnable.Shield;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

[FsmSourceGenerator.FsmState("Action")]
public struct DropDash(PlayerData data)
{
	public const byte MaxCharge = 22;

	private float _charge;
	
    public void Perform()
    {
	    if (data.Movement.IsGrounded || Cancel()) return;
		
	    if (data.Input.Down.Abc)
	    {
		    data.Movement.IsAirLock = false;		
		    _charge += Scene.Instance.ProcessSpeed;
			
		    if (_charge < MaxCharge || data.Visual.Animation == Animations.DropDash) return;
			
		    AudioPlayer.Sound.Play(SoundStorage.Charge3);
		    data.Visual.Animation = Animations.DropDash;
		    return;
	    }
		
	    switch (_charge)
	    {
		    case <= 0f:
			    return;
			
		    case >= MaxCharge:
			    data.Visual.Animation = Animations.Spin;
			    data.State = States.Default;
			    break;
	    }
		
	    _charge = 0f;
    }
    
    public void OnLand()
    {
    	if (Cancel()) return;
    	
    	if (_charge < MaxCharge) return;
    	
    	data.Node.Position += new Vector2(0f, data.Collision.Radius.Y - data.Collision.RadiusSpin.Y);
	    data.Collision.Radius = data.Collision.RadiusSpin;

	    SetGroundSpeed();
    	
	    data.Visual.Animation = Animations.Spin;
	    data.Movement.IsSpinning = true;
    	
    	data.SetCameraDelayX(8f);
    		
    	//TODO: obj_dust_dropdash
    	//instance_create(x, y + Radius.Y, obj_dust_dropdash, { image_xscale: Facing });
    	AudioPlayer.Sound.Stop(SoundStorage.Charge3);
    	AudioPlayer.Sound.Play(SoundStorage.Release);
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
    	if (!SharedData.DropDash || data.State != States.DropDash) return true;
	    
    	if (data.Node.Shield.Type <= ShieldContainer.Types.Normal) return false; 
	    if (data.Super.IsSuper || data.Item.InvincibilityTimer > 0f) return false;
    	
	    data.Visual.Animation = Animations.Spin;
	    data.State = States.Default;
    	return true;
    }
}
