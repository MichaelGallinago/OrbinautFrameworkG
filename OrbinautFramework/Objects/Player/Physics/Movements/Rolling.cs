using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Modules;

namespace OrbinautFramework3.Objects.Player.Physics.Movements;

public struct Rolling(PlayerData data)
{
    public void Roll()
    {
        if (!data.Physics.IsGrounded || !data.Physics.IsSpinning) return;
		
        if (data.Physics.GroundLockTimer <= 0f)
        {
            if (data.Input.Down.Left)
            {
                RollOnGround(Constants.Direction.Negative);
            }
			
            if (data.Input.Down.Right)
            {
                RollOnGround(Constants.Direction.Positive);
            }
        }

        data.Physics.GroundSpeed.ApplyFriction(PhysicParams.FrictionRoll);

        if (data.Physics.IsForcedSpin)
        {
            ForceSpin();
        }
        else
        {
            StopSpinning();
        }
	
        data.Physics.Velocity.SetDirectionalValue(data.Physics.GroundSpeed, data.Rotation.Angle);
        data.Physics.Velocity.ClampX(-16f, 16f);
    }
    
    private void RollOnGround(Constants.Direction direction)
    {
        var sign = (float)direction;
        float absoluteSpeed = sign * data.Physics.GroundSpeed;
		
        if (absoluteSpeed >= 0f)
        {
            data.Visual.SetPushBy = null;
            data.Visual.Facing = direction;
            return;
        }
		
        data.Physics.GroundSpeed.Acceleration = sign * PhysicParams.DecelerationRoll;
        if (direction == Constants.Direction.Positive == data.Physics.GroundSpeed < 0f) return;
        data.Physics.GroundSpeed.Value = sign * 0.5f;
    }
    
    private void StopSpinning()
    {
        if (data.Physics.GroundSpeed != 0f)
        {
            if (SharedData.PhysicsType != PhysicsCore.Types.SK || Math.Abs(data.Physics.GroundSpeed) >= 0.5f) return;
        }
		
        data.PlayerNode.Position += new Vector2(0f, data.Collision.Radius.Y - data.Collision.RadiusNormal.Y);
		
        data.Physics.IsSpinning = false;
        data.Collision.Radius = data.Collision.RadiusNormal;
        data.Visual.Animation = Animations.Idle;
    }
	
    private void ForceSpin()
    {
        if (SharedData.PhysicsType == PhysicsCore.Types.CD)
        {
            if (data.Physics.GroundSpeed.Value is >= 0f and < 2f)
            {
                data.Physics.GroundSpeed.Value = 2f;
            }
            return;
        }
		
        if (data.Physics.GroundSpeed != 0f) return;
        data.Physics.GroundSpeed.Value = 
            SharedData.PhysicsType == PhysicsCore.Types.S1 ? 2f : 4f * (float)data.Visual.Facing;
    }
}
