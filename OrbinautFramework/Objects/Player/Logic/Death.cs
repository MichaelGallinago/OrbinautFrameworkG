using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player.Logic;

//TODO: check this
public struct Death(PlayerData data, IPlayerLogic logic)
{
	public enum States : byte
	{
		Wait, Restart
	}
	
    public void Process()
    {
    	if (!data.Death.IsDead) return;

    	ICamera camera = Views.Instance.BottomCamera;
    	
    	// If drowned, wait until we're far enough off-screen
    	const int drownScreenOffset = 276;
    	if (data.Water.AirTimer == 0 && 
	        (int)data.Node.Position.Y <= camera.DrawPosition.Y + SharedData.ViewSize.Y + drownScreenOffset)
    	{
    		return;
    	}
    	
    	switch (data.Death.State)
    	{
    		case States.Wait: Wait(camera); break;
    		case States.Restart: Restart(); break;
    	}
    }
    
    private void Wait(ICamera camera)
    {
#if S3_PHYSICS || SK_PHYSICS
	    float bound = camera.DrawPosition.Y + SharedData.ViewSize.Y;
#else
	    float bound = camera.Boundary.W;
#endif
    	if ((int)data.Node.Position.Y <= 32f + bound) return;
    	SetNextState();
    }

    private void SetNextState()
    {
    	// If CPU, respawn
    	if (data.Id == 0)
    	{
		    logic.Respawn();
    		return;
    	}
    	
    	//TODO: gui hud
    	/*if (instance_exists(obj_gui_hud))
    	{
    		obj_gui_hud.update_timer = false;
    	}*/
    				
    	if (--SharedData.LifeCount > 0 && Scene.Instance.Time < 36000f)
    	{
    		data.Death.State = States.Restart;
    		data.Death.RestartTimer = 60f;
    	}
    	else
    	{
    		data.Death.State = States.Wait;
    			
    		//TODO: gui gameover
    		//instance_create_depth(0, 0, RENDERER_DEPTH_HUD, obj_gui_gameover);				
    		AudioPlayer.Music.Play(MusicStorage.GameOver);
    	}
    }

    private void Restart()
    {
    	// Wait 60 steps, then restart
    	if (data.Death.RestartTimer > 0f)
    	{
    		data.Death.RestartTimer -= Scene.Instance.ProcessSpeed;
    		if (data.Death.RestartTimer > 0f) return;
    		AudioPlayer.Music.StopAllWithMute(0.5f);
    				
    		// TODO: fade
    		//fade_perform(FADE_MD_OUT, FADE_BL_BLACK, 1);
    	}
    			
    	// TODO: fade
    	//if (c_framework.fade.state != FADESTATE.PLAINCOLOUR) break;

    	Scene.Instance.Tree.ReloadCurrentScene();
    }
}
