using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Framework.View;

namespace OrbinautFramework3.Objects.Common.Spikes;

public partial class MovableSpikes : Spikes
{
    [Export] private bool _isMoving;
    [Export] private Sprite2D _sprite;
    
    private float _retractTimer;
    private float _retractValue = 8f;
    private float _retractOffset;
    private Rect2 _rectangle;
    private Vector2 _initialPosition;
    

    public override void _Ready()
    {
        base._Ready();
        _initialPosition = Position;
        _rectangle = _sprite.GetRect();
    }

    public override void _Process(double delta)
    {
        if (_isMoving)
        {
            Move();
        }
        
        base._Process(delta);
    }

    private void Move()
    {
        if (_retractTimer > 0f)
        {
            _retractTimer -= Scene.Local.ProcessSpeed;
            if (_retractTimer <= 0f && Views.Local.CheckRectInCameras(_rectangle))
            {
                AudioPlayer.Sound.Play(SoundStorage.SpikesMove);
            }
        }
        /*
        else
        {
            _retractOffset += 8 * _retractValue;
			
            if (abs(retract_offset) >= retract_distance || image_yscale >= 0 && retract_offset <= 0 || image_yscale < 0 && retract_offset >= 0)
            {
                retract_offset = image_yscale >= 0 ? clamp(retract_offset, 0, retract_distance) : clamp(retract_offset, -retract_distance, 0);
                _retractTimer = 60f;
                _retractValue *= -1;
            }
        }

        Position = _initialPosition + new Vector2( *);
        y = ystart + retract_offset;
        */
    }
}