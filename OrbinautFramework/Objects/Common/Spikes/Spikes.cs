using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.ObjectBase.AbstractTypes;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Modules;
using OrbinautFramework3.Objects.Player.Physics;

namespace OrbinautFramework3.Objects.Common.Spikes;

using Player;

public abstract partial class Spikes : SolidNode
{
    [Export] public bool IsMoving { get; set; }
    
    [Export] private Sprite2D _sprite;
    
    private Constants.CollisionSensor _sensor;
    private int _retractDistance;
    private bool _isFlipped;
    
    private float _retractValue = 8f;
    private float _retractTimer;
    private float _retractOffset;
    private Rect2 _rectangle;
    private Vector2 _initialPosition;
    
    public override void _Ready()
    {
        base._Ready();
        _initialPosition = Position;
        _rectangle = _sprite.GetRect();

        Vector2 size = _rectangle.Size.Abs() * Scale;
        GetDirectionSpecificData(size).Deconstruct(out _isFlipped, out _sensor, out _retractDistance);
    }

    public override void _Process(double delta)
    {
        if (IsMoving)
        {
            Move();
        }
        
        CollideWithPlayers();
    }

    protected abstract void CollideWithPlayer(PlayerData playerNode);
    protected abstract Vector2 GetRetractOffsetVector(float retractOffset);
    protected abstract SpikesDto GetDirectionSpecificData(Vector2 size);

    private void CollideWithPlayers()
    {
        foreach (PlayerData player in Scene.Instance.Players.Values)
        {
            CollideWithPlayer(player);
            if (!CheckSolidCollision(player, _sensor)) continue;
            HurtPlayer(player);
        }
    }

    private void HurtPlayer(PlayerData player)
    {
        player.Hurt(Position.X);
            
        if (!AudioPlayer.Sound.IsPlaying(SoundStorage.Hurt)) return;
            
        AudioPlayer.Sound.Stop(SoundStorage.Hurt);
        AudioPlayer.Sound.Play(SoundStorage.SpikesHurt);
    }
    
    private void Move()
    {
        UpdateRetraction();
        _rectangle.Position = Position = _initialPosition + GetRetractOffsetVector(_retractOffset);
    }
    
    private void UpdateRetraction()
    {
        if (_retractTimer > 0f)
        {
            _retractTimer -= Scene.Instance.ProcessSpeed;
            if (_retractTimer <= 0f && Views.Instance.CheckRectInCameras(_rectangle))
            {
                AudioPlayer.Sound.Play(SoundStorage.SpikesMove);
            }
            return;
        }

        _retractOffset += _retractValue;

        if (Math.Abs(_retractOffset) < _retractDistance && 
            (_isFlipped ? _retractOffset < 0f : _retractOffset > 0f)) return;
            
        _retractOffset = _isFlipped
            ? Math.Clamp(_retractOffset, -_retractDistance, 0f)
            : Math.Clamp(_retractOffset, 0f, _retractDistance);
            
        _retractTimer = 60f;
        _retractValue = -_retractValue;
    }
}
