using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Sprite;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

[FsmSourceGenerator.FsmState("Action")]
public struct Default(PlayerData data)
{
    public States EarlyPerform()
    {
        if (!data.Input.Press.Abc) return States.Default;
        if (SpinDash()) return States.SpinDash;
        if (Dash()) return States.Dash;
        if (Jump()) return States.Jump;
        return States.Default;
    }
    
    private bool SpinDash()
    {
        if (!SharedData.SpinDash || !data.Movement.IsGrounded) return false;
        
        return data.Visual.Animation is Animations.Duck or Animations.GlideLand && data.Input.Down.Down;
    }
    
    private bool Dash()
    {
        if (!SharedData.Dash || data.Node.Type != PlayerNode.Types.Sonic) return false;
        if (data.Id > 0 && CpuInputTimer <= 0f) return false;
        
        return data.Visual.Animation == Animations.LookUp && data.Input.Down.Up;
    }
    
    private bool Jump()
    {
        if (data.Movement.IsForcedSpin || !data.Movement.IsGrounded) return false;
        if (!CheckCeilingDistance()) return false;
        
        if (!SharedData.FixJumpSize && SharedData.PhysicsType != PhysicsCore.Types.CD)
        {
            // Why do they even do that?
            data.Collision.Radius = data.Collision.RadiusNormal;
        }
	
        if (!data.Movement.IsSpinning)
        {
            data.Node.Position += new Vector2(0f, data.Collision.Radius.Y - data.Collision.RadiusSpin.Y);
            data.Collision.Radius = data.Collision.RadiusSpin;
        }
        else if (!SharedData.NoRollLock && SharedData.PhysicsType != PhysicsCore.Types.CD)
        {
            data.Movement.IsAirLock = true;
        }
		
        float radians = Mathf.DegToRad(data.Movement.Angle);
        var velocity = new Vector2(MathF.Sin(radians), MathF.Cos(radians));
        data.Movement.Velocity.Vector += data.Physics.JumpSpeed * velocity;
        
        data.Movement.IsGrounded = false;
        data.Movement.IsCorePhysicsSkipped = true;
		
        data.Collision.OnObject = null;
        data.Collision.IsStickToConvex = false;
		
        data.Visual.SetPushBy = null;
        
        AudioPlayer.Sound.Play(SoundStorage.Jump);

        return true;
    }
    
    private bool CheckCeilingDistance()
    {
        const int maxCeilingDistance = 6; 
		
        data.TileCollider.SetData((Vector2I)data.Node.Position, data.Collision.TileLayer, data.Collision.TileBehaviour);

        Vector2I radius = data.Collision.Radius;
        int distance = data.Collision.TileBehaviour switch
        {
            Constants.TileBehaviours.Floor => data.TileCollider.FindClosestDistance(
                -radius.X, -radius.Y, radius.X, -radius.Y, true, Constants.Direction.Negative),
			
            Constants.TileBehaviours.RightWall => data.TileCollider.FindClosestDistance(
                -radius.Y, -radius.X, radius.Y, -radius.X, false, Constants.Direction.Negative),
			
            Constants.TileBehaviours.LeftWall => data.TileCollider.FindClosestDistance(
                -radius.Y, radius.X, radius.Y, radius.X, false, Constants.Direction.Positive),
			
            Constants.TileBehaviours.Ceiling => maxCeilingDistance,
			
            _ => throw new ArgumentOutOfRangeException()
        };
		
        return distance >= maxCeilingDistance;
    }
}