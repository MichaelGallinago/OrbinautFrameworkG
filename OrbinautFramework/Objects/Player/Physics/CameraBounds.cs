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
	    MovementData movement = data.Movement;
	    if (movement.Position.X + movement.Velocity.X >= leftBound) return;
	    
	    movement.GroundSpeed.Value = 0f;
	    movement.Velocity.X = 0f;
	    movement.Position = new Vector2(leftBound, movement.Position.Y);
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

	    MovementData movement = data.Movement;
	    if (movement.Position.X + movement.Velocity.X <= rightBound) return;
	    
	    movement.GroundSpeed.Value = 0f;
	    movement.Velocity.X = 0f;
	    movement.Position = new Vector2(rightBound, movement.Position.Y);
    }

    private void ShiftToTopBound(ICamera camera)
    {
	    float topBound = camera.Boundary.Y + 16f;
	    MovementData movement = data.Movement;
	    switch (logic.Action)
	    {
		    case States.Flight or States.Climb:
			    if (movement.Position.Y + movement.Velocity.Y >= topBound) break;

			    movement.Velocity.Y = 0f;
			    movement.Position = new Vector2(movement.Position.X, topBound);
			    
			    if (logic.Action == States.Flight)
			    {
				    movement.Gravity = GravityType.FlightDown;
			    }
			    break;
    		
		    case States.GlideAir or States.GlideFall or States.GlideGround when movement.Position.Y < topBound - 6:
			    movement.GroundSpeed.Value = 0f;
			    break;
	    }
    }

    private void KillUnderBottomBound(ICamera camera)
    {
	    if (data.Water.AirTimer <= 0f) return;
	    if ((int)data.Movement.Position.Y < Math.Max(camera.Boundary.W, camera.TargetBoundary.W)) return; 
	    
	    logic.Kill();
    }
}
