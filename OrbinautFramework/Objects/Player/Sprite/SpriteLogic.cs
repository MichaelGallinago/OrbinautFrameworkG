using System;
using Godot;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Extensions;

namespace OrbinautFramework3.Objects.Player.Sprite;

public abstract partial class SpriteLogic : Resource
{
    protected SpriteData Data { get; private set; }
    protected IPlayer Player { get; private set; }
    
    public SpriteData SetPlayer(IPlayer player, ISpriteNode spriteNode)
    {
        Player = player;
        return Data = new SpriteData(spriteNode);
    }

    public void Process()
    {
        Animate();
        UpdateScale();
        OverrideFrame();
		
        Data.IsFrameChanged = false;
        Data.IsFinished = false;
    }
    
    public void UpdateData()
    {
        UpdateSpeed();
        UpdateType();
        Data.FrameCount = Data.Node.SpriteFrames.GetFrameCount(Data.Node.Animation);
    }
    
    public virtual void OnFinished()
    {
        Data.IsFinished = true;
        Player.Data.Visual.Animation = Player.Data.Visual.Animation switch
        {
            Animations.Bounce or Animations.Breathe or Animations.Flip or Animations.Transform => Animations.Move,
            Animations.Skid when
                Player.Data.Input.Down is { Left: false, Right: false } || 
                Math.Abs(Player.GroundSpeed) < Physics.Movements.Ground.SkidSpeedThreshold => Animations.Move, 
            _ => Player.Data.Visual.Animation
        };
    }
    
    protected abstract void Animate();
    protected abstract void UpdateType();
    protected abstract void UpdateSpeed();
    
    protected Animations GetMoveAnimation(bool canDash, float runThreshold)
    {
        const float dashThreshold = 10f;
		
        float speed = Math.Abs(Player.GroundSpeed);

        if (speed < runThreshold) return Animations.Walk;
        return canDash && speed >= dashThreshold ? Animations.Dash : Animations.Run;
    }
	
    protected float GetGroundSpeed(float speedBound)
    {
        return 1f / MathF.Floor(Math.Max(1f, speedBound - Math.Abs(Player.GroundSpeed)));
    }

    protected void SetType(Animations type, float speed)
    {
        Data.Node.SetAnimation(type.ToStringFast(), speed);
    }

    protected void SetType(Animations type, int startFrame, float speed)
    {
        Data.Node.SetAnimation(type.ToStringFast(), startFrame, speed);
    }
    
    private void OverrideFrame()
    {
        VisualData visual = Player.Data.Visual;
        if (visual.OverrideFrame == null) return;
        Data.Node.Frame = (int)visual.OverrideFrame;
        visual.OverrideFrame = null;
    }

    private void UpdateScale()
    {
        if (Player.Animation == Animations.Spin && !Data.IsFrameChanged) return;
        Data.Node.Scale = new Vector2(Math.Abs(Data.Node.Scale.X) * (float)Player.Facing, Data.Node.Scale.Y);
    }
}