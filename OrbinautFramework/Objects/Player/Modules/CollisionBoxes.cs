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
        data.PlayerNode.SolidBox.Set(data.Collision.RadiusNormal.X + 1, data.Collision.Radius.Y);
        SetRegularHitBox();
        SetExtraHitBox();
    }
    
    private void SetRegularHitBox()
    {
        if (data.Visual.Animation != Animations.Duck || SharedData.PhysicsType >= PhysicsCore.Types.S3)
        {
            data.PlayerNode.HitBox.Set(8, data.Collision.Radius.Y - 3);
            return;
        }

        if (data.PlayerNode.Type is PlayerNode.Types.Tails or PlayerNode.Types.Amy) return;
        data.PlayerNode.HitBox.Set(8, 10, 0, 6);
    }

    private void SetExtraHitBox()
    {
        switch (data.Visual.Animation)
        {
            case Animations.HammerSpin:
                data.PlayerNode.HitBox.SetExtra(25, 25);
                break;
			
            case Animations.HammerDash:
                (int radiusX, int radiusY, int offsetX, int offsetY) = (data.PlayerNode.Sprite.Frame & 3) switch
                {
                    0 => (16, 16,  6,  0),
                    1 => (16, 16, -7,  0),
                    2 => (14, 20, -4, -4),
                    3 => (17, 21,  7, -5),
                    _ => throw new ArgumentOutOfRangeException()
                };
                data.PlayerNode.HitBox.SetExtra(radiusX, radiusY, offsetX * (int)data.Visual.Facing, offsetY);
                break;
            default:
                data.PlayerNode.HitBox.SetExtra(data.PlayerNode.Shield.State == ShieldContainer.States.DoubleSpin ? 
                    new Vector2I(24, 24) : Vector2I.Zero);
                break;
        }
    }
}
