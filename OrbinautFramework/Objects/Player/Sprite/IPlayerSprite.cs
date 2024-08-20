using OrbinautFramework3.Framework.View;

namespace OrbinautFramework3.Objects.Player.Sprite;

public interface IPlayerSprite
{
    protected SpriteData Data { get; }
    protected ISpriteNode Node { get; }

    int Frame => Node.Frame; 
    bool IsFinished => Data.IsFinished;
    bool IsFrameChanged => Data.IsFrameChanged;
    
    Animations Animation
    {
        get => Data.Animation; //TODO: remove this somehow?
        set => Data.Animation = value; 
    }
    
    bool CheckInCamera(ICamera camera) => Node.CheckInCamera(camera);
    bool CheckInCameras() => Node.CheckInCameras();
}
