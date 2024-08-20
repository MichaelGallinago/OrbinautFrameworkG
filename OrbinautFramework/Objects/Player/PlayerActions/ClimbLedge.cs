using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

[FsmSourceGenerator.FsmState("Action")]
public struct ClimbLedge(PlayerData data, IPlayerLogic logic)
{
    public void Perform()
    {
        if (data.Sprite.Animation != Animations.ClimbLedge)
        {
            data.Sprite.Animation = Animations.ClimbLedge;
            data.Node.Position += new Vector2(3f * (float)data.Visual.Facing, -3f);
        }
        else if (data.Sprite.IsFrameChanged)
        {
            switch (data.Sprite.Frame)
            {
                case 1: data.Node.Position += new Vector2(8f * (float)data.Visual.Facing, -10f); break;
                case 2: data.Node.Position -= new Vector2(8f * (float)data.Visual.Facing, 12f); break;
            }
        }
        else if (data.Sprite.IsFinished)
        {
            Land();
            data.Sprite.Animation = Animations.Idle;
            data.Node.Position += new Vector2(8f * (float)data.Visual.Facing, 4f);

            // Subtract that 1px that was applied when we attached to the wall
            if (data.Visual.Facing == Constants.Direction.Negative)
            {
                data.Node.Position += Vector2.Left;
            }
        }
    }
}