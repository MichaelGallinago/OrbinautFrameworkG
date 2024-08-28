using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Sprite;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Physics.StateChangers;

public readonly struct Rolling(PlayerData data, IPlayerLogic logic)
{
    public void Start()
    {
        if (logic.Action is States.SpinDash or States.HammerDash) return;
        if (!data.Movement.IsForcedSpin && (data.Input.Down.Left || data.Input.Down.Right)) return;

        if (!CheckSpinPossibility() && !data.Movement.IsForcedSpin) return;
		
        data.Node.Position += new Vector2(0f, data.Collision.Radius.Y - data.Collision.RadiusSpin.Y);
        data.Collision.Radius = data.Collision.RadiusSpin;
        data.Movement.IsSpinning = true;
        data.Sprite.Animation = Animations.Spin;
		
        AudioPlayer.Sound.Play(SoundStorage.Roll);
    }

    private bool CheckSpinPossibility()
    {
        if (!data.Input.Down.Down) return false;

#if SK_PHYSICS
        if (Math.Abs(data.Movement.GroundSpeed) >= 1f) return true;

        data.Sprite.Animation = Animations.Duck;
        return false;
#else
        return Math.Abs(data.Movement.GroundSpeed) >= 0.5f;
#endif
    }
}
