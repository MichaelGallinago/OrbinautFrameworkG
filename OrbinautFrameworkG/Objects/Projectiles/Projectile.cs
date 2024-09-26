using System;
using Godot;
using OrbinautFrameworkG.Framework.MathUtilities;
using OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;
using OrbinautFrameworkG.Framework.SceneModule;
using OrbinautFrameworkG.Framework.Tiles;
using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Spawnable.Shield;

namespace OrbinautFrameworkG.Objects.Projectiles;

public partial class Projectile : InteractiveNode
{
    private AcceleratedVector2 _velocity;
    private float _gravity;
    
    private bool _isReflected;
    
    public override void _Process(double delta)
    {
        Vector2 position = Position;
        ProcessReflection(position);

        position += _velocity.ValueDelta;
        _velocity.ResetInstanceValue();
        _velocity.Y.AddAcceleration(_gravity); //TODO: check if applying acceleration for next frame is right
        
        Position = position;
    }

    public void SetVelocity(Vector2 velocity, float gravity = 0f)
    {
        _velocity = velocity;
        _gravity = gravity;
    }

    private void ProcessReflection(Vector2 position)
    {
        if (_isReflected) return;
        foreach (IPlayer player in Scene.Instance.Players.Values)
        {
            if (!HitBox.CheckPlayerCollision(player.Data, (Vector2I)position)) continue;

            if (player.Data.Node.Shield.Type <= ShieldContainer.Types.Normal &&
                player.Data.Node.Shield.State != ShieldContainer.States.DoubleSpin)
            {
                player.Hurt(position.X);
                continue;
            }
            
            float angle = Mathf.DegToRad(Angles.GetRoundedVector(player.Position - position));
            
            _isReflected = true;
            _velocity.X = -8f * MathF.Cos(angle);
            _velocity.Y = -8f * MathF.Sin(angle);
            _gravity = 0f;

            break;
        }
    }
}