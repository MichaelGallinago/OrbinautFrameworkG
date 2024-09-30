using OrbinautFrameworkG.Framework.View;

namespace OrbinautFrameworkG.Objects.Player.Sprite;

public interface IPlayerSprite
{
    protected SpriteLogic SpriteLogic { get; }
    
    int Frame { get; }
    int FrameCount => SpriteLogic.Data.FrameCount;
    bool IsFinished => SpriteLogic.Data.IsFinished;
    bool IsFrameChanged => SpriteLogic.Data.IsFrameChanged;
    
    Animations Animation
    {
        get => SpriteLogic.Data.Animation; //TODO: remove this somehow?
        set => SpriteLogic.ChangeAnimation(value);
    }
    
    bool CheckInCameras();
    bool CheckInCamera(ICamera camera);
    void Process() => SpriteLogic.Process();
}
