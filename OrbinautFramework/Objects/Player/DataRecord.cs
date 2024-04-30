using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.InputModule;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Player;

public record DataRecord(
    Vector2 Position,
    Buttons InputPress,
    Buttons InputDown,
    Constants.Direction Facing,
    BaseObject SetPushAnimationBy
);
