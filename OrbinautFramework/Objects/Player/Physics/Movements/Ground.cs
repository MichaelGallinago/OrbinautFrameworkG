using System;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player.Data;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Physics.Movements;

public struct Ground(PlayerData data)
{
    public const float SkidSpeedThreshold = 4f;
    
    public void Move()
    {
        if (!data.Movement.IsGrounded || data.Movement.IsSpinning) return;
        if (data.State is States.SpinDash or States.Dash or States.HammerDash) return;

        CancelGlideLandingAnimation();
		
        if (data.Movement.GroundLockTimer <= 0f)
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
            data.Movement.GroundSpeed.ApplyFriction(data.Physics.Friction);
        }
		
        data.Movement.Velocity.SetDirectionalValue(data.Movement.GroundSpeed, data.Movement.Angle);
    }

    private void CancelGlideLandingAnimation()
    {
        if (data.Visual.Animation is Animations.GlideGround or Animations.GlideLand && 
            (data.Input.Down.Down || data.Movement.GroundSpeed != 0))
        {
            data.Movement.GroundLockTimer = 0f;
        }
    }
    
    private void UpdateMovementGroundAnimation(bool doSkid)
    {
        SetPushAnimation();
		
        Angles.Quadrant quadrant = Angles.GetQuadrant(data.Movement.Angle);
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
        if (data.Visual.SetPushBy != null && data.Node.Sprite.IsFrameChanged)
        {
            data.Visual.Animation = Animations.Push;
        }
    }
    
    private bool SetIdleAnimation(Angles.Quadrant quadrant)
    {
        if (quadrant != Angles.Quadrant.Down || data.Movement.GroundSpeed != 0f) return false;
		
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
        if (Math.Abs(data.Movement.GroundSpeed) < SkidSpeedThreshold) return;
		
        data.Visual.DustTimer = 0f;
        data.Visual.Animation = Animations.Skid;
		
        AudioPlayer.Sound.Play(SoundStorage.Skid);
    }
    
    private bool WalkOnGround(Constants.Direction direction)
    {
        var sign = (float)direction;
		
        if (data.Movement.GroundSpeed * sign < 0f)
        {
            data.Movement.GroundSpeed.Acceleration = sign * data.Physics.Deceleration;
            if (direction == Constants.Direction.Positive == data.Movement.GroundSpeed >= 0f)
            {
                data.Movement.GroundSpeed.Value = 0.5f * sign;
            }
			
            return true;
        }

        if (!SharedData.NoSpeedCap || data.Movement.GroundSpeed * sign < data.Physics.AccelerationTop)
        {
            float acceleration = data.Physics.Acceleration;
            data.Movement.GroundSpeed.Acceleration = acceleration * (float)direction;
			
            switch (direction)
            {
                case Constants.Direction.Positive: data.Movement.GroundSpeed.SetMin( data.Physics.AccelerationTop); break;
                case Constants.Direction.Negative: data.Movement.GroundSpeed.SetMax(-data.Physics.AccelerationTop); break;
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
