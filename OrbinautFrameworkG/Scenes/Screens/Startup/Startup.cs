using Godot;
using OrbinautFrameworkG.Framework;

namespace OrbinautFrameworkG.Scenes.Screens.Startup;

public partial class Startup : Panel
{
    [Export] private PackedScene _nextScene;
    [Export] private PackedScene _packedBranding;
    
    private Branding.Branding _branding;

    public override void _Ready()
    {
        if (_nextScene == null)
        {
            GetTree().Quit();
            return;
        }

        SetupBranding();
    }

    public override void _Process(double delta)
    {
        if (_branding is { IsFinished: false }) return;
        
        ConfigUtilities.Load();
        GetTree().ChangeSceneToPacked(_nextScene);
    }
    
    private void SetupBranding()
    {
        if (_packedBranding == null) return;
        AddChild(_branding = _packedBranding.Instantiate<Branding.Branding>());
    }
}
