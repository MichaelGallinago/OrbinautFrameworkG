using Godot;
using OrbinautFramework3.Framework.View;

namespace OrbinautFramework3.Objects.Player.Sprite;

public interface ISpriteNode
{ 
    int Frame { get; set; }
    Vector2 Scale { get; set; }
    float FrameProgress { get; }
    SpriteFrames SpriteFrames { get; }
    
    void PlayAnimation(StringName animation);
    void PlayAnimation(StringName animation, float customSpeed);
    void PlayAnimation(StringName animation, int startFrame);
    void PlayAnimation(StringName animation, int startFrame, float customSpeed);
    void SetFrameAndProgress(int frame, float progress);
    bool CheckInCamera(ICamera camera);
    bool CheckInCameras();
}
