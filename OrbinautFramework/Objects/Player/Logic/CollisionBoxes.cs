using System;
using Godot;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Sprite;
using OrbinautFramework3.Objects.Spawnable.Shield;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Logic;

public readonly struct CollisionBoxes(PlayerData data, IPlayerLogic logic)
{
    public void Update()
    {
        data.Node.SolidBox.Set(data.Collision.RadiusNormal.X + 1, data.Collision.Radius.Y);
        SetRegularHitBox();
        SetExtraHitBox();
    }
    
    private void SetRegularHitBox()
    {
#if S3_PHYSICS || SK_PHYSICS
        data.Node.HitBox.Set(8, data.Collision.Radius.Y - 3);
#else
        if (data.Sprite.Animation != Animations.Duck)
        {
            data.Node.HitBox.Set(8, data.Collision.Radius.Y - 3);
            return;
        }

        if (data.Node.Type is PlayerNode.Types.Tails or PlayerNode.Types.Amy) return;
        data.Node.HitBox.Set(8, 10, 0, 6);
#endif
    }

    private void SetExtraHitBox()
    {
        if (logic.Action == States.HammerSpin)
        {
            data.Node.HitBox.SetExtra(25, 25);
        }
        else if (data.Sprite.Animation == Animations.HammerDash)
        {
            SetHammerDashExtraHitBox();
        }
        else if (data.Node.Shield.State == ShieldContainer.States.DoubleSpin)
        {
            data.Node.HitBox.SetExtra(24, 24);
        }
        else
        {
            data.Node.HitBox.SetExtra(Vector2I.Zero);
        }
    }

    private void SetHammerDashExtraHitBox()
    {
        (int radiusX, int radiusY, int offsetX, int offsetY) = (data.Sprite.Frame & 3) switch
        {
            0 => (16, 16,  6,  0),
            1 => (16, 16, -7,  0),
            2 => (14, 20, -4, -4),
            3 => (17, 21,  7, -5),
            _ => throw new ArgumentOutOfRangeException()
        };
        data.Node.HitBox.SetExtra(radiusX, radiusY, offsetX * (int)data.Visual.Facing, offsetY);
    }
}
