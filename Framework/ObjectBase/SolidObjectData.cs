using Godot;

namespace OrbinautFramework3.Framework.ObjectBase;

public readonly ref struct SolidObjectData(BaseObject baseObject, Constants.SolidType type)
{
    public readonly Vector2I Radius = baseObject.SolidData.Radius;
    public readonly Vector2 Position = baseObject.SolidData.Offset + baseObject.Position;
    public readonly Constants.SolidType Type = type;
}