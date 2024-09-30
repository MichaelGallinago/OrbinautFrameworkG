using Godot;
using OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;
using static OrbinautFrameworkG.Framework.StaticStorages.Constants;

namespace OrbinautFrameworkG.Framework.ObjectBase;

public readonly struct SolidObjectData(ISolid solidData, SolidType type, AttachType attachType, Vector2I extraSize)
{
    public readonly ISolid Target = solidData;
    public readonly Vector2 Position = solidData.SolidBox.Offset + solidData.Position;
    public readonly Vector2I ExtraSize = extraSize;
    public readonly SolidType Type = type;
    public readonly AttachType AttachType = attachType;
}
