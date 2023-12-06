using Godot;

namespace OrbinautFramework3.Framework.Audio;

public partial class Audio : AudioStreamPlayer
{
    public enum StateType : byte
    {
        Default, Mute, Stop
    }
    
    public StateType State = StateType.Default;
    public float VolumeSfx = SharedData.SoundVolume;
    public float VolumeBgm = SharedData.MusicVolume;
    
    private AudioStreamPlayer _audioStreamPlayer = new();

    public Audio() => AddChild(_audioStreamPlayer);
    
    // TODO: Audio
}