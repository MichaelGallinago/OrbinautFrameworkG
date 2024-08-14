using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.InputModule;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Player;

public readonly record struct DataRecord(
    Vector2 Position,
    Buttons InputPress,
    Buttons InputDown,
    Constants.Direction Facing,
    object SetPushAnimationBy //TODO: replace OrbinautNode with interface
);
