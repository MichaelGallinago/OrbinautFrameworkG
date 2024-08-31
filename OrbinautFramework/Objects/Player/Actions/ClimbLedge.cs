using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Sprite;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Actions;

[FsmSourceGenerator.FsmState("Action")]
public readonly struct ClimbLedge(PlayerData data, IPlayerLogic logic)
{
    public States Perform()
    {
        if (data.Sprite.Animation != Animations.ClimbLedge)
        {
            data.Sprite.Animation = Animations.ClimbLedge;
            data.Node.Position += new Vector2(3f * (float)data.Visual.Facing, -3f);
        }
        else if (data.Sprite.IsFrameChanged)
        {
            OffsetPlayerByFrame();
        }
        else if (data.Sprite.IsFinished)
        {
            StandUp();
            return States.Default;
        }

        return States.ClimbLedge;
    }

    private void OffsetPlayerByFrame()
    {
        switch (data.Sprite.Frame)
        {
            case 1: data.Node.Position += new Vector2(8f * (float)data.Visual.Facing, -10f); break;
            case 2: data.Node.Position -= new Vector2(8f * (float)data.Visual.Facing, 12f); break;
        }
    }

    private void StandUp()
    {
        logic.Land();
        data.Sprite.Animation = Animations.Idle;
        data.Node.Position += new Vector2(8f * (float)data.Visual.Facing, 4f);

        // Subtract that 1px that was applied when we attached to the wall
        if (data.Visual.Facing == Constants.Direction.Negative)
        {
            data.Node.Position += Vector2.Left;
        }
    }
}
