
using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Audio.Player;

public partial class AudioPlayer : Node2D
{
    [ExportGroup("AudioStreamPlayers")]
    [Export] private AudioStreamPlayer _jinglePlayer;
    [Export] private Godot.Collections.Array<AudioStreamPlayer> _musicPlayers;
    [Export] private Godot.Collections.Array<AudioStreamPlayer> _soundPlayers;
    
    public static PlayerContainer Music { get; private set; }
    public static PlayerContainer Sound { get; private set; }

    public override void _Ready()
    {
        Music = new PlayerContainer(ref _musicPlayers, 4);
        Sound = new PlayerContainer(ref _soundPlayers, 16);
    }
    
    public override void _Process(double delta)
    {
        Music.UpdateVolume();
        Sound.UpdateVolume();
    }

    public static void PlayJingle()
    {
        _jinglePlayer
        musicPlayer.Stream = music;
        musicPlayer.VolumeDb = 0f;
        musicPlayer.Play();
        _playingMusic.Add(music);
    }
    
    public static void SetPauseState(bool isPaused)
    {
        Music.SetPauseState(isPaused);
        Sound.SetPauseState(isPaused);
    }
}
