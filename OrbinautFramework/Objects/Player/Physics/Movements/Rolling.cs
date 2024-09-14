using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.Physics.Movements;

public readonly struct Rolling(PlayerData data)
{
    public void Roll()
    {
        MovementData movement = data.Movement;
        if (movement.GroundLockTimer <= 0f)
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

        movement.GroundSpeed.ApplyFriction(data.Physics.FrictionRoll);

        if (movement.IsForcedRoll)
        {
            ForceSpin();
        }
        else
        {
            StopSpinning();
        }
	
        movement.Velocity.SetDirectionalValue(movement.GroundSpeed, movement.Angle);
        movement.Velocity.X.SetClamp(-16f, 16f);
    }
    
    private void RollOnGround(Constants.Direction direction)
    {
        var sign = (float)direction;
        MovementData movement = data.Movement;
        float absoluteSpeed = sign * movement.GroundSpeed;
		
        if (absoluteSpeed >= 0f)
        {
            data.Visual.SetPushBy = null;
            data.Visual.Facing = direction;
            return;
        }
        
        movement.GroundSpeed.AddAcceleration(sign * data.Physics.DecelerationRoll);
        if (direction == Constants.Direction.Positive == movement.GroundSpeed < 0f) return;
        movement.GroundSpeed = sign * 0.5f;
    }
    
    private void StopSpinning()
    {
        MovementData movement = data.Movement;
#if SK_PHYSICS
        if (movement.GroundSpeed != 0f && Math.Abs(movement.GroundSpeed) >= 0.5f) return;
#else
        if (movement.GroundSpeed != 0f) return;
#endif
		
        movement.Position.Y += data.Collision.Radius.Y - data.Collision.RadiusNormal.Y;
		
        movement.IsSpinning = false;
        data.Collision.Radius = data.Collision.RadiusNormal;
        data.Sprite.Animation = Animations.Idle;
    }
	
    private void ForceSpin()
    {
        MovementData movement = data.Movement;
#if CD_PHYSICS
        if ((float)movement.GroundSpeed is >= 0f and < 2f)
        {
            movement.GroundSpeed = 2f;
        }
#else
        if (movement.GroundSpeed != 0f) return;
    #if S1_PHYSICS
        movement.GroundSpeed = 2f;
    #else
        movement.GroundSpeed = 4f * (float)data.Visual.Facing;
    #endif
#endif
    }
}
