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
        if (!data.Physics.IsGrounded || data.Collision.IsStickToConvex || data.State == States.HammerDash) return;
	
        if (data.Physics.GroundLockTimer > 0f)
        {
            data.Physics.GroundLockTimer -= Scene.Instance.ProcessSpeed;
            return;
        }

        if (Math.Abs(data.Physics.GroundSpeed) >= 2.5f) return;

        if (SharedData.PhysicsType >= PhysicsCore.Types.S3)
        {
            NewSlopeRepel();
            return;
        }

        OriginalSlopeRepel();
    }
    
    private void OriginalSlopeRepel()
    {
        if (Angles.GetQuadrant(data.Rotation.Angle) == Angles.Quadrant.Down) return;
        
        data.Physics.GroundSpeed.Value = 0f;	
        data.Physics.GroundLockTimer = 30f;
        data.Physics.IsGrounded = false;
    }

    private void NewSlopeRepel()
    {
        switch (data.Rotation.Angle)
        {
            case <= 33.75f or > 326.25f: return;
			
            case > 67.5f and <= 292.5f:
                data.Physics.IsGrounded = false;
                break;
			
            default:
                data.Physics.GroundSpeed.Acceleration = data.Rotation.Angle < 180f ? -0.5f : 0.5f;
                break;
        }

        data.Physics.GroundLockTimer = 30f;
    }
}
