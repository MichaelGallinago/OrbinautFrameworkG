using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

public struct DropDash : IAction
{
	public PlayerData Data { private get; init; }
	
	public const byte MaxCharge = 22;

	private float _charge;
	
	public PlayerNode PlayerNode { private get; init; }
	
    public void Perform()
    {
	    if (PlayerNode.Data.IsGrounded || Cancel()) return;
		
	    if (PlayerNode.Data.Input.Down.Abc)
	    {
		    PlayerNode.Data.IsAirLock = false;		
		    _charge += Scene.Instance.ProcessSpeed;
			
		    if (_charge < MaxCharge || PlayerNode.Data.Animation == Animations.DropDash) return;
			
		    AudioPlayer.Sound.Play(SoundStorage.Charge3);
		    PlayerNode.Data.Animation = Animations.DropDash;
		    return;
	    }
		
	    switch (_charge)
	    {
		    case <= 0f:
			    return;
			
		    case >= MaxCharge:
			    PlayerNode.Data.Animation = Animations.Spin;
			    PlayerNode.Data.Action.Type = Actions.Types.None;
			    break;
	    }
		
	    _charge = 0f;
    }
    
    public void OnLand()
    {
    	if (Cancel()) return;
    	
    	if (_charge < MaxCharge) return;
    	
    	Position += new Vector2(0f, PlayerNode.Data.Radius.Y - PlayerNode.Data.RadiusSpin.Y);
	    PlayerNode.Data.Radius = PlayerNode.Data.RadiusSpin;
    	
    	if (PlayerNode.Data.SuperData.IsSuper)
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
    	
	    PlayerNode.Data.Animation = Animations.Spin;
	    PlayerNode.Data.IsSpinning = true;
    	
    	SetCameraDelayX(8f);
    		
    	//TODO: obj_dust_dropdash
    	//instance_create(x, y + Radius.Y, obj_dust_dropdash, { image_xscale: Facing });
    	AudioPlayer.Sound.Stop(SoundStorage.Charge3);
    	AudioPlayer.Sound.Play(SoundStorage.Release);
    }
    
    private void UpdateGroundSpeed(float limitSpeed, float force)
    {
	    var sign = (float)PlayerNode.Data.Facing;
	    limitSpeed *= sign;
	    force *= sign;
	    
	    if (PlayerNode.Data.Velocity.X * sign >= 0f)
	    {
		    PlayerNode.Data.GroundSpeed.Value = MathF.Floor(PlayerNode.Data.GroundSpeed / 4f) + force;
		    if (sign * PlayerNode.Data.GroundSpeed <= limitSpeed) return;
		    PlayerNode.Data.GroundSpeed.Value = limitSpeed;
		    return;
	    }
    	
	    PlayerNode.Data.GroundSpeed.Value = force;
	    if (Mathf.IsZeroApprox(PlayerNode.Data.Angle)) return;
    	
	    PlayerNode.Data.GroundSpeed.Value += MathF.Floor(PlayerNode.Data.GroundSpeed / 2f);
    }

    private bool Cancel()
    {
    	if (!SharedData.DropDash || PlayerNode.Data.Action != Actions.Types.DropDash) return true;
    	
    	if (Shield.Type <= ShieldContainer.Types.Normal || 
	        PlayerNode.Data.SuperData.IsSuper || PlayerNode.Data.ItemInvincibilityTimer > 0f) return false;
    	
	    PlayerNode.Data.Animation = Animations.Spin;
	    PlayerNode.Data.Action.Type = Actions.Types.Default;
    	return true;
    }
}