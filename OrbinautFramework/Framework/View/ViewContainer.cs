using Godot;

namespace OrbinautFramework3.Framework.View;

public partial class ViewContainer : SubViewportContainer
{
    [Export] public Camera Camera { get; }
    [Export] public SubViewport SubViewport { get; }
}
