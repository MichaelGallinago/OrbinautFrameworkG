using Godot;
using OrbinautFrameworkG.Framework.StaticStorages;

namespace OrbinautFrameworkG.Scenes.Screens.Branding;

public partial class Branding : Panel
{
	[Export] private Sprite2D _g;
	[Export] private Sprite2D _logo;
	[Export] private Sprite2D _orbinaut;

	private float _logoScale = 1.5f;
	private float _logoTransparency;
	private float _logoOffsetX = 16f;
	
	private float _orbinautScale = 0.5f;
	private float _orbinautTransparency;
	
	private float _gOffsetX = Settings.ViewSize.X;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}