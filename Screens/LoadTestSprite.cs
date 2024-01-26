using Godot;

namespace OrbinautFramework3.Screens;

public partial class LoadTestSprite : Sprite2D
{
    public override void _Process(double delta)
    {
        Position += Vector2.Right * (float)delta * 120f;

        if (Position.X >= 400f)
        {
            GetTree().ChangeSceneToFile("res://Screens/Startup/startup.tscn");
        }
    }
}