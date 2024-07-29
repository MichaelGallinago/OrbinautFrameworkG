using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;

namespace OrbinautFramework3.Objects.Player.Physics.Movements;

public struct Ground
{
    public void Move()
    {
        if (!IsGrounded || IsSpinning) return;
        if (Action is Actions.SpinDash or Actions.Dash or Actions.HammerDash) return;
		
        // Cancel Knuckles' glide-landing animation
        if (Animation is Animations.GlideGround or Animations.GlideLand && (Input.Down.Down || GroundSpeed != 0))
        {
            GroundLockTimer = 0f;
        }
		
        if (GroundLockTimer <= 0f)
        {
            var doSkid = false;
			
            if (Input.Down.Left)
            {
                doSkid |= WalkOnGround(Constants.Direction.Negative);
            }
			
            if (Input.Down.Right)
            {
                doSkid |= WalkOnGround(Constants.Direction.Positive);
            }
			
            UpdateMovementGroundAnimation(doSkid);
        }
		
        if (Input.Down is { Left: false, Right: false })
        {
            GroundSpeed.ApplyFriction(PhysicParams.Friction);
        }
		
        Velocity.SetDirectionalValue(GroundSpeed, Angle);
    }
    
    private void UpdateMovementGroundAnimation(bool doSkid)
    {
        // Set push animation once animation frame changes
        if (SetPushAnimationBy != null && IsAnimationFrameChanged)
        {
            Animation = Animations.Push;
        }
		
        Angles.Quadrant quadrant = Angles.GetQuadrant(Angle);
        if (SetIdleAnimation(quadrant)) return;
			
        if (Animation == Animations.Skid) return;
		
        if (Animation is not (Animations.Push or Animations.Skid))
        {
            Animation = Animations.Move;
        }

        // Perform skid
        if (quadrant != Angles.Quadrant.Down || !doSkid) return;
        if (Math.Abs(GroundSpeed) < PlayerConstants.SkidSpeedThreshold) return;
		
        ActionValue2 = 0f; // We'll use this as a timer to spawn dust particles in UpdateStatus()
        Animation = Animations.Skid;
		
        AudioPlayer.Sound.Play(SoundStorage.Skid);
    }
    
    private bool SetIdleAnimation(Angles.Quadrant quadrant)
    {
        if (quadrant != Angles.Quadrant.Down || GroundSpeed != 0f) return false;
		
        if (Input.Down.Up)
        {
            Animation = Animations.LookUp;
        }
        else if (Input.Down.Down)
        {
            Animation = Animations.Duck;
        }
        else if (Animation != Animations.Wait)
        {
            Animation = Animations.Idle;
        }
			
        SetPushAnimationBy = null;
        return true;
    }
    
    private bool WalkOnGround(Constants.Direction direction)
    {
        var sign = (float)direction;
		
        if (GroundSpeed * sign < 0f)
        {
            GroundSpeed.Acceleration = sign * PhysicParams.Deceleration;
            if (direction == Constants.Direction.Positive == GroundSpeed >= 0f)
            {
                GroundSpeed.Value = 0.5f * sign;
            }
			
            return true;
        }

        if (!SharedData.NoSpeedCap || GroundSpeed * sign < PhysicParams.AccelerationTop)
        {
            float acceleration = PhysicParams.Acceleration;
            GroundSpeed.Acceleration = acceleration * (float)direction;
			
            switch (direction)
            {
                case Constants.Direction.Positive: GroundSpeed.Min( PhysicParams.AccelerationTop); break;
                case Constants.Direction.Negative: GroundSpeed.Max(-PhysicParams.AccelerationTop); break;
            }
        }
		
        // Cancel skid animation
        if (Animation == Animations.Skid)
        {
            Animation = Animations.Move;
        }
		
        // Turn around
        if (Facing == direction) return false;
		
        Animation = Animations.Move;
        Facing = direction;
        SetPushAnimationBy = null;
        OverrideAnimationFrame = 0;
		
        return false;
    }
}
