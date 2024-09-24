using System;
using Godot;
using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.View;

namespace OrbinautFrameworkG.Objects.Common.Spikes;

public partial class MovingSpikes : Spikes
{
    [Export] private bool _isMoving = true;
    [Export] private int _retractDistance = 32;
    
    private Vector2 _initialPosition;
    private Vector2 _retractMultiplier;
    
    private float _retractTimer;
    private float _retractOffset;
    private float _retractTarget;
    
    public override void _Ready()
    {
        base._Ready();
        
        _retractMultiplier = new Vector2(MathF.Sin(Rotation), MathF.Cos(Rotation));
        _initialPosition = Position;
        
        _retractTarget = _retractDistance;
    }

    public override void _Process(double delta)
    {
        if (_isMoving)
        {
            Move();
        }
        
        base._Process(delta);
    }
    
    public void Reset()
    {
        _retractTimer = 0f;
        _retractOffset = 0f;
        _retractTarget = _retractDistance;
    }
    
    private void Move()
    {
        UpdateRetraction();
        Position = _initialPosition + _retractMultiplier * _retractOffset;
    }
    
    private void UpdateRetraction()
    {
        if (Wait()) return;

        const float retractSpeed = 8f;
        float delta = retractSpeed * Scene.Instance.Speed;
        _retractOffset = _retractOffset.MoveTowardChecked(_retractTarget, delta, out bool isFinished);
        
        if (!isFinished) return;
        
        const float waitingTime = 60f;
        _retractTimer = waitingTime;
        _retractTarget = _retractTarget == 0f ? _retractDistance : 0f;
    }
    
    private bool Wait()
    {
        if (_retractTimer <= 0f) return false;
        
        _retractTimer -= Scene.Instance.Speed;
        if (_retractTimer <= 0f && Views.Instance.CheckRectInCameras(Sprite.GetRect()))
        {
            AudioPlayer.Sound.Play(SoundStorage.SpikesMove);
        }
        
        return true;
    }
}
