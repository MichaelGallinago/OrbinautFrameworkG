using System.Collections.Generic;
using Godot;

namespace OrbinautFramework3.Audio.Player;

public class MusicPlayerContainer : PlayerContainer
{
    private readonly AudioStreamPlayer _jinglePlayer;
    
    public MusicPlayerContainer(ICollection<AudioStreamPlayer> players, 
        byte playersLimit, AudioStreamPlayer jinglePlayer) : base(players, playersLimit)
    {
        _jinglePlayer = jinglePlayer;
        _jinglePlayer.Finished += StopJingle;
    }
    
    public bool IsJinglePlaying() => _jinglePlayer.Stream != null;
    
    public void PlayJingle(AudioStream audio)
    {
        _jinglePlayer.Stream = audio;
        _jinglePlayer.Play();
        MuteBus(0f);
    }

    public void StopJingle()
    {
        _jinglePlayer.Stop();
        _jinglePlayer.Stream = null;
        UnmuteBus(1f);
    }
}
