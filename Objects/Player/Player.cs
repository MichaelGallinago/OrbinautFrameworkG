using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CommonObject
{
    public static List<CommonObject> Players { get; }

    static Player()
    {
        Players = new List<CommonObject>();
    }

    public Player()
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
}
