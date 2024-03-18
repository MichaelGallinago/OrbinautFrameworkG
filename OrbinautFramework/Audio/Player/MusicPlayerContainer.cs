using Godot;
using Godot.Collections;

namespace OrbinautFramework3.Audio.Player;

public class MusicPlayerContainer : PlayerContainer
{
    private readonly AudioStreamPlayer _jinglePlayer;
    
    public MusicPlayerContainer(ref Array<AudioStreamPlayer> players, 
        byte playersLimit, AudioStreamPlayer jinglePlayer) : base(ref players, playersLimit)
    {
        _jinglePlayer = jinglePlayer;
        _jinglePlayer.Finished += StopJingle;
    }

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