using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Physics;

namespace OrbinautFramework3.Objects.Player;

public struct Death
{
    public void Process()
    {
    	if (!IsDead) return;

    	ICamera camera = Views.Local.BottomCamera;
    	
    	// If drowned, wait until we're far enough off-screen
    	const int drownScreenOffset = 276;
    	if (AirTimer == 0 && (int)Position.Y <= camera.DrawPosition.Y + SharedData.ViewSize.Y + drownScreenOffset)
    	{
    		return;
    	}
    	
    	switch (DeathState)
    	{
    		case DeathStates.Wait: Wait(camera); break;
    		case DeathStates.Restart: Restart(); break;
    	}
    }
    
    private void Wait(ICamera camera)
    {
    	if ((int)Position.Y <= 32f + (SharedData.PhysicsType < PhysicsTypes.S3 ? 
    		    camera.Boundary.W : camera.DrawPosition.Y + SharedData.ViewSize.Y)) return;
    	
    	RestartState = RestartStates.ResetLevel;
    	
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
    				
    	if (--SharedData.LifeCount > 0 && Scene.Local.Time < 36000f)
    	{
    		DeathState = DeathStates.Restart;
    		RestartTimer = 60f;
    	}
    	else
    	{
    		DeathState = DeathStates.Wait;
    			
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
    		RestartTimer -= Scene.Local.ProcessSpeed;
    		if (RestartTimer > 0f) return;
    		AudioPlayer.Music.StopAllWithMute(0.5f);
    				
    		// TODO: fade
    		//fade_perform(FADE_MD_OUT, FADE_BL_BLACK, 1);
    	}
    			
    	// TODO: fade
    	//if (c_framework.fade.state != FADESTATE.PLAINCOLOUR) break;

    	Scene.Local.Tree.ReloadCurrentScene();
    }
}