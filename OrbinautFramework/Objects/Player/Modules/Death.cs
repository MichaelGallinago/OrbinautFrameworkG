using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Physics;

namespace OrbinautFramework3.Objects.Player.Modules;

//TODO: check this
public struct Death
{
	public enum States : byte
	{
		Wait, Restart
	}
	
    public void Process()
    {
    	if (!IsDead) return;

    	ICamera camera = Views.Instance.BottomCamera;
    	
    	// If drowned, wait until we're far enough off-screen
    	const int drownScreenOffset = 276;
    	if (AirTimer == 0 && (int)Position.Y <= camera.DrawPosition.Y + SharedData.ViewSize.Y + drownScreenOffset)
    	{
    		return;
    	}
    	
    	switch (State)
    	{
    		case States.Wait: Wait(camera); break;
    		case States.Restart: Restart(); break;
    	}
    }
    
    private void Wait(ICamera camera)
    {
    	if ((int)Position.Y <= 32f + (SharedData.PhysicsType < PhysicsTypes.S3 ? 
    		    camera.Boundary.W : camera.DrawPosition.Y + SharedData.ViewSize.Y)) return;
	    
    	SetNextState();
    }

    private void SetNextState()
    {
    	// If CPU, respawn
    	if (Id != 0)
    	{
    		Respawn();
    		return;
    	}
    	
    	//TODO: gui hud
    	/*if (instance_exists(obj_gui_hud))
    	{
    		obj_gui_hud.update_timer = false;
    	}*/
    				
    	if (--SharedData.LifeCount > 0 && Scene.Instance.Time < 36000f)
    	{
    		State = States.Restart;
    		RestartTimer = 60f;
    	}
    	else
    	{
    		State = States.Wait;
    			
    		//TODO: gui gameover
    		//instance_create_depth(0, 0, RENDERER_DEPTH_HUD, obj_gui_gameover);				
    		AudioPlayer.Music.Play(MusicStorage.GameOver);
    	}
    }

    private void Restart()
    {
    	// Wait 60 steps, then restart
    	if (RestartTimer > 0f)
    	{
    		RestartTimer -= Scene.Instance.ProcessSpeed;
    		if (RestartTimer > 0f) return;
    		AudioPlayer.Music.StopAllWithMute(0.5f);
    				
    		// TODO: fade
    		//fade_perform(FADE_MD_OUT, FADE_BL_BLACK, 1);
    	}
    			
    	// TODO: fade
    	//if (c_framework.fade.state != FADESTATE.PLAINCOLOUR) break;

    	Scene.Instance.Tree.ReloadCurrentScene();
    }
}
