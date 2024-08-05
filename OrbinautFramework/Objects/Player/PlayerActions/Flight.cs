using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Modules;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

public struct Flight : IAction
{
	public Player Player { private get; init; }
	
	private float _flightTimer;
	private float _flyUpTimer;
	
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

        if (IsUnderwater)
        {
            Animation = _flightTimer > 0f ? Animations.Fly : Animations.FlyTired;
        }
        else
        {
            Animation = _flightTimer > 0f ? Animations.Swim : Animations.SwimTired;
        }
    }
    
    private void PlayFlightSound()
    {
    	if (!Scene.Instance.IsTimePeriodLooped(16f, 8f) || !Sprite.CheckInCameras() || IsUnderwater) return;
	    
    	if (_flightTimer > 0f)
    	{
    		AudioPlayer.Sound.Play(SoundStorage.Flight);
    		return;
    	}
    	
    	AudioPlayer.Sound.Play(SoundStorage.Flight2);
    }

    private bool FlyUp()
    {
    	if (_flyUpTimer <= 0f) return false;

    	if (Velocity.Y < -1f)
    	{
    		_flyUpTimer = 0f;
    		return true;
    	}
    	
    	Gravity = GravityType.TailsUp;
    			
    	_flyUpTimer += Scene.Instance.ProcessSpeed;
    	if (_flyUpTimer >= 31f)
    	{
    		_flyUpTimer = 0f;
    	}

    	return true;
    }

    private void FlyDown()
    {
    	if (Input.Press.Abc && _flightTimer > 0f && (!IsUnderwater || CarryTarget == null))
    	{
    		//TODO: check that this works
    		_flyUpTimer = 1f;
    	}
    		
    	Gravity = GravityType.TailsDown;
    	
    	if (SharedData.SuperstarsTweaks && Input.Down.Down)
    	{
    		Gravity *= 3f;
    	}
    }
}