using Godot;
using OrbinautFrameworkG.Framework.StaticStorages;

namespace OrbinautFrameworkG.Scenes.Screens.Startup;

public partial class Startup : Panel
{
    [Export] private PackedScene _nextScene;
    [Export] private PackedScene _packedBranding;
    
    private Branding.Branding _branding;
    private Color _defaultClearColor;

    public Startup()
    {
        _defaultClearColor = RenderingServer.GetDefaultClearColor();
        RenderingServer.SetDefaultClearColor(Colors.Black);
    }
    
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
        
        RenderingServer.SetDefaultClearColor(_defaultClearColor);
        GetTree().ChangeSceneToPacked(_nextScene);
    }
    
    private void SetupBranding()
    {
        if (Settings.SkipBranding || _packedBranding == null) return;
        AddChild(_branding = _packedBranding.Instantiate<Branding.Branding>());
    }
}
