using Godot;
using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;
using OrbinautFrameworkG.Framework.SceneModule;
using OrbinautFrameworkG.Framework.Tiles;
using OrbinautFrameworkG.Objects.Player.Data;
using static OrbinautFrameworkG.Framework.StaticStorages.Constants;

namespace OrbinautFrameworkG.Objects.Common.Spikes;

public partial class Spikes : SolidNode
{
    [Export] protected Sprite2D Sprite { get; private set; }

    private CollisionSensor _sensorToDamage;
    private bool _isHorizontal;
    
    public override void _Ready()
    {
        _sensorToDamage = Angles.GetQuadrant(RotationDegrees) switch
        {
            Angles.Quadrant.Right => Scale.X < 0 ? CollisionSensor.Left   : CollisionSensor.Right,
            Angles.Quadrant.Up    => Scale.Y < 0 ? CollisionSensor.Bottom : CollisionSensor.Top,
            Angles.Quadrant.Left  => Scale.X < 0 ? CollisionSensor.Right  : CollisionSensor.Left,
            _                     => Scale.Y < 0 ? CollisionSensor.Top    : CollisionSensor.Bottom
        };
        
        _isHorizontal = _sensorToDamage is CollisionSensor.Left or CollisionSensor.Right;
    }

    public override void _Process(double delta) => CollideWithPlayers();
    
    private void CollideWithPlayers()
    {
        foreach (IPlayer player in Scene.Instance.Players.Values)
        {
            AttachType attachType = _isHorizontal || player.Data.Damage.IsInvincible ? 
                AttachType.Default : AttachType.ResetPlayer; //TODO: optimize this????
            
            player.ActSolid(this, SolidType.Full, attachType);
            if (!player.CheckSolidCollision(this, _sensorToDamage)) continue;
            player.Hurt(Position.X, SoundStorage.SpikesHurt);
        }
    }
}
