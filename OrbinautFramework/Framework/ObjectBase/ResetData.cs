using Godot;

namespace OrbinautFramework3.Framework.ObjectBase;

public readonly record struct ResetData(bool IsVisible, Vector2 Scale, Vector2 Position, int ZIndex);
