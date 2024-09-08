using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Sprite;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Logic;

public readonly struct Carry(PlayerData data, IPlayerLogic logic, ICarrier carrier)
{
    public void Process()
    {
        CarryData carry = data.Carry;
        if (carry.Cooldown > 0f)
        {
            carry.Cooldown -= Scene.Instance.Speed;
            return;
        }
        
        if (carry.Target != null)
        {
            carry.Target.CarryTargetLogic.OnAttached(carrier);
            return;
        }
		
        if (logic.Action != States.Flight) return;

        GrabAnotherPlayer();
    }

    private void GrabAnotherPlayer()
    {
        foreach (IPlayer player in Scene.Instance.Players.Values)
        {
            if (player == logic) continue;
            if (player.Action is States.SpinDash or States.Carried) continue;
            if (player.Data.State.IsObjectInteractable()) continue; 
            
            Vector2I delta = ((Vector2I)player.Data.Node.Position - (Vector2I)data.Node.Position).Abs();
            if (delta.X >= 16 || delta.Y >= 48) continue;
            
            data.Carry.Target = player;
            
            player.ResetData();
            player.Action = States.Carried;
            player.Data.Sprite.Animation = Animations.Grab;
            
            player.CarryTargetLogic.AttachToCarrier(carrier);
            
            AudioPlayer.Sound.Play(SoundStorage.Grab);
        }
    }
}
