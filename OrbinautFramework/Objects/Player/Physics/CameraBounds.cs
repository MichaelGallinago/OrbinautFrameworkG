using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Data;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Physics;

public struct CameraBounds(PlayerData data)
{
    public void Match()
    {
    	if (data.Death.IsDead) return;
    	
	    //TODO: check this
    	if (!data.PlayerNode.IsCameraTarget(out ICamera camera) && 
	        !Scene.Instance.Players.First().PlayerNode.IsCameraTarget(out camera)) return;

	    ShiftToLeftBound(camera);
	    ShiftToRightBound(camera);
	    ShiftToTopBound(camera);
	    KillUnderBottomBound(camera);
    }

    private void ShiftToLeftBound(ICamera camera)
    {
	    if (data.PlayerNode.Position.X + data.Physics.Velocity.X >= camera.Boundary.X + 16f) return;
	    
	    data.Physics.GroundSpeed.Value = 0f;
	    data.Physics.Velocity.X = 0f;
	    data.PlayerNode.Position = new Vector2(camera.Boundary.X + 16f, data.PlayerNode.Position.Y);
    }
    
    private void ShiftToRightBound(ICamera camera)
    {
	    float rightBound = camera.Boundary.Z - 24f;
    	
	    // Allow player to walk past the right bound if they crossed Sign Post
	    //TODO: replace instance_exists
	    /*if (instance_exists(obj_signpost))
	    {
		    // TODO: There should be a better way?
		    rightBound += 64;
	    }*/
    	
	    // Right bound
	    if (data.PlayerNode.Position.X + data.Physics.Velocity.X <= rightBound) return;
	    
	    data.Physics.GroundSpeed.Value = 0f;
	    data.Physics.Velocity.X = 0f;
	    data.PlayerNode.Position = new Vector2(rightBound, data.PlayerNode.Position.Y);
    }

    private void ShiftToTopBound(ICamera camera)
    {
	    switch (data.State)
	    {
		    case States.Flight or States.Climb:
			    if (data.PlayerNode.Position.Y + data.Physics.Velocity.Y >= camera.Boundary.Y + 16f) break;
    
			    if (data.State == States.Flight)
			    {
				    data.Physics.Gravity = GravityType.TailsDown;
			    }

			    data.Physics.Velocity.Y = 0f;
			    data.PlayerNode.Position = new Vector2(data.PlayerNode.Position.X, camera.Boundary.Y + 16f);
			    break;
    		
		    case States.Glide when data.PlayerNode.Position.Y < camera.Boundary.Y + 10f:
			    data.Physics.GroundSpeed.Value = 0f;
			    break;
	    }
    }

    private void KillUnderBottomBound(ICamera camera)
    {
	    if (data.Water.AirTimer <= 0f) return;
	    if (data.PlayerNode.Position.Y <= Math.Max(camera.Boundary.W, camera.TargetBoundary.W)) return;
	    
	    Kill();
    }
}