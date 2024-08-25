using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Common.Spikes;

public partial class SpikesHorizontal : Spikes
{
    protected override void CollideWithPlayer(IPlayer player)
    {
        player.ActSolid(this, Constants.SolidType.Full);
    }

    protected override Vector2 GetRetractOffsetVector(float retractOffset) => new(retractOffset, 0f);
    
    protected override SpikesDto GetDirectionSpecificData(Vector2 size)
    {
        bool isFlipped = Scale.X < 0f;
        return new SpikesDto(
            isFlipped, 
            isFlipped ? Constants.CollisionSensor.Right : Constants.CollisionSensor.Left,
            (int)(size.X * Scale.X)
        );
    }
}
