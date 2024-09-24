using System;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.StaticStorages;
using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Sprite;

namespace OrbinautFrameworkG.Objects.Player.Characters.Sonic;

public partial class SonicSpriteLogic(PlayerData player, ISpriteNode spriteNode) : SpriteLogic(player, spriteNode)
{
    private readonly PlayerData _player = player;

    protected override void Animate()
    {
        SetType(Data.Type, Data.Speed);

        if (!_player.Super.IsSuper || Data.Type != Animations.Walk) return;
        
        if (Scene.Instance.Time % 4d >= 2d) return;
        int frame = (Node.Frame + Data.FrameCount / 2) % Data.FrameCount;
        Node.SetFrameAndProgress(frame, Node.FrameProgress);
    }

    public override void OnFinished()
    {
        Data.IsFinished = true;
        Data.Animation = Data.Animation switch
        {
            Animations.Bounce or Animations.Breathe or Animations.Flip or Animations.Transform => Animations.Move,
            Animations.Idle => Animations.Wait,
            Animations.Skid when
                _player.Input.Down is { Left: false, Right: false } || 
                Math.Abs(_player.Movement.GroundSpeed) < Physics.Movements.Ground.SkidSpeedThreshold 
                    => Animations.Move, 
            _ => Data.Animation
        };
    }
    
    protected override void UpdateSpeed() => Data.Speed = Data.Animation switch
    {
        Animations.Move => GetGroundSpeed(9f),
        Animations.Push => GetGroundSpeed(9f),
        Animations.Spin => GetGroundSpeed(5f),
        _ => 1f
    };
	
    protected override void UpdateType() => Data.Type = Data.Animation switch
    {
        Animations.Move => _player.Super.IsSuper ? 
            GetMoveAnimation(false, 8f) :
            GetMoveAnimation(OriginalDifferences.Dash, 6f),
        _ => Data.Animation
    };
}
