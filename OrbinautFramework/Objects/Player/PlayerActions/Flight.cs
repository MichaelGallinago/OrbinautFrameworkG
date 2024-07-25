using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

public struct Flight : IAction
{
	private float _flightTimer;
	private float _flyUpTimer;
	
    public void Perform(Player player)
    {
        // Flight timer
        if (_flightTimer > 0f)
        {
            _flightTimer -= Scene.Local.ProcessSpeed;
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
    
    public void PlayFlightSound()
    {
    	if (!Scene.Local.IsTimePeriodLooped(16f, 8f) || !Sprite.CheckInCameras() || IsUnderwater) return;
	    
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
    			
    	_flyUpTimer += Scene.Local.ProcessSpeed;
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