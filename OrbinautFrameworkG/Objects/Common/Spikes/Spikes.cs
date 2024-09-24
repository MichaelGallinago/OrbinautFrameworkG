using Godot;
using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;
using OrbinautFrameworkG.Framework.Tiles;
using OrbinautFrameworkG.Objects.Player.Data;
using static OrbinautFrameworkG.Framework.Constants;

namespace OrbinautFrameworkG.Objects.Common.Spikes;

public partial class Spikes : SolidNode
{
    [Export] protected Sprite2D Sprite { get; private set; }

    private Constants.CollisionSensor _sensorToDamage;
    private bool _isHorizontal;
    
    public override void _Ready()
    {
        _sensorToDamage = Angles.GetQuadrant(RotationDegrees) switch
        {
            Angles.Quadrant.Right => Scale.X < 0 ? Constants.CollisionSensor.Left   : Constants.CollisionSensor.Right,
            Angles.Quadrant.Up    => Scale.Y < 0 ? Constants.CollisionSensor.Bottom : Constants.CollisionSensor.Top,
            Angles.Quadrant.Left  => Scale.X < 0 ? Constants.CollisionSensor.Right  : Constants.CollisionSensor.Left,
            _                     => Scale.Y < 0 ? Constants.CollisionSensor.Top    : Constants.CollisionSensor.Bottom
        };
        
        _isHorizontal = _sensorToDamage is Constants.CollisionSensor.Left or Constants.CollisionSensor.Right;
    }

    public override void _Process(double delta) => CollideWithPlayers();
    
    private void CollideWithPlayers()
    {
        foreach (IPlayer player in Scene.Instance.Players.Values)
        {
            Constants.AttachType attachType = _isHorizontal || player.Data.Damage.IsInvincible ? 
                Constants.AttachType.Default : Constants.AttachType.ResetPlayer; //TODO: optimize this????
            
            player.ActSolid(this, Constants.SolidType.Full, attachType);
            if (!player.CheckSolidCollision(SolidBox, _sensorToDamage)) continue;
            player.Hurt(Position.X, SoundStorage.SpikesHurt);
        }
    }
}
