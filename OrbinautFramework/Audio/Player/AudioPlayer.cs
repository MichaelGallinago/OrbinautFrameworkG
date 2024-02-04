using System.Collections.Generic;
using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Audio.Player;

public partial class AudioPlayer : Node2D
{
    public enum MusicStates : byte
    {
        Default, Mute, Stop
    }
    
    private const float MinimalVolume = -80f;
    private const float DefaultVolume = 0f;
    
    [ExportGroup("AudioStreamPlayers")]
    [Export] private AudioStreamPlayer _musicPlayer;
    [Export] private Godot.Collections.Array<AudioStreamPlayer> _soundPlayers;
    
    public static MusicStates MusicState { get; private set; }
    
    private static float _muteSpeed;
    private static AudioPlayer _player;
    private static Stack<AudioStreamPlayer> _freePlayers;
    private static Dictionary<AudioStream, AudioStreamPlayer> _activePlayers;

    public static bool CheckMusicPlaying(AudioStream music) => _player._musicPlayer.Stream == music;
    
    public override void _Ready()
    {
        _player = this;
        _muteSpeed = 0f;
        MusicState = MusicStates.Default;
        _freePlayers = new Stack<AudioStreamPlayer>(_soundPlayers);
        _activePlayers = new Dictionary<AudioStream, AudioStreamPlayer>(_soundPlayers.Count);
        
        foreach (AudioStreamPlayer soundPlayer in _soundPlayers)
        {
            soundPlayer.Finished += () => FreeSoundPlayer(soundPlayer);
        }
    }
    
    public override void _Process(double delta)
    {
        if (_muteSpeed == 0f) return;
        
        _musicPlayer.VolumeDb += FrameworkData.ProcessSpeed * _muteSpeed;
        switch (_musicPlayer.VolumeDb)
        {
            case <= MinimalVolume:
                _muteSpeed = 0f;
                _musicPlayer.VolumeDb = MinimalVolume;
                break;
            
            case >= 0f:
                _muteSpeed = 0f;
                _musicPlayer.VolumeDb = 0f;
                break;
        }
    }
    
    public static void PlaySound(AudioStream sound) => GetSoundPlayer(sound).Play();
    
    public static void PlaySoundPitch(AudioStream sound, float pitch)
    {
        AudioStreamPlayer soundPlayer = GetSoundPlayer(sound);
        soundPlayer.PitchScale = pitch;
        soundPlayer.Play();
    }
    
    public static void StopSound(AudioStream sound)
    {
        if (_activePlayers.TryGetValue(sound, out AudioStreamPlayer existingSoundPlayer))
        {
            FreeSoundPlayer(existingSoundPlayer);
        }
    }
    
    public static void SetPauseState(bool isPaused)
    {
        foreach (AudioStreamPlayer soundPlayers in _player._soundPlayers)
        {
            soundPlayers.StreamPaused = isPaused;
        }
        _player._musicPlayer.StreamPaused = isPaused;
    }

    public static bool IsSoundPlaying(AudioStream sound) => _activePlayers.ContainsKey(sound);
    
    public static void PlayMusic(AudioStream music)
    {
        AudioStreamPlayer musicPlayer = _player._musicPlayer;
        if (musicPlayer.Stream == music) return;
        musicPlayer.Stream = music;
        musicPlayer.VolumeDb = 0f;
        musicPlayer.Play();
    }
    
    public static void StopMusic(float time)
    {
        MusicState = MusicStates.Stop;
        SetMuteSpeed(time, MinimalVolume);
    }
    
    public static void MuteMusic(float time)
    {
        MusicState = MusicStates.Mute;
        SetMuteSpeed(time, MinimalVolume);
    }
    
    public static void UnmuteMusic(float time)
    {
        MusicState = MusicStates.Default;
        SetMuteSpeed(time, 0f);
    }
    
    private static void SetMuteSpeed(float time, float value)
    {
        _muteSpeed = (value - _player._musicPlayer.VolumeDb) / (time * Constants.BaseFramerate);
    }

    private static AudioStreamPlayer GetSoundPlayer(AudioStream sound)
    {
        if (_activePlayers.TryGetValue(sound, out AudioStreamPlayer soundPlayer)) return soundPlayer;
        
        if (!_freePlayers.TryPop(out soundPlayer))
        {
            soundPlayer = CreateSoundPlayer();
        }
        
        _activePlayers.Add(sound, soundPlayer);
        soundPlayer.Stream = sound;

        return soundPlayer;
    }
    
    private static AudioStreamPlayer CreateSoundPlayer()
    {
        var soundPlayer = new AudioStreamPlayer();
        soundPlayer.Finished += () => FreeSoundPlayer(soundPlayer);
        _player._soundPlayers.Add(soundPlayer);
        return soundPlayer;
    }
    
    private static void FreeSoundPlayer(AudioStreamPlayer soundPlayer)
    {
        _activePlayers.Remove(soundPlayer.Stream);
        soundPlayer.Stream = null;
        soundPlayer.PitchScale = 1f;
        _freePlayers.Push(soundPlayer);
    }
}
