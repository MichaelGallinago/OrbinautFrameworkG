using Godot;
using OrbinautFramework3.Objects.Player;

namespace OrbinautFramework3.Framework;

public partial class PlayerPrefabs : Resource
{
    [Export] public PackedScene Sonic { get; private set; }
    [Export] public PackedScene Tails { get; private set; }
    [Export] public PackedScene Knuckles { get; private set; }
    [Export] public PackedScene Amy { get; private set; }
    
    public PackedScene Get(PlayerNode.Types type) => type switch
    {
        PlayerNode.Types.Tails => Tails,
        PlayerNode.Types.Knuckles => Knuckles,
        PlayerNode.Types.Amy => Amy,
        _ => Sonic
    };
}
