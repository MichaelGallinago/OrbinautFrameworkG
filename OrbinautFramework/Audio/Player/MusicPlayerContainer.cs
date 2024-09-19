using System.Collections.Generic;
using Godot;

namespace OrbinautFramework3.Audio.Player;

public class MusicPlayerContainer : PlayerContainer
{
    private readonly AudioStreamPlayer _jinglePlayer;
    private readonly int _jingleBusIndex;
    
    public override float MaxVolume 
    { 
        get => base.MaxVolume;
        set
        {
            base.MaxVolume = value;
            AudioServer.SetBusVolumeDb(_jingleBusIndex, VolumeDb);
        }
    }
    
    public MusicPlayerContainer(IEnumerable<AudioStreamPlayer> players, 
        byte playersLimit, AudioStreamPlayer jinglePlayer) : base(players, playersLimit)
    {
        _jinglePlayer = jinglePlayer;
        _jinglePlayer.Finished += StopJingle;
        _jingleBusIndex = AudioServer.GetBusIndex(jinglePlayer.Bus);
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
