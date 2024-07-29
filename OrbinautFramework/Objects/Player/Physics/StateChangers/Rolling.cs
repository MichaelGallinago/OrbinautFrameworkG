using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player.Physics.StateChangers;

public struct Rolling
{
    public void Start()
    {
        if (!IsGrounded || IsSpinning || Action is Actions.SpinDash or Actions.HammerDash) return;
        if (!IsForcedSpin && (Input.Down.Left || Input.Down.Right)) return;

        if (!CheckSpinPossibility() && !IsForcedSpin) return;
		
        Position += new Vector2(0f, Radius.Y - RadiusSpin.Y);
        Radius = RadiusSpin;
        IsSpinning = true;
        Animation = Animations.Spin;
		
        AudioPlayer.Sound.Play(SoundStorage.Roll);
    }

    private bool CheckSpinPossibility()
    {
        if (!Input.Down.Down) return false;
		
        if (SharedData.PlayerPhysics != PhysicsTypes.SK)
        {
            return Math.Abs(GroundSpeed) >= 0.5f;
        }

        if (Math.Abs(GroundSpeed) >= 1f) return true;

        Animation = Animations.Duck;
        return false;
    }
}
