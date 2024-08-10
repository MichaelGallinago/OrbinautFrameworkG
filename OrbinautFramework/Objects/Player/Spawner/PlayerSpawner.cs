using System.Linq;
using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player.Spawner;

public partial class PlayerSpawner : Sprite2D
{
    [Export] private PlayerNode.Types[] _allowedTypes;
    [Export] private Vector2 _cpuOffset;

    public override void _Ready()
    {
        SpawnPlayers();
        QueueFree();
    }

    private void SpawnPlayers()
    {
        if (Scene.Instance.Players.Count > 0) return;

        PlayerNode.Types type = SharedData.PlayerTypes.First();
        if (!_allowedTypes.Contains(type)) return;

        Node spawnerParent = GetParent();
        SpawnPlayer(type, spawnerParent, Position);

        Vector2 offsetPosition = Position + _cpuOffset;
        for (var i = 1; i < SharedData.PlayerTypes.Length; i++)
        {
            SpawnPlayer(type, spawnerParent, offsetPosition);
        }
    }

    private static void SpawnPlayer(PlayerNode.Types type, Node spawnerParent, Vector2 position)
    {
        PackedScene packedPlayer = Scene.Instance.PrefabStorage.GetPlayer(type);
        if (packedPlayer.Instantiate() is not PlayerNode player) return;

        Scene.Instance.Players.Add(player);
        spawnerParent.AddChild(player);
        player.Position = position;
    }
}
