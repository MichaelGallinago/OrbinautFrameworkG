using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

[FsmSourceGenerator.FsmState("Action")]
public struct Flight(PlayerData data)
{
	private float _flightTimer;
	private float _ascendTimer;
	
    public void Perform()
    {
        // Flight timer
        if (_flightTimer > 0f)
        {
            _flightTimer -= Scene.Instance.ProcessSpeed;
        }

        if (!Ascend())
        {
            Descend();
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

    private bool Ascend()
    {
    	if (_ascendTimer <= 0f) return false;

    	if (data.Movement.Velocity.Y < -1f)
    	{
    		_ascendTimer = 0f;
    		return true;
    	}
    	
    	data.Movement.Gravity = GravityType.TailsUp;
    			
    	_ascendTimer += Scene.Instance.ProcessSpeed;
    	if (_ascendTimer >= 31f)
    	{
    		_ascendTimer = 0f;
    	}

    	return true;
    }

    private void Descend()
    {
    	if (data.Input.Press.Abc && _flightTimer > 0f && (!data.Water.IsUnderwater || data.Carry.Target == null))
    	{
    		//TODO: check that this works
    		_ascendTimer = 1f;
    	}
    		
    	data.Movement.Gravity = GravityType.TailsDown;
    	
    	if (SharedData.SuperstarsTweaks && data.Input.Down.Down)
    	{
    		data.Movement.Gravity *= 3f;
    	}
    }
}