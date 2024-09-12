using System;
using Godot;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Extensions;

namespace OrbinautFramework3.Objects.Player.Sprite;

public abstract partial class SpriteLogic(PlayerData playerData, ISpriteNode spriteNode) : Resource
{
    public SpriteData Data { get; } = new();
    public ISpriteNode Node { get; } = spriteNode;

    public void Process()
    {
        Animate();
        UpdateScale();
        OverrideFrame();
		
        Data.IsFrameChanged = false;
        Data.IsFinished = false;
    }
    
    public virtual void OnFinished()
    {
        Data.IsFinished = true;
        Data.Animation = Data.Animation switch
        {
            Animations.Bounce or Animations.Breathe or Animations.Flip or Animations.Transform => Animations.Move,
            Animations.Skid when
                playerData.Input.Down is { Left: false, Right: false } || 
                Math.Abs(playerData.Movement.GroundSpeed) < Physics.Movements.Ground.SkidSpeedThreshold 
                    => Animations.Move, 
            _ => Data.Animation
        };
    }

    public void OnAnimationChanged(Animations animation)
    {
        UpdateSpeed();
        UpdateType();
        Data.FrameCount = Node.SpriteFrames.GetFrameCount(Data.Type.ToStringFast());
    }
    
    protected abstract void Animate();
    protected abstract void UpdateType();
    protected abstract void UpdateSpeed();
    
    protected Animations GetMoveAnimation(bool canDash, float runThreshold)
    {
        const float dashThreshold = 10f;
		
        float speed = Math.Abs(playerData.Movement.GroundSpeed);

        if (speed < runThreshold) return Animations.Walk;
        return canDash && speed >= dashThreshold ? Animations.Dash : Animations.Run;
    }
	
    protected float GetGroundSpeed(float speedBound)
    {
        return 1f / MathF.Floor(Math.Max(1f, speedBound - Math.Abs(playerData.Movement.GroundSpeed)));
    }

    protected void SetType(Animations type, float speed)
    {
        Node.SetAnimation(type.ToStringFast(), speed);
    }

    protected void SetType(Animations type, int startFrame, float speed)
    {
        Node.SetAnimation(type.ToStringFast(), startFrame, speed);
    }
    
    private void OverrideFrame()
    {
        VisualData visual = playerData.Visual;
        if (visual.OverrideFrame == null) return;
        Node.Frame = (int)visual.OverrideFrame;
        visual.OverrideFrame = null;
    }

    private void UpdateScale()
    {
        if (Data.Animation == Animations.Spin && !Data.IsFrameChanged) return;
        Node.Scale = new Vector2(Math.Abs(Node.Scale.X) * (float)playerData.Visual.Facing, Node.Scale.Y);
    }
}
