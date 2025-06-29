using System;
using System.Linq;
using Godot;
using OrbinautFrameworkG.Framework.SceneModule;
using OrbinautFrameworkG.Framework.StaticStorages;
using static OrbinautFrameworkG.Objects.Player.PlayerNode;

namespace OrbinautFrameworkG.Objects.Player.Spawner;

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
    [Export] private PlayerPrefabs _playerPrefabs;
    
    public override void _Process(double delta)
    {
        SpawnPlayers();
        QueueFree();
    }

    private void SpawnPlayers()
    {
        if (Scene.Instance.Players.Count > 0) return;

        Types type = SaveData.PlayerTypes.First();
        if (!_allowedTypes.HasFlag((AllowedTypes)(1 << (int)type))) return;
        
        SpawnPlayer(type, Position);

        Vector2 offsetPosition = Position + _cpuOffset;
        for (var i = 1; i < SaveData.PlayerTypes.Count; i++)
        {
            SpawnPlayer(type, offsetPosition);
        }

        Scene.Instance.Views.AttachCamerasToPlayers();
    }
    
    private void SpawnPlayer(Types type, Vector2 position)
    {
        if (_playerPrefabs.Get(type).Instantiate() is not PlayerNode player) return;
        
        player.Position = position;
        Scene.Instance.AddChild(player);
    }
}
