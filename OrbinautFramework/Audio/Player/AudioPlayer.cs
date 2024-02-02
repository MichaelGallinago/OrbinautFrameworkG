using System.Collections.Generic;
using Godot;
using Godot.Collections;
using OrbinautFramework3.Framework;

[assembly: AudioStorageSourceGenerator.AudioStorage("SoundStorage", "OrbinautFramework3.Audio.Player", "res://Audio/SFX/")]

namespace OrbinautFramework3.Audio.Player;

public partial class AudioPlayer : Node2D
{
    public enum States : byte
    {
        Default, Mute, Stop
    }

    private const float MinimalVolume = -80f;
    private const float DefaultVolume = 0f;
    
    [ExportGroup("AudioStreamPlayers")]
    [Export] private AudioStreamPlayer _musicPlayer;
    [Export] private Array<AudioStreamPlayer> _soundPlayers;
    
    private static float _muteSpeed;
    private static AudioPlayer _player;
    private static Stack<AudioStreamPlayer> _freePlayers;

    public override void _Ready()
    {
        _player = this;
        _muteSpeed = 0f;
        _freePlayers = new Stack<AudioStreamPlayer>(_soundPlayers);
        
        foreach (AudioStreamPlayer soundPlayer in _soundPlayers)
        {
            soundPlayer.Finished += () => _freePlayers.Push(soundPlayer);
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

    public static void PlaySound(AudioStream audioStream)
    {
        if (!_freePlayers.TryPop(out AudioStreamPlayer soundPlayer))
        {
            soundPlayer = CreateSoundPlayer();
        }
        
        soundPlayer.Stream = audioStream;
        soundPlayer.Play();
    }
    
    public static void PlayMusic(AudioStream audioStream)
    {
        AudioStreamPlayer musicPlayer = _player._musicPlayer;
        if (musicPlayer.Stream == audioStream) return;
        musicPlayer.Stream = audioStream;
        musicPlayer.VolumeDb = 0f;
    }

    public static void MuteMusic(float time) => SetMuteSpeed(time, MinimalVolume);
    public static void UnmuteMusic(float time) => SetMuteSpeed(time, 0f);

    private static void SetMuteSpeed(float time, float value)
    {
        _muteSpeed = (value - _player._musicPlayer.VolumeDb) / (time * Constants.BaseFramerate);
    }

    private static AudioStreamPlayer CreateSoundPlayer()
    {
        var soundPlayer = new AudioStreamPlayer();
        soundPlayer.Finished += () => _freePlayers.Push(soundPlayer);
        _player._soundPlayers.Add(soundPlayer);
        return soundPlayer;
    }
}
