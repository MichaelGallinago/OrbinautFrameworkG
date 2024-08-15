using Godot;
using OrbinautFramework3.Objects.Player;

namespace OrbinautFramework3.Framework;

public partial class PlayerPrefabs : Resource
{
    [Export] public PackedScene Sonic { get; init; }
    [Export] public PackedScene Tails { get; init; }
    [Export] public PackedScene Knuckles { get; init; }
    [Export] public PackedScene Amy { get; init; }
    
    public PackedScene Get(PlayerNode.Types type) => type switch
    {
        PlayerNode.Types.Tails => Tails,
        PlayerNode.Types.Knuckles => Knuckles,
        PlayerNode.Types.Amy => Amy,
        _ => Sonic
    };
}
