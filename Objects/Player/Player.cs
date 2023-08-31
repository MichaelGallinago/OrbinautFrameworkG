using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CommonObject
{
    public static List<CommonObject> Players { get; }

    [Export] public PlayerConstants.Type Type;
    
    public bool IsEditMode { get; private set; }
    public int EditModeIndex { get; private set; }
    public float EditModeSpeed { get; private set; }
    public List<CommonObject> EditModeObjects { get; private set; }
    
    static Player()
    {
        Players = new List<CommonObject>();
    }
    
    public override void _Ready()
    {
        RespawnData.ProcessType = Constants.ProcessType.Default;
    }

    public override void _EnterTree()
    {
        base._EnterTree();
        Players.Add(this);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Players.Remove(this);
    }

    protected override void BeginStep(double processSpeed)
    {
        EditModeObjects = new List<CommonObject>
        { 
            new Ring(), new GiantRing(), new ItemBox(), new Spring(), new Motobug(), new Signpost()
        };
    }

    private void EditModeInit()
    {
        switch (FrameworkData.CurrentScene)
        {
            case StageTSZ:
                GD.Print("This is TSZ");
                EditModeObjects.AddRange(new List<CommonObject>
                {
                    //obj_platform_swing_tsz, obj_platform_tsz, obj_falling_floor_tsz, obj_block_tsz
                });
                break;
        }
    }
}
