﻿using System;
using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.StaticStorages;
using OrbinautFrameworkG.Framework.Tiles;
using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Logic;
using OrbinautFrameworkG.Objects.Player.Sprite;
using static OrbinautFrameworkG.Objects.Player.ActionFsm;

namespace OrbinautFrameworkG.Objects.Player.Physics.Movements;

public readonly struct Ground(PlayerData data, IPlayerLogic logic)
{
    public const float SkidSpeedThreshold = 4f;
    
    public void Move()
    {
        if (logic.Action is States.SpinDash or States.Dash or States.HammerDash) return;
        
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
        if (data.Sprite.Animation is Animations.GlideGround or Animations.GlideLand && 
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
			
        if (data.Sprite.Animation == Animations.Skid) return;
		
        if (data.Sprite.Animation is not (Animations.Push or Animations.Skid))
        {
            data.Sprite.Animation = Animations.Move;
        }
        
        if (!doSkid || quadrant != Angles.Quadrant.Down) return;
        PerformSkid();
    }

    private void SetPushAnimation()
    {
        if (data.Visual.SetPushBy != null && data.Sprite.IsFrameChanged)
        {
            data.Sprite.Animation = Animations.Push;
        }
    }
    
    private bool SetIdleAnimation(Angles.Quadrant quadrant)
    {
        if (quadrant != Angles.Quadrant.Down || data.Movement.GroundSpeed != 0f) return false;
		
        if (data.Input.Down.Up)
        {
            data.Sprite.Animation = Animations.LookUp;
        }
        else if (data.Input.Down.Down)
        {
            data.Sprite.Animation = Animations.Duck;
        }
        else if (data.Sprite.Animation != Animations.Wait)
        {
            data.Sprite.Animation = Animations.Idle;
        }
			
        data.Visual.SetPushBy = null;
        return true;
    }

    private void PerformSkid()
    {
        if (Math.Abs(data.Movement.GroundSpeed) < SkidSpeedThreshold) return;
		
        data.Visual.DustTimer = 0f;
        data.Sprite.Animation = Animations.Skid;
		
        AudioPlayer.Sound.Play(SoundStorage.Skid);
    }
    
    private bool WalkOnGround(Constants.Direction direction)
    {
        var sign = (float)direction;
        
        if (data.Movement.GroundSpeed * sign < 0f)
        {
            data.Movement.GroundSpeed.AddAcceleration(sign * data.Physics.Deceleration);
            if (direction == Constants.Direction.Positive == data.Movement.GroundSpeed >= 0f)
            {
                data.Movement.GroundSpeed = 0.5f * sign;
            }
			
            return true;
        }
        
        if (!Improvements.NoSpeedCap || data.Movement.GroundSpeed * sign < data.Physics.AccelerationTop)
        {
            float acceleration = data.Physics.Acceleration;
            data.Movement.GroundSpeed.AddAcceleration(acceleration * (float)direction);
            
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
        if (data.Sprite.Animation != Animations.Skid) return;
        data.Sprite.Animation = Animations.Move;
    }

    private void TurnAround(Constants.Direction direction)
    {
        if (data.Visual.Facing == direction) return;
		
        data.Sprite.Animation = Animations.Move;
        data.Visual.Facing = direction;
        data.Visual.SetPushBy = null;
        data.Visual.OverrideFrame = 0;
    }
}
