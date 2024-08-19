using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Sprite;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Logic;

public struct Carry(PlayerData data, IPlayerLogic logic)
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
            data.Carry.Target.CarryTargetLogic.OnAttached(data);
            return;
        }
		
        if (logic.Action != States.Flight) return;

        GrabAnotherPlayer();
    }

    private void GrabAnotherPlayer()
    {
        foreach (IPlayer player in Scene.Instance.Players.Values)
        {
            if (player.Data == data) continue;
            if (player.Action is States.SpinDash or States.Carried) continue;
            if (!player.Data.Movement.IsControlRoutineEnabled) continue; 
            if (!player.Data.Collision.IsObjectInteractionEnabled) continue;

            Vector2 delta = (player.Data.Node.Position - data.Node.Position).Abs();
            if (delta.X >= 16f || delta.Y >= 48f) continue;
				
            player.ResetState();
            AudioPlayer.Sound.Play(SoundStorage.Grab);
				
            player.Data.Visual.Animation = Animations.Grab;
            player.Action = States.Carried;
            data.Carry.Target = player;

            player.CarryTargetLogic.AttachToCarrier(this);
        }
    }
}
