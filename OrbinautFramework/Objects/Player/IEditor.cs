using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player;

public interface IEditor
{
    Vector2 Position { get; set; }
    Constants.Direction Facing { get; }
    
    void OnEnableEditMode();
    void OnDisableEditMode();
}
