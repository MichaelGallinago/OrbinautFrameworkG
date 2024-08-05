using Godot;

namespace OrbinautFramework3.Framework;

//TODO: fill this
public partial class PrefabStorage : Resource
{
    [Export] public PackedScene PlayerSonic { get; private set; }
    [Export] public PackedScene PlayerKnuckles { get; private set; }
    [Export] public PackedScene PlayerAmy { get; private set; }
    [Export] public PackedScene PlayerTails { get; private set; }
}