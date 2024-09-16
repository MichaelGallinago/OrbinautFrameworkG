using System;
using OrbinautFramework3.Framework;
using System.Collections.Generic;
using Godot;

namespace OrbinautFramework3.Audio.Player;

public class PlayerContainer
{
    private const float MinimalVolume = -80f;
    
    private readonly Dictionary<AudioStream, AudioStreamPlayer> _activePlayers;
    private readonly Dictionary<AudioStreamPlayer, (float speed, bool stop)> _volumeChangeList;
    private readonly Stack<AudioStreamPlayer> _freePlayers;
    private readonly byte _playersLimit;
    private readonly int _busIndex;
    
    public virtual float Volume
    {
        get => _volume;
        set
        {
            MaxVolume = Math.Clamp(_volume = value, 0f, 100f) / 100f * 80f - 80f;
        
            if (!Mathf.IsEqualApprox(AudioServer.GetBusVolumeDb(_busIndex), MaxVolume)) return;
            AudioServer.SetBusVolumeDb(_busIndex, MaxVolume);
        }
    }
    private float _volume;
    protected float MaxVolume { get; private set; }
   
    private float _busMuteSpeed;
    
    public PlayerContainer(IEnumerable<AudioStreamPlayer> players, byte playersLimit)
    {
        _freePlayers = new Stack<AudioStreamPlayer>(players);
        _volumeChangeList = new Dictionary<AudioStreamPlayer, (float speed, bool stop)>(_freePlayers.Count);
        _activePlayers = new Dictionary<AudioStream, AudioStreamPlayer>(_freePlayers.Count);
        
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
        SetMuteSpeed(seconds, music, MinimalVolume, true);
    }
    
    public void StopAllWithMute(float seconds)
    {
        foreach (AudioStreamPlayer player in _activePlayers.Values)
        {
            SetMuteSpeed(seconds, player.Stream, MinimalVolume, true);
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
    
    public void Mute(float seconds, AudioStream audio) => SetMuteSpeed(seconds, audio, MinimalVolume);
    public void Unmute(float seconds, AudioStream audio) => SetMuteSpeed(seconds, audio, Volume);
    
    public void MuteBus(float seconds) => SetBusMuteSpeed(seconds, MinimalVolume);
    public void UnmuteBus(float seconds) => SetBusMuteSpeed(seconds, Volume);
    
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
    
    public void UpdateVolume()
    {
        UpdateBusVolume();
        UpdatePlayersVolume();
    }
    
    private void UpdateBusVolume()
    {
        if (_busMuteSpeed == 0f) return;
        
        float volume = AudioServer.GetBusVolumeDb(_busIndex);
        volume += _busMuteSpeed;

        if (volume > MinimalVolume && volume < Volume)
        {
            AudioServer.SetBusVolumeDb(_busIndex, volume);
            return;
        }

        _busMuteSpeed = 0f;
        AudioServer.SetBusVolumeDb(_busIndex, Math.Clamp(volume, MinimalVolume, Volume));
    }
    
    private void UpdatePlayersVolume()
    {
        foreach (AudioStreamPlayer player in _volumeChangeList.Keys)
        {
            (float speed, bool stop) data = _volumeChangeList[player];
            player.VolumeDb += Scene.Instance.Speed * data.speed;

            if (player.VolumeDb <= MinimalVolume)
            {
                player.VolumeDb = MinimalVolume;
                if (data.stop)
                {
                    player.Stop();
                    FreePlayer(player);
                }
                _volumeChangeList.Remove(player);
            }
            else if (player.VolumeDb >= Volume)
            {
                player.VolumeDb = Volume;
                _volumeChangeList.Remove(player);
            }
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
    
    private void SetMuteSpeed(float seconds, AudioStream music, float value, bool stop = false)
    {
        if (!_activePlayers.TryGetValue(music, out AudioStreamPlayer musicPlayer)) return;
        
        float speed = (value - musicPlayer.VolumeDb) / (seconds * Constants.BaseFrameRate);
        if (_volumeChangeList.TryAdd(musicPlayer, (speed, stop))) return;
        _volumeChangeList[musicPlayer] = (speed, stop);
    }
    
    private void SetBusMuteSpeed(float seconds, float value)
    {
        _busMuteSpeed = (value - AudioServer.GetBusVolumeDb(_busIndex)) / (seconds * Constants.BaseFrameRate);
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
}
