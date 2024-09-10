using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.Actions;

[FsmSourceGenerator.FsmState("Action")]
public struct Flight(PlayerData data, CharacterFlightLogic flightLogic)
{
	private float _flightTimer = 480f;
	private float _ascendTimer = 0f;

	public void Enter()
	{
		data.Collision.Radius = data.Collision.RadiusNormal;
		
		data.Movement.Gravity = GravityType.FlightDown;
		data.Movement.IsAirLock = false;
		data.Movement.IsSpinning = false;
		
		if (!data.Water.IsUnderwater)
		{
			AudioPlayer.Sound.Play(SoundStorage.Flight);
		}
		
		data.Input.Down = data.Input.Down with { Aby = false };
		data.Input.Press = data.Input.Press with { Aby = false };
	}
	
    public void Perform()
    {
	    UpdateTimer();
	    
        if (!Ascend())
        {
            Descend();
        }
        
        PlayFlightSound();
        SetAnimation();
    }

    public static void Exit()
    {
	    AudioPlayer.Sound.Stop(SoundStorage.Flight);
	    AudioPlayer.Sound.Stop(SoundStorage.Flight2);
    }

    private void UpdateTimer()
    {
	    if (_flightTimer > 0f)
	    {
		    _flightTimer -= Scene.Instance.Speed;
	    }
    }
    
    private void PlayFlightSound()
    {
	    if (data.Water.IsUnderwater) return;
    	if (!Scene.Instance.IsTimePeriodLooped(16f, 8f)) return;
	    if (!data.Sprite.CheckInCameras()) return;
	    
	    AudioPlayer.Sound.Play(_flightTimer > 0f ? SoundStorage.Flight : SoundStorage.Flight2);
    }

    private bool Ascend()
    {
    	if (_ascendTimer <= 0f) return false;

    	if (data.Movement.Velocity.Y < -1f)
    	{
    		_ascendTimer = 0f;
    		return true;
    	}
    	
    	data.Movement.Gravity = GravityType.FlightUp;
    			
    	_ascendTimer += Scene.Instance.Speed;
    	if (_ascendTimer >= 31f)
    	{
    		_ascendTimer = 0f;
    	}

    	return true;
    }

    private void Descend()
    {
    	if (_flightTimer > 0f && data.Input.Press.Aby && flightLogic.CheckAscendAllowed())
    	{
    		_ascendTimer = 1f;
    	}
	    
    	data.Movement.Gravity = GravityType.FlightDown;
    }

    private void SetAnimation()
    {
	    if (data.Water.IsUnderwater)
	    {
		    data.Sprite.Animation = _flightTimer > 0f ? Animations.Swim : Animations.SwimTired;
	    }
	    else
	    {
		    data.Sprite.Animation = _flightTimer > 0f ? Animations.Fly : Animations.FlyTired;
	    }
    }
}
