using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Common.Spikes;

using Player;

public partial class SpikesHorizontal : Spikes
{
    protected override void CollideWithPlayer(Player player)
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