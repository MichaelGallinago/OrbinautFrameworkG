using Godot;

namespace OrbinautFramework3.Framework.ObjectBase;

public struct AnimationRespawnData(int frame, StringName animation, bool isVisible)
{
    public int Frame = frame;
    public bool IsVisible = isVisible;
    public StringName Animation = animation;
}