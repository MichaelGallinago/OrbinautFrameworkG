using System;
using Godot;
using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Logic;
using OrbinautFrameworkG.Objects.Player.Sprite;
using static OrbinautFrameworkG.Objects.Player.ActionFsm;

namespace OrbinautFrameworkG.Objects.Player.Physics.StateChangers;

public readonly struct Rolling(PlayerData data, IPlayerLogic logic)
{
    public void Start()
    {
        if (logic.Action is States.SpinDash or States.HammerDash) return;

        MovementData movement = data.Movement;
        if (!movement.IsForcedRoll && (data.Input.Down.Left || data.Input.Down.Right)) return;

        if (!CheckSpinPossibility() && !movement.IsForcedRoll) return;

        CollisionData collision = data.Collision;
        movement.Position.Y += collision.Radius.Y - collision.RadiusSpin.Y;
        movement.IsSpinning = true;
        
        collision.Radius = collision.RadiusSpin;
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
