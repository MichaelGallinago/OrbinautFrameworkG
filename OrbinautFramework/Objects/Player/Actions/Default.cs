using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.StaticStorages;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Sprite;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Actions;

[FsmSourceGenerator.FsmState("Action")]
public readonly struct Default(PlayerData data, IPlayerLogic logic)
{
    public States EarlyPerform()
    {
        if (!data.Input.Press.Aby) return States.Default;
        if (SpinDash()) return States.SpinDash;
        if (Dash()) return States.Dash;
        if (Jump()) return States.Jump;
        return States.Default;
    }
    
    private bool SpinDash()
    {
        if (!OriginalDifferences.SpinDash || !data.Movement.IsGrounded) return false;
        
        return data.Sprite.Animation is Animations.Duck or Animations.GlideLand && data.Input.Down.Down;
    }
    
    private bool Dash()
    {
        if (!OriginalDifferences.Dash || data.Node.Type != PlayerNode.Types.Sonic) return false;
        if (logic.ControlType.IsCpu && data.Cpu.InputTimer <= 0f) return false;
        
        return data.Sprite.Animation == Animations.LookUp && data.Input.Down.Up;
    }
    
    private bool Jump()
    {
        MovementData movement = data.Movement;
        if (movement.IsForcedRoll || !movement.IsGrounded) return false;
        if (!CheckCeilingDistance()) return false;

        CollisionData collision = data.Collision;
#if S1_PHYSICS || S2_PHYSICS || S3_PHYSICS || SK_PHYSICS
        if (!Improvements.FixJumpSize)
        {
            // Why do they even do that?
            collision.Radius = collision.RadiusNormal;
        }
#endif
        if (!movement.IsSpinning)
        {
            movement.Position.Y += collision.Radius.Y - collision.RadiusSpin.Y;
            collision.Radius = collision.RadiusSpin;
        }
#if S1_PHYSICS || S2_PHYSICS || S3_PHYSICS || SK_PHYSICS
        else if (!Improvements.NoRollLock)
        {
            movement.IsAirLock = true;
        }
#endif
        float radians = Mathf.DegToRad(movement.Angle);
        var velocity = new Vector2(MathF.Sin(radians), MathF.Cos(radians));
        movement.Velocity += data.Physics.JumpSpeed * velocity;
        
        movement.IsGrounded = false;
        movement.IsCorePhysicsSkipped = true;
		
        collision.OnObject = null;
        collision.IsStickToConvex = false;
		
        data.Visual.SetPushBy = null;
        
        AudioPlayer.Sound.Play(SoundStorage.Jump);
        
        return true;
    }
    
    private bool CheckCeilingDistance()
    {
        const int maxCeilingDistance = 6;

        CollisionData collision = data.Collision;
        logic.TileCollider.SetData((Vector2I)data.Movement.Position, collision.TileLayer, collision.TileBehaviour);

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
