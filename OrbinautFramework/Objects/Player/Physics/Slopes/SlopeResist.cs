using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Modules;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Physics.Slopes;

public struct SlopeResist(PlayerData data)
{
    public void Apply()
    {
        if (data.Physics.IsSpinning)
        {
            ResistRoll();
            return;
        }
        
        ResistWalk();
    }
    
    private void ResistWalk()
    {
        if (!data.Physics.IsGrounded || data.Physics.IsSpinning || data.State is States.HammerDash or States.Dash) return;
        if (data.Rotation.Angle is >= 135f and < 225f) return; // Exit if we're on ceiling
		
        float slopeGravity = 0.125f * MathF.Sin(Mathf.DegToRad(data.Rotation.Angle));
		
        // Decrease ground speed
        if (data.Physics.GroundSpeed != 0f || 
            SharedData.PhysicsType >= PhysicsCore.Types.S3 && Math.Abs(slopeGravity) > 0.05078125f)
        {
            data.Physics.GroundSpeed.Acceleration = -slopeGravity;
        }
    }
    
    private void ResistRoll()
    {
        if (!data.Physics.IsGrounded || !data.Physics.IsSpinning) return;
        if (data.Rotation.Angle is >= 135f and < 225f) return; // Exit if we're on ceiling
	
        float angleSine = MathF.Sin(Mathf.DegToRad(data.Rotation.Angle));
        float slopeGravity = Math.Sign(data.Physics.GroundSpeed) == Math.Sign(angleSine) ? 0.078125f : 0.3125f;
        data.Physics.GroundSpeed.Acceleration = -slopeGravity * angleSine;
    }
}