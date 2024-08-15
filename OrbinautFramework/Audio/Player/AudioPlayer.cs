using Godot;

namespace OrbinautFramework3.Audio.Player;

public partial class AudioPlayer : Node2D
{
    [ExportGroup("AudioStreamPlayers")]
    [Export] private AudioStreamPlayer _jinglePlayer;
    [Export] private AudioStreamPlayer[] _musicPlayers;
    [Export] private AudioStreamPlayer[] _soundPlayers;
    
    //TODO: singleton 
    public static MusicPlayerContainer Music { get; private set; }
    public static PlayerContainer Sound { get; private set; }
    
    public override void _Ready()
    {
        Music = new MusicPlayerContainer(_musicPlayers, 3, _jinglePlayer);
        Sound = new PlayerContainer(_soundPlayers, 16);

        _jinglePlayer = null;
        _musicPlayers = null;
        _soundPlayers = null;
    }
    
    public override void _Process(double delta)
    {
        Music.UpdateVolume();
        Sound.UpdateVolume();
    }
    
    public static void SetPauseState(bool isPaused)
    {
        Music.SetPauseState(isPaused);
        Sound.SetPauseState(isPaused);
    }
}
