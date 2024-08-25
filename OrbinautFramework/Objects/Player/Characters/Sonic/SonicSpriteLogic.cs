using System;
using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player.Sprite.Characters;

[GlobalClass]
public partial class SonicSpriteLogic : SpriteLogic
{
    protected override void Animate()
    {
        SetType(Data.Type, Data.Speed);

        if (!Player.Data.Super.IsSuper || Data.Type != Animations.Walk) return;
        
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
                Player.Data.Input.Down is { Left: false, Right: false } || 
                Math.Abs(Player.GroundSpeed) < Physics.Movements.Ground.SkidSpeedThreshold => Animations.Move, 
            _ => Data.Animation
        };
    }
    
    protected override void UpdateSpeed() => Data.Speed = Player.Animation switch
    {
        Animations.Move => GetGroundSpeed(9f),
        Animations.Push => GetGroundSpeed(9f),
        Animations.Spin => GetGroundSpeed(5f),
        _ => 1f
    };
	
    protected override void UpdateType() => Data.Type = Player.Animation switch
    {
        Animations.Move => Player.Data.Super.IsSuper ? 
            GetMoveAnimation(false, 8f) :
            GetMoveAnimation(SharedData.Dash, 6f),
        _ => Player.Animation
    };
}
