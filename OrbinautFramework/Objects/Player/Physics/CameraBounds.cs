using Godot;

namespace OrbinautFramework3.Objects.Player.Physics;

public struct CameraBounds
{
    public void Match()
    {
    	if (IsDead) return;
    	
    	if (!IsCameraTarget(out ICamera camera) && !Scene.Local.Players.First().IsCameraTarget(out camera)) return;

	    ShiftToLeftBound();
	    ShiftToRightBound();
	    ShiftToTopBound();
	    KillUnderBottomBound();
    }

    private void ShiftToLeftBound()
    {
	    if (Position.X + Velocity.X < camera.Boundary.X + 16f)
	    {
		    GroundSpeed.Value = 0f;
		    Velocity.X = 0f;
		    Position = new Vector2(camera.Boundary.X + 16f, Position.Y);
	    }
    }
    
    private void ShiftToRightBound()
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
	    if (Position.X + Velocity.X > rightBound)
	    {
		    GroundSpeed.Value = 0f;
		    Velocity.X = 0f;
		    Position = new Vector2(rightBound, Position.Y);
	    }
    }

    private void ShiftToTopBound()
    {
	    switch (Action)
	    {
		    case Actions.Flight or Actions.Climb:
			    if (Position.Y + Velocity.Y >= camera.Boundary.Y + 16f) break;
    
			    if (Action == Actions.Flight)
			    {
				    Gravity	= GravityType.TailsDown;
			    }

			    Velocity.Y = 0f;
			    Position = new Vector2(Position.X, camera.Boundary.Y + 16f);
			    break;
    		
		    case Actions.Glide when Position.Y < camera.Boundary.Y + 10f:
			    GroundSpeed.Value = 0f;
			    break;
	    }
    }

    private void KillUnderBottomBound()
    {
	    if (AirTimer > 0f && Position.Y > Math.Max(camera.Boundary.W, camera.TargetBoundary.W))
	    {
		    Kill();
	    }
    }
}