using System;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player.Data;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Physics.Movements;

public struct Ground(PlayerData data)
{
    public void Move()
    {
        if (!data.Physics.IsGrounded || data.Physics.IsSpinning) return;
        if (data.State is States.SpinDash or States.Dash or States.HammerDash) return;

        CancelGlideLandingAnimation();
		
        if (data.Physics.GroundLockTimer <= 0f)
        {
            var doSkid = false;
			
            if (data.Input.Down.Left)
            {
                doSkid |= WalkOnGround(Constants.Direction.Negative);
            }
			
            if (data.Input.Down.Right)
            {
                doSkid |= WalkOnGround(Constants.Direction.Positive);
            }
			
            UpdateMovementGroundAnimation(doSkid);
        }
		
        if (data.Input.Down is { Left: false, Right: false })
        {
            data.Physics.GroundSpeed.ApplyFriction(PhysicParams.Friction);
        }
		
        data.Physics.Velocity.SetDirectionalValue(data.Physics.GroundSpeed, data.Rotation.Angle);
    }

    private void CancelGlideLandingAnimation()
    {
        if (data.Visual.Animation is Animations.GlideGround or Animations.GlideLand && 
            (data.Input.Down.Down || data.Physics.GroundSpeed != 0))
        {
            data.Physics.GroundLockTimer = 0f;
        }
    }
    
    private void UpdateMovementGroundAnimation(bool doSkid)
    {
        SetPushAnimation();
		
        Angles.Quadrant quadrant = Angles.GetQuadrant(data.Rotation.Angle);
        if (SetIdleAnimation(quadrant)) return;
			
        if (data.Visual.Animation == Animations.Skid) return;
		
        if (data.Visual.Animation is not (Animations.Push or Animations.Skid))
        {
            data.Visual.Animation = Animations.Move;
        }
        
        if (!doSkid || quadrant != Angles.Quadrant.Down) return;
        PerformSkid();
    }

    private void SetPushAnimation()
    {
        if (data.Visual.SetPushBy != null && data.PlayerNode.Sprite.IsFrameChanged)
        {
            data.Visual.Animation = Animations.Push;
        }
    }
    
    private bool SetIdleAnimation(Angles.Quadrant quadrant)
    {
        if (quadrant != Angles.Quadrant.Down || data.Physics.GroundSpeed != 0f) return false;
		
        if (data.Input.Down.Up)
        {
            data.Visual.Animation = Animations.LookUp;
        }
        else if (data.Input.Down.Down)
        {
            data.Visual.Animation = Animations.Duck;
        }
        else if (data.Visual.Animation != Animations.Wait)
        {
            data.Visual.Animation = Animations.Idle;
        }
			
        data.Visual.SetPushBy = null;
        return true;
    }

    private void PerformSkid()
    {
        if (Math.Abs(data.Physics.GroundSpeed) < PlayerConstants.SkidSpeedThreshold) return;
		
        data.Visual.DustTimer = 0f;
        data.Visual.Animation = Animations.Skid;
		
        AudioPlayer.Sound.Play(SoundStorage.Skid);
    }
    
    private bool WalkOnGround(Constants.Direction direction)
    {
        var sign = (float)direction;
		
        if (data.Physics.GroundSpeed * sign < 0f)
        {
            data.Physics.GroundSpeed.Acceleration = sign * PhysicParams.Deceleration;
            if (direction == Constants.Direction.Positive == data.Physics.GroundSpeed >= 0f)
            {
                data.Physics.GroundSpeed.Value = 0.5f * sign;
            }
			
            return true;
        }

        if (!SharedData.NoSpeedCap || data.Physics.GroundSpeed * sign < PhysicParams.AccelerationTop)
        {
            float acceleration = PhysicParams.Acceleration;
            data.Physics.GroundSpeed.Acceleration = acceleration * (float)direction;
			
            switch (direction)
            {
                case Constants.Direction.Positive: data.Physics.GroundSpeed.SetMin( PhysicParams.AccelerationTop); break;
                case Constants.Direction.Negative: data.Physics.GroundSpeed.SetMax(-PhysicParams.AccelerationTop); break;
            }
        }

        CancelSkidAnimation();
        TurnAround(direction);
		
        return false;
    }

    private void CancelSkidAnimation()
    {
        if (data.Visual.Animation != Animations.Skid) return;
        data.Visual.Animation = Animations.Move;
    }

    private void TurnAround(Constants.Direction direction)
    {
        if (data.Visual.Facing == direction) return;
		
        data.Visual.Animation = Animations.Move;
        data.Visual.Facing = direction;
        data.Visual.SetPushBy = null;
        data.Visual.OverrideFrame = 0;
    }
}
