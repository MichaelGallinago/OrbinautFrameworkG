using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.Physics.Movements;

public struct Rolling(PlayerData data)
{
    public void Roll()
    {
        if (!data.Movement.IsGrounded || !data.Movement.IsSpinning) return;
		
        if (data.Movement.GroundLockTimer <= 0f)
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

        data.Movement.GroundSpeed.ApplyFriction(data.Physics.FrictionRoll);

        if (data.Movement.IsForcedSpin)
        {
            ForceSpin();
        }
        else
        {
            StopSpinning();
        }
	
        data.Movement.Velocity.SetDirectionalValue(data.Movement.GroundSpeed, data.Movement.Angle);
        data.Movement.Velocity.ClampX(-16f, 16f);
    }
    
    private void RollOnGround(Constants.Direction direction)
    {
        var sign = (float)direction;
        float absoluteSpeed = sign * data.Movement.GroundSpeed;
		
        if (absoluteSpeed >= 0f)
        {
            data.Visual.SetPushBy = null;
            data.Visual.Facing = direction;
            return;
        }
		
        data.Movement.GroundSpeed.Acceleration = sign * data.Physics.DecelerationRoll;
        if (direction == Constants.Direction.Positive == data.Movement.GroundSpeed < 0f) return;
        data.Movement.GroundSpeed.Value = sign * 0.5f;
    }
    
    private void StopSpinning()
    {
        if (data.Movement.GroundSpeed != 0f)
        {
            if (SharedData.PhysicsType != PhysicsCore.Types.SK || Math.Abs(data.Movement.GroundSpeed) >= 0.5f) return;
        }
		
        data.Node.Position += new Vector2(0f, data.Collision.Radius.Y - data.Collision.RadiusNormal.Y);
		
        data.Movement.IsSpinning = false;
        data.Collision.Radius = data.Collision.RadiusNormal;
        data.Visual.Animation = Animations.Idle;
    }
	
    private void ForceSpin()
    {
        if (SharedData.PhysicsType == PhysicsCore.Types.CD)
        {
            if (data.Movement.GroundSpeed.Value is >= 0f and < 2f)
            {
                data.Movement.GroundSpeed.Value = 2f;
            }
            return;
        }
		
        if (data.Movement.GroundSpeed != 0f) return;
        data.Movement.GroundSpeed.Value = 
            SharedData.PhysicsType == PhysicsCore.Types.S1 ? 2f : 4f * (float)data.Visual.Facing;
    }
}
