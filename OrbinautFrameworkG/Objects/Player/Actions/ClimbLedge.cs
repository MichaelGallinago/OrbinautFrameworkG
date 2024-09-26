using Godot;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.StaticStorages;
using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Logic;
using OrbinautFrameworkG.Objects.Player.Sprite;
using static OrbinautFrameworkG.Objects.Player.ActionFsm;

namespace OrbinautFrameworkG.Objects.Player.Actions;

[FsmSourceGenerator.FsmState("Action")]
public readonly struct ClimbLedge(PlayerData data, IPlayerLogic logic)
{
    public States Perform()
    {
        IPlayerSprite sprite = data.Sprite;
        if (sprite.Animation != Animations.ClimbLedge)
        {
            sprite.Animation = Animations.ClimbLedge;
            data.Movement.Position += new Vector2(3f * (float)data.Visual.Facing, -3f);
        }
        else if (sprite.IsFrameChanged)
        {
            OffsetPlayerByFrame();
        }
        else if (sprite.IsFinished)
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
            case 1: data.Movement.Position += new Vector2(8f * (float)data.Visual.Facing, -10f); break;
            case 2: data.Movement.Position -= new Vector2(8f * (float)data.Visual.Facing, 12f); break;
        }
    }

    private void StandUp()
    {
        logic.Land();
        data.Sprite.Animation = Animations.Idle;
        data.Movement.Position += new Vector2(8f * (float)data.Visual.Facing, 4f);

        // Subtract that 1px that was applied when we attached to the wall
        if (data.Visual.Facing == Constants.Direction.Negative)
        {
            data.Movement.Position.X--;
        }
    }
}
