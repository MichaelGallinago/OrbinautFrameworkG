using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Objects.Player.Modules;

public struct CollisionBoxes(PlayerData data)
{
    public void Update()
    {
        SolidBox.Set(RadiusNormal.X + 1, Radius.Y);
        SetRegularHitBox();
        SetExtraHitBox();
    }
    
    private void SetRegularHitBox()
    {
        if (Animation != Animations.Duck || SharedData.PhysicsType >= PhysicsTypes.S3)
        {
            HitBox.Set(8, Radius.Y - 3);
            return;
        }

        if (Type is Types.Tails or Types.Amy) return;
        HitBox.Set(8, 10, 0, 6);
    }

    private void SetExtraHitBox()
    {
        switch (Animation)
        {
            case Animations.HammerSpin:
                HitBox.SetExtra(25, 25);
                break;
			
            case Animations.HammerDash:
                (int radiusX, int radiusY, int offsetX, int offsetY) = (_sprite.Frame & 3) switch
                {
                    0 => (16, 16,  6,  0),
                    1 => (16, 16, -7,  0),
                    2 => (14, 20, -4, -4),
                    3 => (17, 21,  7, -5),
                    _ => throw new ArgumentOutOfRangeException()
                };
                HitBox.SetExtra(radiusX, radiusY, offsetX * (int)Facing, offsetY);
                break;
            default:
                HitBox.SetExtra(_shield.State == ShieldContainer.States.DoubleSpin ? 
                    new Vector2I(24, 24) : Vector2I.Zero);
                break;
        }
    }
}
