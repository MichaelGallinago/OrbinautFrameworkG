using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Modules;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Physics.StateChangers;

public struct Rolling(PlayerData data)
{
    public void Start()
    {
        if (data.State is States.SpinDash or States.HammerDash) return;
        if (!data.Movement.IsGrounded || data.Movement.IsSpinning) return;
        if (!data.Movement.IsForcedSpin && (data.Input.Down.Left || data.Input.Down.Right)) return;

        if (!CheckSpinPossibility() && !data.Movement.IsForcedSpin) return;
		
        data.PlayerNode.Position += new Vector2(0f, data.Collision.Radius.Y - data.Collision.RadiusSpin.Y);
        data.Collision.Radius = data.Collision.RadiusSpin;
        data.Movement.IsSpinning = true;
        data.Visual.Animation = Animations.Spin;
		
        AudioPlayer.Sound.Play(SoundStorage.Roll);
    }

    private bool CheckSpinPossibility()
    {
        if (!data.Input.Down.Down) return false;
		
        if (SharedData.PhysicsType != PhysicsCore.Types.SK)
        {
            return Math.Abs(data.Movement.GroundSpeed) >= 0.5f;
        }

        if (Math.Abs(data.Movement.GroundSpeed) >= 1f) return true;

        data.Visual.Animation = Animations.Duck;
        return false;
    }
}
