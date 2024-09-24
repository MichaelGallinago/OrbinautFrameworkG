using System;
using Godot;
using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Logic;
using static OrbinautFrameworkG.Objects.Player.ActionFsm;

namespace OrbinautFrameworkG.Objects.Player.Physics.Slopes;

public readonly struct SlopeResist(PlayerData data, IPlayerLogic logic)
{
    public void ResistWalk()
    {
        if (data.Movement.Angle is >= 135f and < 225f) return;
        if (logic.Action is States.HammerDash or States.Dash) return;

#if S3_PHYSICS || SK_PHYSICS
        float slopeGravity = 0.125f * MathF.Sin(Mathf.DegToRad(data.Movement.Angle));
        if (data.Movement.GroundSpeed == 0f && Math.Abs(slopeGravity) <= 0.05078125f) return;
#else
        if (data.Movement.GroundSpeed == 0f) return;
        float slopeGravity = 0.125f * MathF.Sin(Mathf.DegToRad(data.Movement.Angle));
#endif
        data.Movement.GroundSpeed.AddAcceleration(-slopeGravity);
    }
    
    public void ResistRoll()
    {
        if (data.Movement.Angle is >= 135f and < 225f) return;
        
        float angleSine = MathF.Sin(Mathf.DegToRad(data.Movement.Angle));
        float slopeGravity = Math.Sign(data.Movement.GroundSpeed) == Math.Sign(angleSine) ? 0.078125f : 0.3125f;
        data.Movement.GroundSpeed.AddAcceleration(-slopeGravity * angleSine);
    }
}