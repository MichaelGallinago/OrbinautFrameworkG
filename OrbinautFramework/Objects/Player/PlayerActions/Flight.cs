using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Modules;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

[FsmSourceGenerator.FsmState("Action")]
public struct Flight(PlayerData data)
{
	private float _flightTimer;
	private float _ascensionTimer;
	
    public void Perform()
    {
        // Flight timer
        if (_flightTimer > 0f)
        {
            _flightTimer -= Scene.Instance.ProcessSpeed;
        }

        if (!FlyUp())
        {
            FlyDown();
        }

        PlayFlightSound();

        if (data.Water.IsUnderwater)
        {
            data.Visual.Animation = _flightTimer > 0f ? Animations.Fly : Animations.FlyTired;
        }
        else
        {
            data.Visual.Animation = _flightTimer > 0f ? Animations.Swim : Animations.SwimTired;
        }
    }
    
    private void PlayFlightSound()
    {
	    if (data.Water.IsUnderwater) return;
    	if (!Scene.Instance.IsTimePeriodLooped(16f, 8f)) return;
	    if (!data.PlayerNode.Sprite.CheckInCameras()) return;
	    
    	if (_flightTimer > 0f)
    	{
    		AudioPlayer.Sound.Play(SoundStorage.Flight);
    		return;
    	}
    	
    	AudioPlayer.Sound.Play(SoundStorage.Flight2);
    }

    private bool FlyUp()
    {
    	if (_ascensionTimer <= 0f) return false;

    	if (data.Physics.Velocity.Y < -1f)
    	{
    		_ascensionTimer = 0f;
    		return true;
    	}
    	
    	data.Physics.Gravity = GravityType.TailsUp;
    			
    	_ascensionTimer += Scene.Instance.ProcessSpeed;
    	if (_ascensionTimer >= 31f)
    	{
    		_ascensionTimer = 0f;
    	}

    	return true;
    }

    private void FlyDown()
    {
    	if (data.Input.Press.Abc && _flightTimer > 0f && (!data.Water.IsUnderwater || CarryTarget == null))
    	{
    		//TODO: check that this works
    		_ascensionTimer = 1f;
    	}
    		
    	data.Physics.Gravity = GravityType.TailsDown;
    	
    	if (SharedData.SuperstarsTweaks && data.Input.Down.Down)
    	{
    		data.Physics.Gravity *= 3f;
    	}
    }
}