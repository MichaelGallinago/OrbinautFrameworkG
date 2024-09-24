using System;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.Tiles;
using OrbinautFrameworkG.Objects.Player.Data;

namespace OrbinautFrameworkG.Objects.Player.Physics.Slopes;

public readonly struct SlopeRepel(PlayerData data)
{
    public void Apply()
    {
        if (data.Collision.IsStickToConvex) return;
	
        if (data.Movement.GroundLockTimer > 0f)
        {
            data.Movement.GroundLockTimer -= Scene.Instance.Speed;
            return;
        }

        if (Math.Abs(data.Movement.GroundSpeed) >= 2.5f) return;

        Process();
    }

    private void Process()
    {
#if S3_PHYSICS || SK_PHYSICS
        switch (data.Movement.Angle)
        {
            case <= 33.75f or > 326.25f: return;
			
            case > 67.5f and <= 292.5f:
                data.Movement.IsGrounded = false;
                break;
			
            default:
                data.Movement.GroundSpeed.AddAcceleration(data.Movement.Angle < 180f ? -0.5f : 0.5f);
                break;
        }

        data.Movement.GroundLockTimer = 30f;
#else
        if (Angles.GetQuadrant(data.Movement.Angle) == Angles.Quadrant.Down) return;
        
        data.Movement.GroundSpeed = 0f;	
        data.Movement.GroundLockTimer = 30f;
        data.Movement.IsGrounded = false;
#endif
    }
}
