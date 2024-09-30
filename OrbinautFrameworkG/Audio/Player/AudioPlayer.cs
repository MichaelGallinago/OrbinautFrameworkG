using Godot;
using OrbinautFrameworkG.Framework.MathUtilities;

namespace OrbinautFrameworkG.Audio.Player;

public partial class AudioPlayer : Node2D
{
    [ExportGroup("AudioStreamPlayers")]
    [Export] private AudioStreamPlayer   _jinglePlayer;
    [Export] private AudioStreamPlayer[] _musicPlayers;
    [Export] private AudioStreamPlayer[] _soundPlayers;
    
    public static MusicPlayerContainer Music { get; private set; }
    public static PlayerContainer Sound { get; private set; }
    
    private static AudioPlayer _instance;

    public override void _EnterTree()
    {
        if (_instance != null)
        {
            QueueFree();
            return;
        }
        
        _instance = this;
        CreateContainers();
    }

    public override void _ExitTree()
    {
        if (_instance != this) return;
        _instance = null;
    }

    public override void _Process(double deltaTime)
    {
        float speed = DeltaTimeUtilities.CalculateSpeed(deltaTime);
        Music.UpdateVolume(speed);
        Sound.UpdateVolume(speed);
    }
    
    public static void SetPauseState(bool isPaused)
    {
        Music.SetPauseState(isPaused);
        Sound.SetPauseState(isPaused);
    }

    public static void StopAll()
    {
        Music.StopAll();
        Sound.StopAll();
    }
    
    private void CreateContainers()
    {
        const float defaultMusicVolume = 0.5f;
        const float defaultSoundVolume = 0.5f;
        
        Music = new MusicPlayerContainer(_musicPlayers, 3, _jinglePlayer) { MaxVolume = defaultMusicVolume };
        Sound = new PlayerContainer(_soundPlayers, 16) { MaxVolume = defaultSoundVolume };
        
        _jinglePlayer = null;
        _musicPlayers = null;
        _soundPlayers = null;
    }
}
