using Godot;

public struct AnimationRespawnData
{
    public int Frame;
    public bool IsVisible;
    public StringName Animation;
    
    public AnimationRespawnData(int frame, StringName animation, bool isVisible)
    {
        Frame = frame;
        IsVisible = isVisible;
        Animation = animation;
    }
}
