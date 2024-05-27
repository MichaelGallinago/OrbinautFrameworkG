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

    private const float Force = 7f;
    
    [Export] private HitsLimit _hitsLimit = HitsLimit.Sonic2;
    [Export] private AdvancedAnimatedSprite _sprite;
    
    private int _state;
    private int _hitsLeft;

    public Bumper() => SetHitBox(new Vector2I(8, 8));
    
    public override void _Ready()
    {
        base._Ready();
        _sprite.AnimationFinished += OnAnimationFinished;
    }
    
    public override void _Process(double delta) => CheckCollisionWithPlayers();
    
    protected override void Init() => _hitsLeft = (int)_hitsLimit;

    private void CheckCollisionWithPlayers()
    {
        foreach (Player player in Scene.Local.Players.Values)
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
            Player.IncreaseComboScore();
            
            break;
        }
    }

    private static void BumpPlayer(PlayerData player, Vector2 position)
    {
        if (player.Action == Actions.Carried)
        {
            player.Action = Actions.None;
        }
        
        player.IsJumping = false;
        player.IsGrounded = false;
        player.IsAirLock = false;
        player.SetPushAnimationBy = null;
        
        float radians = Mathf.DegToRad(Angles.GetVector256(player.Position - position));
        player.Velocity.Vector = Force * new Vector2(MathF.Sin(radians), MathF.Cos(radians));
    }

    private void OnAnimationFinished()
    {
        if (_sprite.Animation != "Bump") return; 
        _sprite.Play("Default");
    }
}
