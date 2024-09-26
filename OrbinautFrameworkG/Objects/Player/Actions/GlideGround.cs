using System;
using Godot;
using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework.SceneModule;
using OrbinautFrameworkG.Framework.StaticStorages;
using OrbinautFrameworkG.Framework.Tiles;
using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Logic;
using OrbinautFrameworkG.Objects.Player.Sprite;
using static OrbinautFrameworkG.Objects.Player.ActionFsm;

namespace OrbinautFrameworkG.Objects.Player.Actions;

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
        movement.Velocity.X.AddAcceleration(Math.Sign(movement.Velocity.X) * slideFriction);
        switch (speedX)
        {
            case > 0f: movement.Velocity.X.SetMax(0f); break;
            case < 0f: movement.Velocity.X.SetMin(0f); break;
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
        movement.GroundSpeed = 0f;
        
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
