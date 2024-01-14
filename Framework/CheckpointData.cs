using Godot;

namespace OrbinautFramework3.Framework;

public record CheckpointData(Vector2I position, int frameCounter, int bottomCameraBound, int id);
