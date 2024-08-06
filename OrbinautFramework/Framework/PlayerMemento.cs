using Godot;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Player;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Framework;

public class PlayerMemento(PlayerData originator) : IMemento
{
    public Vector2 Position { get; } = originator.Owner.Position;
    private readonly Vector2 _scale = originator.Owner.Scale;
    private readonly int _zIndex = originator.Owner.ZIndex;
    
    public void Reset()
    {
        originator.Owner.Position = Position;
        originator.Owner.ZIndex = _zIndex;
        originator.Owner.Scale = _scale;

        originator.Init();
    }
}
