using System;
using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player.Physics.Slopes;

public struct SlopeResist
{
    public void Apply()
    {
        if (IsSpinning)
        {
            ResistRoll();
            return;
        }
        
        ResistWalk();
    }
    
    private void ResistWalk()
    {
        if (!IsGrounded || IsSpinning || Action is Actions.HammerDash or Actions.Dash) return;
        if (Angle is >= 135f and < 225f) return; // Exit if we're on ceiling
		
        float slopeGravity = 0.125f * MathF.Sin(Mathf.DegToRad(Angle));
		
        // Decrease ground speed
        if (GroundSpeed != 0f || SharedData.PhysicsType >= PhysicsTypes.S3 && Math.Abs(slopeGravity) > 0.05078125f)
        {
            GroundSpeed.Acceleration = -slopeGravity;
        }
    }
    
    private void ResistRoll()
    {
        if (!IsGrounded || !IsSpinning) return;
        if (Angle is >= 135f and < 225f) return; // Exit if we're on ceiling
	
        float angleSine = MathF.Sin(Mathf.DegToRad(Angle));
        float slopeGravity = Math.Sign(GroundSpeed) == Math.Sign(angleSine) ? 0.078125f : 0.3125f;
        GroundSpeed.Acceleration = -slopeGravity * angleSine;
    }
}