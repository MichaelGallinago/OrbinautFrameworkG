using Godot;
using OrbinautFramework3.Framework.View;

namespace OrbinautFramework3.Objects.Player.Sprite;

public interface ISpriteNode
{ 
    int Frame { get; set; }
    Vector2 Scale { get; set; }
    float FrameProgress { get; }
    SpriteFrames SpriteFrames { get; }
    
    void SetFrameAndProgress(int frame, float progress);
    void SetAnimation(StringName animation, float customSpeed = 1f);
    void SetAnimation(StringName animation, int startFrame, float customSpeed = 1f);
    bool CheckInCamera(ICamera camera);
    bool CheckInCameras();
}
