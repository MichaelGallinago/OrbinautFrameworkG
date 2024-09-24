using Godot;

namespace OrbinautFrameworkG.Framework;

public record CheckpointData(Vector2I Position, int FrameCounter, int BottomCameraBound, int Id);
