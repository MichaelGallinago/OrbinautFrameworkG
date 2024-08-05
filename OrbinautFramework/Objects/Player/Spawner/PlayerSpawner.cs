using System;
using Godot;

namespace OrbinautFramework3.Objects.Player.Spawner;

public partial class PlayerSpawner : Sprite2D
{
    public enum SpawnTypes : byte
    {
        Global, GlobalAI, Unique
    }
    
    [Export] private SpawnTypes _spawnType;
    [Export] private PackedScene _uniquePlayer;

    public override void _Ready()
    {
        Visible = false;
        
        switch (_spawnType)
        {
            case SpawnTypes.Global:
                break;
            case SpawnTypes.GlobalAI:
                break;
            case SpawnTypes.Unique:
                break;
            default: throw new ArgumentOutOfRangeException();
        }
        
        GetParent().AddChild(_uniquePlayer.Instantiate());
        QueueFree();
    }
}
