using Godot;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Player;

namespace OrbinautFramework3.Framework;

public class PlayerMemento(Player originator) : IMemento
{
    public Vector2 Position { get; } = originator.Position;
    
    private readonly bool _isVisible = originator.Visible;
    private readonly Vector2 _scale = originator.Scale;
    private readonly int _zIndex = originator.ZIndex;
    
    public void Reset()
    {
        originator.Visible = _isVisible;
        originator.Position = Position;
        originator.ZIndex = _zIndex;
        originator.Scale = _scale;

        originator.Init();
    }
}