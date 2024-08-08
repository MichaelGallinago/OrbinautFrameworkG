using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Common.Spikes;

using Player;

public partial class SpikesVertical : Spikes
{
    protected override void CollideWithPlayer(PlayerNode playerNode)
    {
        playerNode.ActSolid(this, playerNode.IsInvincible ? Constants.SolidType.Full : Constants.SolidType.FullReset);
    }

    protected override Vector2 GetRetractOffsetVector(float retractOffset) => new(0f, retractOffset);
    
    protected override SpikesDto GetDirectionSpecificData(Vector2 size)
    {
        bool isFlipped = Scale.Y < 0f;
        return new SpikesDto(
            isFlipped, 
            isFlipped ? Constants.CollisionSensor.Bottom : Constants.CollisionSensor.Top, 
            (int)(size.Y * Scale.Y)
        );
    }
}
