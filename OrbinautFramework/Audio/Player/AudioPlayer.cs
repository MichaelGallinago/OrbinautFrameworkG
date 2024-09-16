using Godot;

namespace OrbinautFramework3.Audio.Player;

public partial class AudioPlayer : Node2D
{
    public const float DefaultMusicVolume = 0.5f;
    public const float DefaultSoundVolume = 0.5f;
    
    [ExportGroup("AudioStreamPlayers")]
    [Export] private AudioStreamPlayer   _jinglePlayer;
    [Export] private AudioStreamPlayer[] _musicPlayers;
    [Export] private AudioStreamPlayer[] _soundPlayers;
    
    public static MusicPlayerContainer Music { get; private set; }
    public static PlayerContainer Sound { get; private set; }
    
    private static AudioPlayer _instance;
    
    public override void _EnterTree()
    {
        if (_instance == null)
        {
            _instance = this;
            CreateContainers();
            return;
        }
        
        QueueFree();
    }

    public override void _ExitTree()
    {
        if (_instance != this) return;
        _instance = null;
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
    
    private void CreateContainers()
    {
        Music = new MusicPlayerContainer(_musicPlayers, 3, _jinglePlayer) { Volume = DefaultMusicVolume };
        Sound = new PlayerContainer(_soundPlayers, 16) { Volume = DefaultSoundVolume };
        
        _jinglePlayer = null;
        _musicPlayers = null;
        _soundPlayers = null;
    }
}
