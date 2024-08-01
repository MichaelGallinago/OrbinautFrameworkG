using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player;

public struct Carry
{
    public ICarried Target { get; set; }
    public float Timer { get; set; }
    public Vector2 TargetPosition { get; set; }
    
    public void Process()
    {
        if (Type != Types.Tails) return;

        if (Timer > 0f)
        {
            Timer -= Scene.Local.ProcessSpeed;
            if (Timer > 0f) return;
        }
	
        if (Target != null)
        {
            Target.OnAttached(this);
            return;
        }
		
        if (Action != Actions.Flight) return;

        GrabAnotherPlayer();
    }

    private void GrabAnotherPlayer(Player carrier)
    {
        foreach (Player player in Scene.Local.Players.Values)
        {
            if (player == carrier) continue;
            if (player.Action is Actions.SpinDash or Actions.Carried) continue;
            if (!player.IsControlRoutineEnabled || !player.IsObjectInteractionEnabled) continue;

            Vector2 delta = (player.Position - Position).Abs();
            if (delta.X >= 16f || delta.Y >= 48f) continue;
				
            player.ResetState();
            AudioPlayer.Sound.Play(SoundStorage.Grab);
				
            player.Animation = Animations.Grab;
            player.Action = Actions.Carried;
            Target = player;

            player.AttachToPlayer(this);
        }
    }
}
