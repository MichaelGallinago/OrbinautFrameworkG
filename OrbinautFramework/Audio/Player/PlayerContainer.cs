using OrbinautFramework3.Framework;
using System.Collections.Generic;
using Godot;

namespace OrbinautFramework3.Audio.Player;

public class PlayerContainer
{
    private const float MinimalVolume = -80f;
    private const float DefaultVolume = 0f;
    
    private readonly Dictionary<AudioStream, AudioStreamPlayer> _activePlayers;
    private readonly Dictionary<AudioStreamPlayer, (float speed, bool stop)> _volumeChangeList;
    private readonly Stack<AudioStreamPlayer> _freePlayers;
    private readonly byte _playersLimit;

    public PlayerContainer(ref Godot.Collections.Array<AudioStreamPlayer> players, byte playersLimit)
    {
        _freePlayers = new Stack<AudioStreamPlayer>(players);
        _volumeChangeList = new Dictionary<AudioStreamPlayer, (float speed, bool stop)>(_freePlayers.Count);
        _activePlayers = new Dictionary<AudioStream, AudioStreamPlayer>(_freePlayers.Count);
        players.Clear();
        players = null;
        
        foreach (AudioStreamPlayer soundPlayer in _freePlayers)
        {
            soundPlayer.Finished += () => FreePlayer(soundPlayer);
        }

        _playersLimit = playersLimit;
    }

    public void UpdateVolume()
    {
        foreach (AudioStreamPlayer player in _volumeChangeList.Keys)
        {
            (float speed, bool stop) data = _volumeChangeList[player];
            player.VolumeDb += FrameworkData.ProcessSpeed * data.speed;
            
            switch (player.VolumeDb)
            {
                case <= MinimalVolume:
                    player.VolumeDb = MinimalVolume;
                    if (data.stop)
                    {
                        player.Stop();
                        FreePlayer(player);
                    }
                    _volumeChangeList.Remove(player);
                    break;
                
                case >= DefaultVolume:
                    player.VolumeDb = DefaultVolume;
                    _volumeChangeList.Remove(player);
                    break;
            }
        }
    }

    public bool IsPlaying(AudioStream audio) => _activePlayers.ContainsKey(audio);
    public bool IsAnyPlaying() => _activePlayers.Count > 0;
    public void Play(AudioStream audio) => GetPlayer(audio)?.Play();
    
    public void PlayPitched(AudioStream audio, float pitch)
    {
        AudioStreamPlayer soundPlayer = GetPlayer(audio);
        soundPlayer.PitchScale = pitch;
        soundPlayer.Play();
    }
    
    public void Stop(AudioStream audio)
    {
        if (!_activePlayers.TryGetValue(audio, out AudioStreamPlayer soundPlayer)) return;
        soundPlayer.Stop();
        FreePlayer(soundPlayer);
    }

    public void StopWithMute(float time, AudioStream music) => SetMuteSpeed(time, music, MinimalVolume, true);
    
    public void StopAllWithMute(float time)
    {
        foreach (AudioStreamPlayer player in _activePlayers.Values)
        {
            SetMuteSpeed(time, player.Stream, MinimalVolume, true);
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
    
    private void SetMuteSpeed(float time, AudioStream music, float value, bool stop = false)
    {
        if (!_activePlayers.TryGetValue(music, out AudioStreamPlayer musicPlayer)) return;
        
        float speed = (value - musicPlayer.VolumeDb) / (time * Constants.BaseFramerate);
        if (_volumeChangeList.TryAdd(musicPlayer, (speed, stop))) return;
        _volumeChangeList[musicPlayer] = (speed, stop);
    }

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
