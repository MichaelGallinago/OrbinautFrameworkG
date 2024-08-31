using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Physics;

public readonly struct CameraBounds(PlayerData data, IPlayerLogic logic)
{
    public void Match()
    {
	    if (!data.IsInCamera(out ICamera camera)) return;

	    ShiftToLeftBound(camera);
	    ShiftToRightBound(camera);
	    ShiftToTopBound(camera);
	    KillUnderBottomBound(camera);
    }

    private void ShiftToLeftBound(ICamera camera)
    {
	    float leftBound = camera.Boundary.X + 16f;
	    if (data.Node.Position.X + data.Movement.Velocity.X >= leftBound) return;
	    
	    data.Movement.GroundSpeed.Value = 0f;
	    data.Movement.Velocity.X = 0f;
	    data.Node.Position = new Vector2(leftBound, data.Node.Position.Y);
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
	    
	    if (data.Node.Position.X + data.Movement.Velocity.X <= rightBound) return;
	    
	    data.Movement.GroundSpeed.Value = 0f;
	    data.Movement.Velocity.X = 0f;
	    data.Node.Position = new Vector2(rightBound, data.Node.Position.Y);
    }

    private void ShiftToTopBound(ICamera camera)
    {
	    float topBound = camera.Boundary.Y + 16f;
	    switch (logic.Action)
	    {
		    case States.Flight or States.Climb:
			    if (data.Node.Position.Y + data.Movement.Velocity.Y >= topBound) break;
    
			    if (logic.Action == States.Flight)
			    {
				    data.Movement.Gravity = GravityType.TailsDown;
			    }

			    data.Movement.Velocity.Y = 0f;
			    data.Node.Position = new Vector2(data.Node.Position.X, topBound);
			    break;
    		
		    case States.GlideAir or States.GlideFall or States.GlideGround when data.Node.Position.Y < topBound - 6:
			    data.Movement.GroundSpeed.Value = 0f;
			    break;
	    }
    }

    private void KillUnderBottomBound(ICamera camera)
    {
	    if (data.Water.AirTimer <= 0f) return;
	    if ((int)data.Node.Position.Y < Math.Max(camera.Boundary.W, camera.TargetBoundary.W)) return; 
	    
	    logic.Kill();
    }
}
