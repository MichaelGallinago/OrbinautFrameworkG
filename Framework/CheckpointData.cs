using Godot;

namespace OrbinautFramework3.Framework;

public class CheckpointData
{
    public Vector2I Position { get; }
    public int FrameCounter { get; }
    public int BottomCameraBound { get; }
    public int Id { get; }

    public CheckpointData(Vector2I position, int frameCounter, int bottomCameraBound, int id)
    {
        Position = position;
        FrameCounter = frameCounter;
        BottomCameraBound = bottomCameraBound;
        Id = id;
    }
}