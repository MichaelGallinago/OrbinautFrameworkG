using System;
using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player.Physics.Movements;

public struct Rolling
{
    public void Roll()
    {
        if (!IsGrounded || !IsSpinning) return;
		
        if (GroundLockTimer <= 0f)
        {
            if (Input.Down.Left)
            {
                RollOnGround(Constants.Direction.Negative);
            }
			
            if (Input.Down.Right)
            {
                RollOnGround(Constants.Direction.Positive);
            }
        }

        GroundSpeed.ApplyFriction(PhysicParams.FrictionRoll);

        if (IsForcedSpin)
        {
            ForceSpin();
        }
        else
        {
            StopSpinning();
        }
	
        Velocity.SetDirectionalValue(GroundSpeed, Angle);
        Velocity.ClampX(-16f, 16f);
    }
    
    private void RollOnGround(Constants.Direction direction)
    {
        var sign = (float)direction;
        float absoluteSpeed = sign * GroundSpeed;
		
        if (absoluteSpeed >= 0f)
        {
            SetPushAnimationBy = null;
            Facing = direction;
            return;
        }
		
        GroundSpeed.Acceleration = sign * PhysicParams.DecelerationRoll;
        if (direction == Constants.Direction.Positive == GroundSpeed < 0f) return;
        GroundSpeed.Value = sign * 0.5f;
    }
    
    private void StopSpinning()
    {
        if (GroundSpeed != 0f)
        {
            if (SharedData.PhysicsType != PhysicsTypes.SK || Math.Abs(GroundSpeed) >= 0.5f) return;
        }
		
        Position += new Vector2(0f, Radius.Y - RadiusNormal.Y);
		
        IsSpinning = false;
        Radius = RadiusNormal;
        Animation = Animations.Idle;
    }
	
    private void ForceSpin()
    {
        if (SharedData.PhysicsType == PhysicsTypes.CD)
        {
            if (GroundSpeed.Value is >= 0f and < 2f)
            {
                GroundSpeed.Value = 2f;
            }
            return;
        }
		
        if (GroundSpeed != 0f) return;
        GroundSpeed.Value = SharedData.PhysicsType == PhysicsTypes.S1 ? 2f : 4f * (float)Facing;
    }
}
