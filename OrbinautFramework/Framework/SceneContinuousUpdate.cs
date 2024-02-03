using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework.Input;

namespace OrbinautFramework3.Framework;

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
        if (!FrameworkData.AllowPause || !InputUtilities.Press[0].Start) return;
        _sceneTree.Paused = FrameworkData.IsPaused = !FrameworkData.IsPaused;
        AudioPlayer.SetPauseState(FrameworkData.IsPaused);
    }
}
