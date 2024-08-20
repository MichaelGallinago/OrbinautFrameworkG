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
public struct Default(PlayerData data, IPlayerLogic logic)
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
        
        return data.Sprite.Animation is Animations.Duck or Animations.GlideLand && data.Input.Down.Down;
    }
    
    private bool Dash()
    {
        if (!SharedData.Dash || data.Node.Type != PlayerNode.Types.Sonic) return false;
        if (data.Id > 0 && data.Cpu.InputTimer <= 0f) return false;
        
        return data.Sprite.Animation == Animations.LookUp && data.Input.Down.Up;
    }
    
    private bool Jump()
    {
        if (data.Movement.IsForcedSpin || !data.Movement.IsGrounded) return false;
        if (!CheckCeilingDistance()) return false;
        
#if S1_PHYSICS || S2_PHYSICS || S3_PHYSICS || SK_PHYSICS
        if (!SharedData.FixJumpSize)
        {
            // Why do they even do that?
            data.Collision.Radius = data.Collision.RadiusNormal;
        }
#endif
	
        if (!data.Movement.IsSpinning)
        {
            data.Node.Position += new Vector2(0f, data.Collision.Radius.Y - data.Collision.RadiusSpin.Y);
            data.Collision.Radius = data.Collision.RadiusSpin;
        }
#if S1_PHYSICS || S2_PHYSICS || S3_PHYSICS || SK_PHYSICS
        else if (!SharedData.NoRollLock)
        {
            data.Movement.IsAirLock = true;
        }
#endif
		
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

        CollisionData collision = data.Collision;
        logic.TileCollider.SetData((Vector2I)data.Node.Position, collision.TileLayer, collision.TileBehaviour);

        Vector2I radius = collision.Radius;
        int distance = collision.TileBehaviour switch
        {
            Constants.TileBehaviours.Floor => logic.TileCollider.FindClosestDistance(
                -radius.X, -radius.Y, radius.X, -radius.Y, true, Constants.Direction.Negative),
			
            Constants.TileBehaviours.RightWall => logic.TileCollider.FindClosestDistance(
                -radius.Y, -radius.X, radius.Y, -radius.X, false, Constants.Direction.Negative),
			
            Constants.TileBehaviours.LeftWall => logic.TileCollider.FindClosestDistance(
                -radius.Y, radius.X, radius.Y, radius.X, false, Constants.Direction.Positive),
			
            Constants.TileBehaviours.Ceiling => maxCeilingDistance,
			
            _ => throw new ArgumentOutOfRangeException()
        };
		
        return distance >= maxCeilingDistance;
    }
}
