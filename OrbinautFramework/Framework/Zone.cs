using Godot;
using OrbinautFramework3.Audio.Player;

namespace OrbinautFramework3.Framework;

public abstract partial class Zone : Scene
{
    public static Zone Local { get; private set; } //TODO: singleton
    
    [Export] public string ZoneName { get; set; } = "UNKNOWN";
    [Export] public byte ActId { get; set; }
    [Export] private uint InitialWaterLevel { get; set; } = ushort.MaxValue;
    [Export] public bool IsWaterEnabled { get; set; }
    [Export] public AudioStream Music { get; set; }
    public uint WaterLevel { get; set; }
    
    /*
    animal_set       =  [];
    next_stage	     =  noone;
    save_progress    =  false;
    */

    public override void _EnterTree()
    {
        base._EnterTree();
        Local = this;
    }
    
    public override void _ExitTree()
    {
        base._ExitTree();
        Local = null;
    }
    
    public override void _Ready()
    {
        //TODO: Zone init
        base._Ready();
        if (Music != null)
        {
            AudioPlayer.Music.Play(Music);
        }
    }
}
