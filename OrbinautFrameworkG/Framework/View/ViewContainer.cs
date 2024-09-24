using Godot;

namespace OrbinautFrameworkG.Framework.View;

public partial class ViewContainer : Control
{
    [Export] public Camera Camera { get; private set; }
    [Export] public SubViewport SubViewport { get; private set; }
}
