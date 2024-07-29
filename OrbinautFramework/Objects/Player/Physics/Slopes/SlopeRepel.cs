namespace OrbinautFramework3.Objects.Player.Physics.Slopes;

public struct SlopeRepel
{
    public void Apply()
    {
        if (!IsGrounded || IsStickToConvex || Action == Actions.HammerDash) return;
	
        if (GroundLockTimer > 0f)
        {
            GroundLockTimer -= Scene.Local.ProcessSpeed;
            return;
        }

        if (Math.Abs(GroundSpeed) >= 2.5f) return;

        if (SharedData.PlayerPhysics >= PhysicsTypes.S3)
        {
            NewSlopeRepel();
            return;
        }

        OriginalSlopeRepel();
    }
    
    private void OriginalSlopeRepel()
    {
        if (Angles.GetQuadrant(Angle) == Angles.Quadrant.Down) return;
        
        GroundSpeed.Value = 0f;	
        GroundLockTimer = 30f;
        IsGrounded = false;
    }

    private void NewSlopeRepel()
    {
        switch (Angle)
        {
            case <= 33.75f or > 326.25f: return;
			
            case > 67.5f and <= 292.5f:
                IsGrounded = false;
                break;
			
            default:
                GroundSpeed.Acceleration = Angle < 180f ? -0.5f : 0.5f;
                break;
        }

        GroundLockTimer = 30f;
    }
}