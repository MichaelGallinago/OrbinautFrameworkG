using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Animations;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.ObjectBase.AbstractTypes;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Common.Bumper;

using Player;

public partial class Bumper : InteractiveNode
{
    private enum HitsLimit : sbyte
    {
        Sonic1 = 10, Sonic2 = -1
    }

    private const float Force = 7f;
    
    [Export] private HitsLimit _hitsLimit = HitsLimit.Sonic2;
    [Export] private AdvancedAnimatedSprite _sprite;
    
    private int _state;
    private int _hitsLeft;
    
    public override void _Ready()
    {
        base._Ready();
        _hitsLeft = (int)_hitsLimit;
        _sprite.AnimationFinished += OnAnimationFinished;
    }
    
    public override void _Process(double delta) => CheckCollisionWithPlayers();

    private void CheckCollisionWithPlayers()
    {
        foreach (PlayerData player in Scene.Instance.Players.Values)
        {
            if (!CheckPlayerHitBoxCollision(player)) continue;
            
            if (_sprite.Animation == "Default")
            {
                _sprite.Play("Bump");
            }
            
            AudioPlayer.Sound.Play(SoundStorage.Bumper);
            
            BumpPlayer(player, Position);
            
            if (_hitsLeft == 0) break;
            _hitsLeft--;
            
            //TODO: obj_score
            //instance_create(x, y, obj_score);
            PlayerData.IncreaseComboScore();
            
            break;
        }
    }

    private static void BumpPlayer(PlayerData player, Vector2 position)
    {
        if (player.State == ActionFsm.States.Carried)
        {
            player.State = ActionFsm.States.Default;
        }
        
        player.Movement.IsJumping = false;
        player.Movement.IsGrounded = false;
        player.Movement.IsAirLock = false;
        player.Visual.SetPushBy = null;
        
        float radians = Mathf.DegToRad(Angles.GetVector256(player.Node.Position - position));
        player.Movement.Velocity.Vector = Force * new Vector2(MathF.Sin(radians), MathF.Cos(radians));
    }

    private void OnAnimationFinished()
    {
        if (_sprite.Animation != "Bump") return; 
        _sprite.Play("Default");
    }
}
