using System;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Modules;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Physics.Slopes;

public struct SlopeRepel(PlayerData data)
{
    public void Apply()
    {
        if (!data.Movement.IsGrounded || data.Collision.IsStickToConvex || data.State == States.HammerDash) return;
	
        if (data.Movement.GroundLockTimer > 0f)
        {
            data.Movement.GroundLockTimer -= Scene.Instance.ProcessSpeed;
            return;
        }

        if (Math.Abs(data.Movement.GroundSpeed) >= 2.5f) return;

        if (SharedData.PhysicsType >= PhysicsCore.Types.S3)
        {
            NewSlopeRepel();
            return;
        }

        OriginalSlopeRepel();
    }
    
    private void OriginalSlopeRepel()
    {
        if (Angles.GetQuadrant(data.Movement.Angle) == Angles.Quadrant.Down) return;
        
        data.Movement.GroundSpeed.Value = 0f;	
        data.Movement.GroundLockTimer = 30f;
        data.Movement.IsGrounded = false;
    }

    private void NewSlopeRepel()
    {
        switch (data.Movement.Angle)
        {
            case <= 33.75f or > 326.25f: return;
			
            case > 67.5f and <= 292.5f:
                data.Movement.IsGrounded = false;
                break;
			
            default:
                data.Movement.GroundSpeed.Acceleration = data.Movement.Angle < 180f ? -0.5f : 0.5f;
                break;
        }

        data.Movement.GroundLockTimer = 30f;
    }
}
