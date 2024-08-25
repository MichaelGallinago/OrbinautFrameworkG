using System;
using System.Linq;
using Godot;
using OrbinautFramework3.Framework;
using static OrbinautFramework3.Objects.Player.PlayerNode;

namespace OrbinautFramework3.Objects.Player.Spawner;

public partial class PlayerSpawner : Sprite2D
{
    [Flags] public enum AllowedTypes : byte
    {
        Sonic = 1 << Types.Sonic,
        Tails = 1 << Types.Tails,
        Knuckles = 1 << Types.Knuckles,
        Amy = 1 << Types.Amy,
        All = Sonic | Tails | Knuckles | Amy
    }

    [Export] private AllowedTypes _allowedTypes = AllowedTypes.All;
    [Export] private Vector2 _cpuOffset;

    public override void _Ready()
    {
        SpawnPlayers();
        QueueFree();
    }

    private void SpawnPlayers()
    {
        if (Scene.Instance.Players.Count > 0) return;

        Types type = SharedData.PlayerTypes.First();
        if (!_allowedTypes.HasFlag((AllowedTypes)(1 << (int)type))) return;

        Node spawnerParent = GetParent();
        SpawnPlayer(type, spawnerParent, Position);

        Vector2 offsetPosition = Position + _cpuOffset;
        for (var i = 1; i < SharedData.PlayerTypes.Length; i++)
        {
            SpawnPlayer(type, spawnerParent, offsetPosition);
        }
    }

    private static void SpawnPlayer(Types type, Node spawnerParent, Vector2 position)
    {
        PackedScene packedPlayer = Scene.Instance.PlayerPrefabs.Get(type);
        if (packedPlayer.Instantiate() is not PlayerNode player) return;
        
        spawnerParent.AddChild(player);
        player.Position = position;
    }
}
