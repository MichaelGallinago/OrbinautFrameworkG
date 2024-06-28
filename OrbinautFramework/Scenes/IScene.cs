using Godot;
using OrbinautFramework3.Framework.View;

namespace OrbinautFramework3.Scenes;

public interface IScene
{
    float Time { get; }
    bool IsStage { get; }
    IViews Views { get; }
    PlayerList Players { get; }
    bool AllowPause { get; set; }
    Scene.States State { get; set; }
    float RingSpillTimer { get; set; }
    CollisionTileMap CollisionTileMapMain { get; }
    CollisionTileMap CollisionTileMapSecondary { get; }
    
    void Reload();
    World2D GetWorld2D();
    void Change(string path);
    bool IsTimePeriodLooped(float period);
    bool IsTimePeriodLooped(float period, float offset);
    void AddChild(Node node, bool forceReadableName = false, Node.InternalMode @internal = Node.InternalMode.Disabled);
}
