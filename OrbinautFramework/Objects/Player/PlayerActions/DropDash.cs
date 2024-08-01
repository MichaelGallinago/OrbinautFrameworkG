using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

public struct DropDash : IAction
{
	public const byte MaxCharge = 22;

	private float _charge;
	
	public Player Player { private get; init; }
	
    public void Perform()
    {
	    if (Player.Data.IsGrounded || Cancel()) return;
		
	    if (Player.Data.Input.Down.Abc)
	    {
		    Player.Data.IsAirLock = false;		
		    _charge += Scene.Local.ProcessSpeed;
			
		    if (_charge < MaxCharge || Player.Data.Animation == Animations.DropDash) return;
			
		    AudioPlayer.Sound.Play(SoundStorage.Charge3);
		    Player.Data.Animation = Animations.DropDash;
		    return;
	    }
		
	    switch (_charge)
	    {
		    case <= 0f:
			    return;
			
		    case >= MaxCharge:
			    Player.Data.Animation = Animations.Spin;
			    Player.Data.Action.Type = Actions.Types.None;
			    break;
	    }
		
	    _charge = 0f;
    }
    
    private void Release()
    {
    	if (Cancel()) return;
    	
    	if (_charge < MaxCharge) return;
    	
    	Position += new Vector2(0f, Player.Data.Radius.Y - Player.Data.RadiusSpin.Y);
	    Player.Data.Radius = Player.Data.RadiusSpin;
    	
    	if (Player.Data.SuperData.IsSuper)
    	{
    		UpdateGroundSpeed(13f, 12f);
    		if (IsCameraTarget(out ICamera camera))
    		{
    			camera.SetShakeTimer(6f);
    		}
    	}
    	else
    	{
    		UpdateGroundSpeed(12f, 8f);
    	}
    	
	    Player.Data.Animation = Animations.Spin;
	    Player.Data.IsSpinning = true;
    	
    	SetCameraDelayX(8f);
    		
    	//TODO: obj_dust_dropdash
    	//instance_create(x, y + Radius.Y, obj_dust_dropdash, { image_xscale: Facing });
    	AudioPlayer.Sound.Stop(SoundStorage.Charge3);
    	AudioPlayer.Sound.Play(SoundStorage.Release);
    }

    private bool Cancel()
    {
    	if (!SharedData.DropDash || Player.Data.Action != Actions.Types.DropDash) return true;
    	
    	if (Shield.Type <= ShieldContainer.Types.Normal || 
	        Player.Data.SuperData.IsSuper || Player.Data.ItemInvincibilityTimer > 0f) return false;
    	
	    Player.Data.Animation = Animations.Spin;
	    Player.Data.Action.Type = Actions.Types.Default;
    	return true;
    }

    private void UpdateGroundSpeed(float limitSpeed, float force)
    {
    	var sign = (float)Player.Data.Facing;
    	limitSpeed *= sign;
    	force *= sign;
	    
    	if (Player.Data.Velocity.X * sign >= 0f)
    	{
		    Player.Data.GroundSpeed.Value = MathF.Floor(Player.Data.GroundSpeed / 4f) + force;
    		if (sign * Player.Data.GroundSpeed <= limitSpeed) return;
		    Player.Data.GroundSpeed.Value = limitSpeed;
    		return;
    	}
    	
	    Player.Data.GroundSpeed.Value = force;
    	if (Mathf.IsZeroApprox(Player.Data.Angle)) return;
    	
	    Player.Data.GroundSpeed.Value += MathF.Floor(Player.Data.GroundSpeed / 2f);
    }
}