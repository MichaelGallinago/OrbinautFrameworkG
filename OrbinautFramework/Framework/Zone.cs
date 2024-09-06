using Godot;
using OrbinautFramework3.Audio.Player;

namespace OrbinautFramework3.Framework;

public partial class Zone : Scene
{
    public new static Zone Instance { get; private set; }
    
    [Export] public string ZoneName { get; set; } = "UNKNOWN";
    [Export(PropertyHint.Range, "0,3,")] public byte ActId { get; set; }
    [Export] public AudioStream Music { get; set; }
    [Export(PropertyHint.Range, "0,65535,")] private uint _initialWaterLevel = ushort.MaxValue;
    
    public uint WaterLevel { get; set; }
    public bool IsWaterEnabled { get; set; }
    
    /*
    animal_set       =  [];
    next_stage	     =  noone;
    save_progress    =  false;
    */

    public override void _EnterTree()
    {
        base._EnterTree();
        if (Instance == null)
        {
            Instance = this;
            return;
        }
        
        QueueFree();
    }
    
    public override void _ExitTree()
    {
        base._ExitTree();
        if (Instance != this) return;
        Instance = null;
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
