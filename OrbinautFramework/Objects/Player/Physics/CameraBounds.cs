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
	    float leftBound = camera.Boundary.X + 16f;
	    if (data.PlayerNode.Position.X + data.Movement.Velocity.X >= leftBound) return;
	    
	    data.Movement.GroundSpeed.Value = 0f;
	    data.Movement.Velocity.X = 0f;
	    data.PlayerNode.Position = new Vector2(leftBound, data.PlayerNode.Position.Y);
    }
    
    private void ShiftToRightBound(ICamera camera)
    {
	    float rightBound = camera.Boundary.Z - 24f;
    	
	    // Allow player to walk past the right bound if they crossed Sign Post
	    //TODO: replace instance_exists
	    /*if instance_exists(obj_signpost) && x >= obj_signpost.x
	    {
		    _right_bound += 64;
	    }*/
	    
	    if (data.PlayerNode.Position.X + data.Movement.Velocity.X <= rightBound) return;
	    
	    data.Movement.GroundSpeed.Value = 0f;
	    data.Movement.Velocity.X = 0f;
	    data.PlayerNode.Position = new Vector2(rightBound, data.PlayerNode.Position.Y);
    }

    private void ShiftToTopBound(ICamera camera)
    {
	    float topBound = camera.Boundary.Y + 16f;
	    switch (data.State)
	    {
		    case States.Flight or States.Climb:
			    if (data.PlayerNode.Position.Y + data.Movement.Velocity.Y >= topBound) break;
    
			    if (data.State == States.Flight)
			    {
				    data.Movement.Gravity = GravityType.TailsDown;
			    }

			    data.Movement.Velocity.Y = 0f;
			    data.PlayerNode.Position = new Vector2(data.PlayerNode.Position.X, topBound);
			    break;
    		
		    case States.Glide when data.PlayerNode.Position.Y < topBound - 6:
			    data.Movement.GroundSpeed.Value = 0f;
			    break;
	    }
    }

    private void KillUnderBottomBound(ICamera camera)
    {
	    if (data.Water.AirTimer <= 0f) return;
	    if (data.PlayerNode.Position.Y <= Math.Max(camera.Boundary.W, camera.TargetBoundary.W)) return; //TODO: check c_stage.bound_bottom[player_camera.index]
	    
	    Kill();
    }
}