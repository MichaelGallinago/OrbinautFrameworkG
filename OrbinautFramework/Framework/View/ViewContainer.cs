using Godot;

namespace OrbinautFramework3.Framework.View;

public partial class ViewContainer : Control
{
    [Export] public Camera Camera { get; private set; }
    [Export] public SubViewport SubViewport { get; private set; }
}
