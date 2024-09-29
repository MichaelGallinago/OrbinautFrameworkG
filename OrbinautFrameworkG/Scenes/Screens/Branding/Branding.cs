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
		if (_time >= 120f) //(_fade.routine == FADEROUTINE.OUT && _fade.state == FADESTATE.PLAINCOLOUR) TODO: fade
		{
			IsFinished = true;
			SetProcess(false);
			return;
		}
		
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
		
		float digitStep = _gOffsetX / 4f * speed;
		if (_logoScale < 1.1f)
		{
			_gOffsetX = _gOffsetX.MoveToward(1f, digitStep);
		}
		
		if (_gOffsetX < 16f)
		{
			_logoOffsetX = _logoOffsetX.MoveToward(1f, digitStep);
		}
		
		if (_time >= 96f && previousTime < 96f || InputUtilities.Press[0].Start)
		{
			//fade_perform_black(FADEROUTINE.OUT, 1); TODO: fade
		}
		
		SetVisualData();
	}

	private void SetVisualData()
	{
		Vector2I centre = Settings.HalfViewSize;
		
		_orbinaut.Position = centre + Vector2.Up * 32f;
		_orbinaut.Scale = _orbinautScale * Vector2.One;
		_orbinaut.Modulate = _orbinaut.Modulate with { A = _orbinautTransparency };
		
		_logo.Position = new Vector2(centre.X - 24f + _logoOffsetX, centre.Y + 69f);
		_logo.Scale = _orbinautScale * Vector2.One;
		_logo.Modulate = _orbinaut.Modulate with { A = _logoTransparency };
		
		_g.Position = new Vector2(centre.X + 98f + _gOffsetX, centre.Y + 65f);
	}
}
