using System;
using EnumToStringNameSourceGenerator;
using Godot;
using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework.StaticStorages;
using OrbinautFrameworkG.Framework.Animations;
using OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;
using OrbinautFrameworkG.Framework.SceneModule;
using OrbinautFrameworkG.Framework.Tiles;
using OrbinautFrameworkG.Objects.Player.Data;
using static OrbinautFrameworkG.Objects.Player.ActionFsm;

namespace OrbinautFrameworkG.Objects.Common.Bumper;

public partial class Bumper : InteractiveNode
{
    [EnumToStringName] public enum Animations { Default, Bump }
    
    private enum HitsLimit : sbyte { Sonic1 = 10, Sonic2 = -1 }

    private const float Force = 7f;
    
    [Export] private HitsLimit _hitsLimit = HitsLimit.Sonic2;
    [Export] private AdvancedAnimatedSprite _sprite;
    
    private int _state;
    private int _hitsLeft;
    
    public override void _Ready()
    {
        _hitsLeft = (int)_hitsLimit;
        _sprite.AnimationFinished += OnAnimationFinished;
    }
    
    public override void _Process(double delta) => CheckCollisionWithPlayers();

    private void CheckCollisionWithPlayers()
    {
        foreach (IPlayer player in Scene.Instance.Players.Values)
        {
            if (!HitBox.CheckPlayerCollision(player.Data, (Vector2I)Position)) continue;
            
            if (_sprite.Animation == AnimationsStringNames.Default)
            {
                _sprite.Play(AnimationsStringNames.Bump);
            }
            
            AudioPlayer.Sound.Play(SoundStorage.Bumper);
            
            BumpPlayer(player, Position);
            
            if (_hitsLeft == 0) break;
            _hitsLeft--;
            
            //TODO: obj_score
            //instance_create(x, y, obj_score);
            SaveData.IncreaseComboScore();
            
            break;
        }
    }

    private static void BumpPlayer(IPlayer player, Vector2 position)
    {
        if (player.Action == States.Carried)
        {
            player.Action = States.Default;
        }

        PlayerData data = player.Data;
        MovementData movement = data.Movement;

        movement.IsJumping = false;
        movement.IsGrounded = false;
        movement.IsAirLock = false;
        data.Visual.SetPushBy = null;
        
        float radians = Mathf.DegToRad(Angles.GetRoundedVector(data.Node.Position - position));
        movement.Velocity = Force * new Vector2(MathF.Sin(radians), MathF.Cos(radians));
    }

    private void OnAnimationFinished()
    {
        if (_sprite.Animation != AnimationsStringNames.Bump) return; 
        _sprite.Play(AnimationsStringNames.Default);
    }
}
