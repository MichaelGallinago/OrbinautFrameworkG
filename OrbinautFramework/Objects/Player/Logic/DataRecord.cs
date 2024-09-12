using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.InputModule;

namespace OrbinautFramework3.Objects.Player.Logic;

public readonly record struct DataRecord(
    Vector2I Position,
    Buttons InputPress,
    Buttons InputDown,
    Constants.Direction Facing,
    object SetPushAnimationBy
);
