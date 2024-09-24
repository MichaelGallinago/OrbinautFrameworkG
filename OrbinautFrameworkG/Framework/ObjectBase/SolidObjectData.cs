using Godot;
using static OrbinautFrameworkG.Framework.Constants;

namespace OrbinautFrameworkG.Framework.ObjectBase;

public readonly struct SolidObjectData(ISolid orbinautData, Constants.SolidType type, Constants.AttachType attachType, Vector2I extraSize)
{
    public readonly ISolid Target = orbinautData;
    public readonly Vector2 Position = orbinautData.SolidBox.Offset + orbinautData.Position;
    public readonly Vector2I ExtraSize = extraSize;
    public readonly Constants.SolidType Type = type;
    public readonly Constants.AttachType AttachType = attachType;
}
