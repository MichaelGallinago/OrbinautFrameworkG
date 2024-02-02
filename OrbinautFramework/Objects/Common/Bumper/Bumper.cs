using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Animations;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.Tiles;

namespace OrbinautFramework3.Objects.Common.Bumper;

using Player;

public partial class Bumper : BaseObject
{
    private enum HitsLimit : sbyte
    {
        Sonic1 = 10, Sonic2 = -1
    }
    
    [Export] private HitsLimit _hitsLimit = HitsLimit.Sonic2;
    [Export] private AdvancedAnimatedSprite _sprite;
    private int _state;
    private int _hitsLeft;

    public Bumper() => SetHitbox(new Vector2I(8, 8));
    public override void _Ready()
    {
        _hitsLeft = (int)_hitsLimit;
        _sprite.AnimationFinished += OnAnimationFinished;
    }

    public override void _Process(double delta)
    {
        foreach (Player player in PlayerData.Players)
        {
            if (player.IsHurt || !CheckCollision(player, Constants.CollisionSensor.Hitbox)) continue;
		    
            if (_sprite.Animation == "Default")
            {
                _sprite.Play("Bump");
            }
            
            AudioPlayer.PlaySound(SoundStorage.Bumper);
            
            float radians = Mathf.DegToRad(Angles.GetVector256(player.Position - Position));
		
            if (player.Action != Actions.Glide || player.ActionState == (int)GlideStates.Fall)
            {
                float bumpSpeed = 7f * MathF.Sin(radians);
			
                if (player.IsGrounded)
                {
                    player.GroundSpeed = bumpSpeed;
                }
                else
                {
                    player.Velocity.X = bumpSpeed;
                }
            }

            player.Velocity.Y = 7f * MathF.Cos(radians);
            player.IsJumping = false;
            player.IsAirLock = false;
            
            if (_hitsLeft == 0) break;
            
            _hitsLeft--;
            
            //TODO: obj_score
            //instance_create(x, y, obj_score);
            player.IncreaseComboScore();
		
            break;
        }
    }

    private void OnAnimationFinished()
    {
        if (_sprite.Animation != "Bump") return; 
        _sprite.Play("Default");
    }
}
