using Godot;
using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework.InputModule;

namespace OrbinautFrameworkG.Framework.SceneModule;

public partial class SceneContinuousUpdate : Node
{
    private SceneTree _sceneTree;

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
        if (!Scene.Instance.AllowPause || !InputUtilities.Press[0].Start) return;
        
        bool isPause = Scene.Instance.State != Scene.States.Paused;
        Scene.Instance.State = isPause ? Scene.States.Paused : Scene.States.Normal;
        _sceneTree.Paused = isPause;
        AudioPlayer.SetPauseState(isPause);
    }
}
