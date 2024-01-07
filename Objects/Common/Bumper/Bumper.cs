using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Animations;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.Tiles;

namespace OrbinautFramework3.Objects.Common.Bumper;

using Player;

public partial class Bumper : BaseObject
{
    // There is no limit in Sonic 2. Set to 10 to match S1 behaviour
    [Export] private int _hitsLeft = -1;
    [Export] private AdvancedAnimatedSprite _sprite;
    private int _state;

    public Bumper()
    {
        SetHitbox(new Vector2I(8, 8));
    }

    public override void _Process(double delta)
    {
        foreach (Player player in Player.Players)
        {
            if (player.IsHurt || !CheckCollision(player, Constants.CollisionSensor.Hitbox)) continue;
		    
            if (_sprite.Animation == "Default")
            {
                _sprite.Play("Bump");
                _sprite.NextAnimation = "Default";
            }
		
            //TODO: audio
            //audio_play_sfx(sfx_bumper);
            
            float radians = Mathf.DegToRad(Angles.GetVector256(player.Position - Position));
		
            if (player.Action != Player.Actions.Glide || player.ActionState == (int)Player.GlideStates.Fall)
            {
                float bumpSpeed = 7f * MathF.Sin(radians);
			
                if (player.IsGrounded)
                {
                    player.GroundSpeed = bumpSpeed;
                }
                else
                {
                    player.Speed.X = bumpSpeed;
                }
            }
            
            player.Speed.Y = 7f * MathF.Cos(radians);
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
}