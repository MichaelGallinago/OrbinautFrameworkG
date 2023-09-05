using Godot;
using System;

public partial class Barrier : CommonObject
{
    public float Angle { get; set; }
    public Player Target { get; set; }

    public Barrier(Player target)
    {
        Target = target;
    }
    
    public override void _Ready()
    {
        base._Ready();
    }
}
