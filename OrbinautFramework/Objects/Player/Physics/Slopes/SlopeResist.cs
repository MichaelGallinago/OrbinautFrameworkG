using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Physics.Slopes;

public struct SlopeResist(PlayerData data, IPlayerLogic logic)
{
    public void Apply()
    {
        if (data.Movement.IsSpinning)
        {
            ResistRoll();
            return;
        }
        
        ResistWalk();
    }
    
    private void ResistWalk()
    {
        if (!data.Movement.IsGrounded || data.Movement.IsSpinning) return;
        if (logic.Action is States.HammerDash or States.Dash) return;
        if (data.Movement.Angle is >= 135f and < 225f) return;
		
        float slopeGravity = 0.125f * MathF.Sin(Mathf.DegToRad(data.Movement.Angle));
		
        // Decrease ground speed
        if (data.Movement.GroundSpeed != 0f || 
            SharedData.PhysicsType >= PhysicsCore.Types.S3 && Math.Abs(slopeGravity) > 0.05078125f)
        {
            data.Movement.GroundSpeed.Acceleration = -slopeGravity;
        }
    }
    
    private void ResistRoll()
    {
        if (!data.Movement.IsGrounded || !data.Movement.IsSpinning) return;
        if (data.Movement.Angle is >= 135f and < 225f) return; // Exit if we're on ceiling
	
        float angleSine = MathF.Sin(Mathf.DegToRad(data.Movement.Angle));
        float slopeGravity = Math.Sign(data.Movement.GroundSpeed) == Math.Sign(angleSine) ? 0.078125f : 0.3125f;
        data.Movement.GroundSpeed.Acceleration = -slopeGravity * angleSine;
    }
}