using System;
using Godot;
using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Objects.Player.Data;
using static OrbinautFrameworkG.Objects.Player.ActionFsm;

namespace OrbinautFrameworkG.Objects.Player.Logic;

public readonly struct Carry(PlayerData data, CarryData carryData, IPlayerActionStorage logic)
{
    public void Process()
    {
        if (carryData.Cooldown > 0f)
        {
            carryData.Cooldown -= Scene.Instance.Speed;
            return;
        }
        
        if (carryData.Target == null)
        {
            if (logic.Action != States.Flight) return;

            GrabAnotherPlayer();
            return;
        }
		
        if (carryData.Target.TryFree(out float cooldown))
        {
            carryData.Free(cooldown);
        }
        else if ((Vector2I)carryData.Target.Position != (Vector2I)carryData.TargetPosition)
        {
            carryData.Free();
        }
        else
        {
            AttachToCarrier();
            carryData.Target.Collide();
        }
    }

    private void GrabAnotherPlayer()
    {
        foreach (IPlayer player in Scene.Instance.Players.Values)
        {
            if (player == logic) continue;
            if (player.Action is States.SpinDash or States.Carried) continue;
            if (!player.Data.State.IsObjectInteractable()) continue; 
            
            Vector2I delta = ((Vector2I)player.Data.Movement.Position - (Vector2I)data.Movement.Position).Abs();
            if (delta.X >= 16 || delta.Y >= 48) continue;
            
            carryData.Target = player;
            player.Action = States.Carried;
            
            AttachToCarrier();
            
            AudioPlayer.Sound.Play(SoundStorage.Grab);
            return;
        }
    }

    private void AttachToCarrier()
    {
        ICarryTarget target = carryData.Target;
        target.Facing = data.Visual.Facing;
        target.Velocity = data.Movement.Velocity;
        target.Scale = new Vector2(Math.Abs(target.Scale.X) * (float)data.Visual.Facing, target.Scale.Y);
        target.Position = carryData.TargetPosition = data.Movement.Position + new Vector2(0f, 28f);
    }
}
