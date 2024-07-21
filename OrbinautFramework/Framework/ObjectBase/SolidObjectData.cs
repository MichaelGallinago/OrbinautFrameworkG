using Godot;

namespace OrbinautFramework3.Framework.ObjectBase;

public readonly struct SolidObjectData(BaseObject baseObject, Constants.SolidType type, Vector2I extraSize)
{
    public readonly BaseObject Target = baseObject;
    public readonly Vector2 Position = baseObject.SolidData.Offset + baseObject.Position;
    public readonly Vector2I ExtraSize = extraSize;
    public readonly Constants.SolidType Type = type;
}