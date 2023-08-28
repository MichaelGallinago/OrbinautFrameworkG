using Godot;
using System;

public partial class Player : CommonObject
{
    public Player()
    {
        RespawnData.ProcessType = Constants.ProcessType.Default;
    }
}
