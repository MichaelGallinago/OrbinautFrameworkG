using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Modules;

public struct Carry(PlayerData data)
{
    public void Process()
    {
        if (data.Node.Type != PlayerNode.Types.Tails) return;

        if (data.Carry.Timer > 0f)
        {
            data.Carry.Timer -= Scene.Instance.ProcessSpeed;
            if (data.Carry.Timer > 0f) return;
        }
	
        if (data.Carry.Target != null)
        {
            data.Carry.Target.CarryTarget.OnAttached(data);
            return;
        }
		
        if (data.State != States.Flight) return;

        GrabAnotherPlayer();
    }

    private void GrabAnotherPlayer()
    {
        foreach (PlayerData player in Scene.Instance.Players.Values)
        {
            if (player == data) continue;
            if (player.State is States.SpinDash or States.Carried) continue;
            if (!player.Movement.IsControlRoutineEnabled || !player.Collision.IsObjectInteractionEnabled) continue;

            Vector2 delta = (player.Node.Position - data.Node.Position).Abs();
            if (delta.X >= 16f || delta.Y >= 48f) continue;
				
            player.ResetState();
            AudioPlayer.Sound.Play(SoundStorage.Grab);
				
            player.Visual.Animation = Animations.Grab;
            player.State = States.Carried;
            data.Carry.Target = player;

            player.AttachToCarrier(this);
        }
    }
}
