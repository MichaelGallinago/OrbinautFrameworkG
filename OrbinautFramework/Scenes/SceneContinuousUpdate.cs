using Godot;
using JetBrains.Annotations;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework.InputModule;

namespace OrbinautFramework3.Scenes;

public partial class SceneContinuousUpdate : Node
{
    private SceneTree _sceneTree;
    [UsedImplicitly] private IScene _scene;

    public SceneContinuousUpdate()
    {
        ProcessPriority = int.MinValue;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Ready() => _sceneTree = GetTree();
    
    public override void _Process(double delta)
    {
        InputUtilities.Update();
        UpdatePause();
    }

    private void UpdatePause()
    {
        if (!_scene.AllowPause || !InputUtilities.Press[0].Start) return;
        
        bool isPause = _scene.State != Scene.States.Paused;
        _scene.State = isPause ? Scene.States.Paused : Scene.States.Normal;
        _sceneTree.Paused = isPause;
        AudioPlayer.SetPauseState(isPause);
    }
}
