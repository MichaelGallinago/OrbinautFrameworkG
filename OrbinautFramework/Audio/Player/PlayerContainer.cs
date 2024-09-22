using System;
using OrbinautFramework3.Framework;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace OrbinautFramework3.Audio.Player;

public class PlayerContainer
{
    private readonly record struct VolumeChangeData(float Speed, float TargetVolume, bool Stop);
    
    private readonly Dictionary<AudioStream, AudioStreamPlayer> _activePlayers;
    private readonly Dictionary<AudioStreamPlayer, VolumeChangeData> _volumeChangeList;
    private readonly Stack<AudioStreamPlayer> _freePlayers;
    private readonly Dictionary<AudioStreamPlayer, float> _audioStreamVolumes;
    private readonly byte _playersLimit;
    private readonly int _busIndex;
   
    private float _busMuteSpeed;
    private float _targetVolume;
    private float _volume; // TODO: Where is NaN?
    
    public virtual float MaxVolume
    {
        get => _maxVolume;
        set
        {
            _volume = _targetVolume = _maxVolume = value;
            VolumeDb = GetVolumeDb(value);
            AudioServer.SetBusVolumeDb(_busIndex, VolumeDb);
        }
    }
    private float _maxVolume;
    protected float VolumeDb { get; private set; }
    
    public PlayerContainer(IEnumerable<AudioStreamPlayer> players, byte playersLimit)
    {
        _freePlayers = new Stack<AudioStreamPlayer>(players);
        _volumeChangeList = new Dictionary<AudioStreamPlayer, VolumeChangeData>(_freePlayers.Count);
        _activePlayers = new Dictionary<AudioStream, AudioStreamPlayer>(_freePlayers.Count);
        _audioStreamVolumes = _freePlayers.ToDictionary(key => key, _ => 1f);
        
        foreach (AudioStreamPlayer soundPlayer in _freePlayers)
        {
            soundPlayer.Finished += () => FreePlayer(soundPlayer);
        }
        
        _playersLimit = playersLimit;
        
        StringName busName = _freePlayers.Peek().Bus;
        foreach (AudioStreamPlayer player in _freePlayers)
        {
            player.Bus = busName;
        }
        _busIndex = AudioServer.GetBusIndex(busName);
    }
    
    public bool IsPlaying(AudioStream audio) => _activePlayers.ContainsKey(audio);
    public bool IsAnyPlaying() => _activePlayers.Count > 0;
    public void Play(AudioStream audio) => GetPlayer(audio)?.Play();
    
    public void PlayPitched(AudioStream audio, float pitch)
    {
        AudioStreamPlayer soundPlayer = GetPlayer(audio);
        if (soundPlayer == null) return;
        soundPlayer.PitchScale = pitch;
        soundPlayer.Play();
    }
    
    public void Stop(AudioStream audio)
    {
        if (!_activePlayers.TryGetValue(audio, out AudioStreamPlayer soundPlayer)) return;
        soundPlayer.Stop();
        FreePlayer(soundPlayer);
    }
    
    public void StopWithMute(float seconds, AudioStream music)
    {
        SetMuteSpeed(seconds, music, 0, true);
    }
    
    public void StopAllWithMute(float seconds)
    {
        foreach (AudioStreamPlayer player in _activePlayers.Values)
        {
            SetMuteSpeed(seconds, player.Stream, 0, true);
        }
    }
    
    public void StopAll()
    {
        foreach (AudioStreamPlayer player in _activePlayers.Values)
        {
            player.Stop();
            FreePlayer(player);
        }
    }
    
    public void Mute(float seconds, AudioStream audio) => SetMuteSpeed(seconds, audio, 0f);
    public void Unmute(float seconds, AudioStream audio) => SetMuteSpeed(seconds, audio, 1f);
    
    public void MuteBus(float seconds) => SetBusMuteSpeed(seconds, 0f);
    public void UnmuteBus(float seconds) => SetBusMuteSpeed(seconds, MaxVolume);
    
    public void SetPauseState(bool isPaused)
    {
        foreach (AudioStreamPlayer soundPlayers in _activePlayers.Values)
        {
            soundPlayers.StreamPaused = isPaused;
        }
        
        foreach (AudioStreamPlayer soundPlayers in _freePlayers)
        {
            soundPlayers.StreamPaused = isPaused;
        }
    }
    
    public void UpdateVolume(float speed)
    {
        UpdateBusVolume(speed);
        UpdatePlayersVolume(speed);
    }
    
    private void UpdateBusVolume(float speed)
    {
        if (_busMuteSpeed == 0f) return;
        
        _volume = _volume.MoveTowardChecked(_targetVolume, _busMuteSpeed * speed, out bool isFinished);
        
        if (isFinished)
        {
            _busMuteSpeed = 0f;
        }
        
        AudioServer.SetBusVolumeDb(_busIndex, _volume);
    }
    
    private void UpdatePlayersVolume(float speed)
    {
        foreach (AudioStreamPlayer player in _volumeChangeList.Keys)
        {
            (float muteSpeed, float to, bool stop) = _volumeChangeList[player];

            float delta = speed * muteSpeed;
            float newVolume = _audioStreamVolumes[player].MoveTowardChecked(to, delta, out bool isFinished);
            
            _audioStreamVolumes[player] = newVolume;
            player.VolumeDb = GetVolumeDb(newVolume);
            
            if (!isFinished) continue;
            _volumeChangeList.Remove(player);
            
            if (!stop) continue;
            player.Stop();
            FreePlayer(player);
        }
    }
    
    private AudioStreamPlayer GetPlayer(AudioStream audio)
    {
        if (_activePlayers.TryGetValue(audio, out AudioStreamPlayer soundPlayer)) return soundPlayer;
        
        if (!_freePlayers.TryPop(out soundPlayer))
        {
            if (_activePlayers.Count >= _playersLimit)
            {
#if TOOLS
                GD.PrintErr($"Not enough players! The audio \"{audio.ResourceName}\" was skipped");
#endif
                return null;
            }
            soundPlayer = CreatePlayer();
        }
        
        _activePlayers.Add(audio, soundPlayer);
        soundPlayer.Stream = audio;

        return soundPlayer;
    }
    
    private void SetMuteSpeed(float seconds, AudioStream music, float targetVolume, bool stop = false)
    {
        if (!_activePlayers.TryGetValue(music, out AudioStreamPlayer musicPlayer)) return;
        
        float speed = Math.Abs(targetVolume - _audioStreamVolumes[musicPlayer]) / (seconds * Constants.BaseFrameRate);
        var volumeChangeData = new VolumeChangeData(speed, targetVolume, stop);
        if (_volumeChangeList.TryAdd(musicPlayer, volumeChangeData)) return;
        _volumeChangeList[musicPlayer] = volumeChangeData;
    }
    
    private void SetBusMuteSpeed(float seconds, float value)
    {
        if (seconds <= 0f)
        {
            _busMuteSpeed = float.PositiveInfinity;
            return;
        }
        
        _busMuteSpeed = MathF.Abs(value - _volume) / (seconds * Constants.BaseFrameRate);
    }
    
    private AudioStreamPlayer CreatePlayer()
    {
        var player = new AudioStreamPlayer();
        player.Finished += () => FreePlayer(player);
        return player;
    }
    
    private void FreePlayer(AudioStreamPlayer player)
    {
        _activePlayers.Remove(player.Stream);
        player.PitchScale = 1f;
        player.VolumeDb = 0f;
        player.Stream = null;
        _freePlayers.Push(player);
    }
    
    private static float GetVolumeDb(float volume) => 20f * MathF.Log10(Math.Clamp(volume, 0.0001f, 1f));
}
