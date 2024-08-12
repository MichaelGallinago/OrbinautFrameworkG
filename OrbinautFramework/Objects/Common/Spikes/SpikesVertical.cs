using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Common.Spikes;

public partial class SpikesVertical : Spikes
{
    protected override void CollideWithPlayer(PlayerData player)
    {
        player.ActSolid(this, player.Damage.IsInvincible ? Constants.SolidType.Full : Constants.SolidType.FullReset);
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
