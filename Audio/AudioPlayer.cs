using Godot;
using Godot.Collections;

namespace OrbinautFramework3.Audio;

public partial class AudioPlayer : Node2D
{
    [Export] private AudioStreamPlayer _bgmPlayer;
    [Export] private Array<AudioStreamPlayer> _sfxPlayers;

    public override void _Process(double delta)
    {
        _bgmPlayer.;
    }
}