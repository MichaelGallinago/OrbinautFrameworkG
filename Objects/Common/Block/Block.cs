using Godot;
using System;

public partial class Block : CommonObject
{
    public Block()
    {
        SetSolid(new Vector2I(16, 16));
    }
}
