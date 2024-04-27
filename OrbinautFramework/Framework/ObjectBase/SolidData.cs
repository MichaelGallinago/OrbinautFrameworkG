using Godot;

namespace OrbinautFramework3.Framework.ObjectBase;

public record struct SolidData(bool NoBalance, Vector2I Radius, Vector2I Offset, short[] HeightMap);