using Godot;
using System;

public partial class CommonObject : Node2D
{
    public ObjectRespawnData RespawnData { get; set; }

    public CommonObject()
    {
        //TODO: depth
        RespawnData = new ObjectRespawnData(Position, Scale, Visible, 0);
    }
}
