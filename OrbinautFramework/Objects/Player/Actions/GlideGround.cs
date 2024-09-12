using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Sprite;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Actions;

[FsmSourceGenerator.FsmState("Action")]
public struct GlideGround(PlayerData data, IPlayerLogic logic)
{
    private readonly GlideCollisionLogic _collision = new(data, logic);
    private float _dustTimer;
    
    public States Perform()
    {
        UpdateGroundVelocityX();
        
        if (StopSliding()) return States.Default;
        SpawnDustParticles();
        return States.GlideGround;
    }
    
    public States LatePerform()
    {
        _collision.CollideWallsAndCeiling(out Angles.Quadrant moveQuadrant);

        return moveQuadrant != Angles.Quadrant.Up && !SlideOnFloor() ? States.GlideFall : States.GlideGround;
    }
    
    private bool SlideOnFloor()
    {
        Vector2I radius = data.Collision.Radius;
        (int floorDistance, float floorAngle) = logic.TileCollider.FindClosestTile(
            -radius.X, radius.Y, radius.X, radius.Y, true, Constants.Direction.Positive);
	
        if (floorDistance > 14) return false;
			
        data.Movement.Position.Y += floorDistance;
        data.Movement.Angle = floorAngle;
        return true;
    }
    
    private void UpdateGroundVelocityX()
    {
        MovementData movement = data.Movement;
        if (!data.Input.Down.Aby)
        {
            movement.Velocity.X = 0f;
            return;
        }
		
        const float slideFriction = -0.09375f;
		
        float speedX = movement.Velocity.X;
        movement.Velocity.AccelerationX = Math.Sign(movement.Velocity.X) * slideFriction;
        switch (speedX)
        {
            case > 0f: movement.Velocity.MaxX(0f); break;
            case < 0f: movement.Velocity.MinX(0f); break;
        }
    }

    private bool StopSliding()
    {
        MovementData movement = data.Movement;
        if (movement.Velocity.X != 0f) return false;
        
        logic.Land();
        data.Visual.OverrideFrame = 1;
			
        data.Sprite.Animation = Animations.GlideGround;
        movement.GroundLockTimer = 16f;
        movement.GroundSpeed.Value = 0f;
			
        return true;
    }

    private void SpawnDustParticles()
    {
        if (_dustTimer % 4f < Scene.Instance.Speed)
        {
            //TODO: obj_dust_skid
            //instance_create(x, y + data.Collision.Radius.Y, obj_dust_skid);
        }
				
        if (_dustTimer > 0f && _dustTimer % 8f < Scene.Instance.Speed)
        {
            AudioPlayer.Sound.Play(SoundStorage.Slide);
        }
					
        _dustTimer += Scene.Instance.Speed;
    }
}
