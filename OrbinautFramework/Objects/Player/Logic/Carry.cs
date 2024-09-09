using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Sprite;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Logic;

public readonly struct Carry(PlayerData data, CarryData carryData, IPlayerLogic logic)
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
		
        if (carryData.Target.IsFree)
        {
            carryData.Free();
        }
        else
        {
            carryData.Target.CarryTargetLogic.OnAttached(carrier);
        }
        else if ((Vector2I)carryData.Target.Position != (Vector2I)carryData.TargetPosition)
        {
            carryData.Free();
        }
        else
        {
            AttachToCarrier();
            carryData.Target.scr_player_collision_air();
        }
    }

    private void GrabAnotherPlayer()
    {
        foreach (IPlayer player in Scene.Instance.Players.Values)
        {
            if (player == logic) continue;
            if (player.Action is States.SpinDash or States.Carried) continue;
            if (!player.Data.State.IsObjectInteractable()) continue; 
            
            Vector2I delta = ((Vector2I)player.Data.Node.Position - (Vector2I)data.Node.Position).Abs();
            if (delta.X >= 16 || delta.Y >= 48) continue;
            
            carryData.Target = player;
            
            player.ResetData();
            player.Action = States.Carried;
            player.Data.Sprite.Animation = Animations.Grab;
            
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
        target.Position = carryData.TargetPosition = data.Node.Position + new Vector2(0f, 28f);
    }
}
