using Godot;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.InputModule;

namespace OrbinautFrameworkG.Objects.Player.Logic;

public readonly record struct DataRecord(
    Vector2I Position,
    Buttons InputPress,
    Buttons InputDown,
    Constants.Direction Facing,
    object SetPushAnimationBy
);
