using OrbinautFramework3.Framework.View;

namespace OrbinautFramework3.Objects.Player.Sprite;

public interface IPlayerSprite
{
    protected SpriteData Data { get; }
    protected ISpriteNode Node { get; }

    int Frame => Node.Frame;
    int FrameCount => Data.FrameCount;
    bool IsFinished => Data.IsFinished;
    bool IsFrameChanged => Data.IsFrameChanged;
    
    Animations Animation
    {
        get => Data.Animation; //TODO: remove this somehow?
        set
        {
            Data.Animation = value;
            OnAnimationChanged(value);
        }
    }

    bool CheckInCamera(ICamera camera) => Node.CheckInCamera(camera);
    bool CheckInCameras() => Node.CheckInCameras();
    protected void OnAnimationChanged(Animations animation);
}
