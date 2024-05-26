using System;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.Tiles;

namespace OrbinautFramework3.Objects.Common.Spikes;

using Player;

public partial class Spikes : BaseObject
{
    private Constants.CollisionSensor _sensor;
    
    public override void _Ready()
    {
        base._Ready();
        
        _sensor = Angles.GetQuadrant(RotationDegrees) switch
        {
            Angles.Quadrant.Up => Constants.CollisionSensor.Top,
            Angles.Quadrant.Down => Constants.CollisionSensor.Bottom,
            Angles.Quadrant.Left => Constants.CollisionSensor.Left,
            Angles.Quadrant.Right => Constants.CollisionSensor.Right,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        SetSolid(16, 16);
    }

    public override void _Process(double delta) => HurtPlayers();

    private void HurtPlayers()
    {
        foreach (Player player in Scene.Local.Players.Values)
        {
            player.ActSolid(this, Constants.SolidType.Full);
            
            if (!CheckSolidCollision(player, _sensor)) continue;
            
            player.Hurt(Position.X);

            if (!AudioPlayer.Sound.IsPlaying(SoundStorage.Hurt)) continue;
            
            AudioPlayer.Sound.Stop(SoundStorage.Hurt);
            AudioPlayer.Sound.Play(SoundStorage.SpikesHurt);
        }
    }
}