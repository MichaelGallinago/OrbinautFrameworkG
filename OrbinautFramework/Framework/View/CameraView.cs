using Godot;

namespace OrbinautFramework3.Framework.View;

public partial class CameraView : SubViewport
{
    [Export] public Camera Camera { get; }
}