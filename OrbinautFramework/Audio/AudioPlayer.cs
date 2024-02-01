using Godot;
using Godot.Collections;

[assembly: AudioStorageSourceGenerator.AudioStorage("SfxStorage", "OrbinautFramework3.Audio", "res://Audio/SFX/")]

namespace OrbinautFramework3.Audio;

public partial class AudioPlayer : Node2D
{
    [ExportGroup("AudioStreamPlayers")]
    [Export] private AudioStreamPlayer _bgmPlayer;
    [Export] private Array<AudioStreamPlayer> _sfxPlayers;
    
    public override void _Process(double delta)
    {
        //_bgmPlayer.;
    }
}
