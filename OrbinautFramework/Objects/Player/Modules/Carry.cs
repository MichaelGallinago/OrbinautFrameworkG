using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Physics;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Modules;

public struct Carry(PlayerData data)
{
    public void Process()
    {
        if (data.PlayerNode.Type != PlayerNode.Types.Tails) return;

        if (data.Carry.Timer > 0f)
        {
            data.Carry.Timer -= Scene.Instance.ProcessSpeed;
            if (data.Carry.Timer > 0f) return;
        }
	
        if (data.Carry.Target != null)
        {
            data.Carry.Target.OnAttached(this);
            return;
        }
		
        if (data.State != States.Flight) return;

        GrabAnotherPlayer();
    }

    private void GrabAnotherPlayer(PlayerNode carrier)
    {
        foreach (PlayerData player in Scene.Instance.Players.Values)
        {
            if (player == carrier) continue;
            if (player.data.State is Actions.SpinDash or Actions.Carried) continue;
            if (!player.IsControlRoutineEnabled || !player.IsObjectInteractionEnabled) continue;

            Vector2 delta = (player.Position - Position).Abs();
            if (delta.X >= 16f || delta.Y >= 48f) continue;
				
            player.ResetState();
            AudioPlayer.Sound.Play(SoundStorage.Grab);
				
            player.Animation = Animations.Grab;
            player.data.State = Actions.Carried;
            data.Carry.Target = player;

            player.AttachToCarrier(this);
        }
    }
}
