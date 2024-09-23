using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.StaticStorages;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player.Logic;

//TODO: check this
public readonly struct Death(PlayerData data, IPlayerLogic logic)
{
	public enum States : byte
	{
		Wait, Restart, GameOver
	}
	
    public void Process()
    {
    	switch (data.Death.State)
    	{
    		case States.Wait: Wait(); break;
    		case States.Restart: Restart(); break;
    	}
    }
    
    private void Wait()
    {
	    ICamera camera = Views.Instance.BottomCamera;
	    
	    // If drowned, wait until we're far enough off-screen
	    const int drownScreenOffset = 276;
	    if (data.Water.AirTimer <= 0f)
	    {
		    int bottomBound = camera.DrawPosition.Y + Settings.ViewSize.Y + drownScreenOffset;
		    if ((int)data.Movement.Position.Y <= bottomBound) return;

		    if (!logic.ControlType.IsCpu)
		    {
			    Scene.Instance.State = Scene.States.StopObjects;
		    }
	    }
	    
#if S3_PHYSICS || SK_PHYSICS
	    float bound = camera.DrawPosition.Y + SharedData.ViewSize.Y;
#else
	    float bound = camera.Boundary.W;
#endif
    	if ((int)data.Movement.Position.Y <= 32f + bound) return;
    	SetNextState();
    }

    private void SetNextState()
    {
    	if (logic.ControlType.IsCpu)
    	{
		    logic.Respawn();
    		return;
    	}
	    
    	//TODO: gui hud
    	/*if (instance_exists(obj_gui_hud))
    	{
    		obj_gui_hud.update_timer = false;
    	}*/
    				
    	if (--SaveData.LifeCount > 0 && Scene.Instance.Time < 36000f)
    	{
    		data.Death.State = States.Restart;
    		data.Death.RestartTimer = 60f;
    	}
    	else
    	{
		    data.Death.State = States.GameOver;
    		//TODO: gui gameover
    		//instance_create_depth(0, 0, RENDERER_DEPTH_HUD, obj_gui_gameover);
	    }
    }

    private void Restart()
    {
    	// Wait 60 steps, then restart
    	if (data.Death.RestartTimer > 0f)
    	{
    		data.Death.RestartTimer -= Scene.Instance.Speed;
    		if (data.Death.RestartTimer > 0f) return;
    		AudioPlayer.Music.StopAllWithMute(0.5f);
    				
    		// TODO: fade
    		//fade_perform(FADE_MD_OUT, FADE_BL_BLACK, 1);
    	}
    			
    	// TODO: fade
    	//if (c_framework.fade.state != FADESTATE.PLAINCOLOUR) break;
		
	    SharedData.Clear();
    	Scene.Instance.Tree.ReloadCurrentScene();
    }
}
