using Godot;

namespace OrbinautFramework3.Framework;

public record CheckpointData(Vector2I Position, int FrameCounter, int BottomCameraBound, int Id);
