using Godot;
using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework.InputModule;
using OrbinautFrameworkG.Framework.MathUtilities;
using OrbinautFrameworkG.Framework.StaticStorages;

namespace OrbinautFrameworkG.Scenes.Screens.Branding;

public partial class Branding : Panel
{
	[Export] private Sprite2D _g;
	[Export] private Sprite2D _logo;
	[Export] private Sprite2D _orbinaut;
	
	public bool IsFinished { get; private set; }

	private float _time;
	
	private float _logoScale = 1.5f;
	private float _logoTransparency;
	private float _logoOffsetX = 16f;
	
	private float _orbinautScale = 1.5f;
	private float _orbinautTransparency;
	
	private float _gOffsetX = Settings.ViewSize.X;
	
	public override void _Ready()
	{
		//fade_perform_black(FADEROUTINE.IN, 1); TODO: fade
	}
	
	public override void _Process(double delta)
	{
		InputUtilities.Update(); //TODO: move to preloaded object
		
		float speed = DeltaTimeUtilities.CalculateSpeed(delta);
		
		float previousTime = _time;
		_time += speed;
		
		_orbinautTransparency = _orbinautTransparency.MoveToward(1f, 0.05f * speed);
		_orbinautScale = _orbinautScale.MoveToward(1f, (_orbinautScale - 1f) / 8f * speed);

		if (_time >= 8f)
		{
			_logoTransparency = _logoTransparency.MoveToward(1f, 0.05f * speed);
			_logoScale = _logoScale.MoveToward(1f, (_logoScale - 1f) / 8f * speed);
		}

		if (_time >= 11f && previousTime < 11f)
		{
			AudioPlayer.Sound.Play(SoundStorage.Branding);
		}
		
		if (_time >= 96f && previousTime < 96f || InputUtilities.Press[0].Start)
		{
			//fade_perform_black(FADEROUTINE.OUT, 1); TODO: fade
		}
	}
}