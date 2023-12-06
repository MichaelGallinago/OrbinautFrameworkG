using System.Linq;
using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3;

public partial class Camera : Camera2D
{
	private const byte CameraCentreOffset = 16;
	public const float Tolerance = 0.000001f;
	
	private static readonly int[] ShakeData = {
		1, 2, 1, 3, 1, 2, 2, 1, 2, 3, 1, 2, 1, 2, 0, 0,
		2, 0, 3, 2, 2, 3, 2, 2, 1, 3, 0, 0, 1, 0, 1, 3
	};

	public static Camera MainCamera { get; set; }
    
	[Export] public Framework.CommonObject.CommonObject Target { get; set; }
    
	public Vector4I Bounds { get; private set; }
		
	private Vector2I _maxSpeed;
	private Vector2I _speed;
	private Vector2I _position;
	private Vector2 _rawPosition;
	private Vector2I _delay;
	private Vector2I _offset;
	private Vector2I _boundSpeed;
	private Vector4 _bound;
	private Vector4 _limit;
	private Vector4 _previousLimit;

	private Vector2I _shakeOffset;
	private int _shakeTimer;

	public Camera()
	{
		_bound = new Vector4I(LimitTop, LimitLeft, LimitBottom, LimitRight);
		_limit = _bound;
		_previousLimit = _bound;
		_maxSpeed = new Vector2I(16, 16);

		if (FrameworkData.CheckpointData is not null)
		{
			LimitBottom = FrameworkData.CheckpointData.BottomCameraBound;
		}
	}

	public override void _Ready()
	{
		if (Target != null || Objects.Player.Player.Players.Count == 0) return;
		Objects.Player.Player playerTarget = Objects.Player.Player.Players.First();
		Target = playerTarget;
		_position = (Vector2I)playerTarget.Position - FrameworkData.ViewSize;
		_position.Y += 16;
		
		_rawPosition = _position;
	}

	public override void _EnterTree()
	{
		FrameworkData.CurrentScene.LateUpdate += EndStep;
		MainCamera ??= this;
	}

	public override void _ExitTree()
	{
		FrameworkData.CurrentScene.LateUpdate -= EndStep;
		if (MainCamera == this)
		{
			MainCamera = null;
		}
	}

	private void EndStep(double processSpeed)
	{
		if (MainCamera != this) return;
		var processSpeedF = (float)processSpeed;
		var boundSpeed = 0;
		
		if (FrameworkData.UpdateObjects)
		{
			Vector2I centre = FrameworkData.ViewSize / 2;
			centre.Y -= CameraCentreOffset;

			// Get boundary update speed
			boundSpeed = Mathf.Max(2, _boundSpeed.X);

			if (Target != null && !IsInstanceValid(Target))
			{
				Target = null;
			}
			
			if (Target != null)
			{
				Vector2I targetPosition = (Vector2I)Target.Position - (Vector2I)_rawPosition;

				int extX = FrameworkData.CDCamera ? 0 : 16;
				
				if (targetPosition.X > centre.X)
				{
					_speed.X = Mathf.Clamp(targetPosition.X - centre.X, 0, _maxSpeed.X);    
				}
				else if (targetPosition.X < centre.X - extX)
				{ 
					_speed.X = Mathf.Clamp(targetPosition.X - centre.X + extX, -_maxSpeed.X, 0);  
				}
				else
				{
					_speed.X = 0;
				}
				
				if (Target is Objects.Player.Player { IsGrounded: true } playerTarget)
				{	
					if (playerTarget.IsSpinning)
					{
						targetPosition.Y -= playerTarget.RadiusNormal.Y - playerTarget.InteractData.Radius.Y;
					}
					
					int limit = Mathf.Abs(playerTarget.GroundSpeed) < 8 ? 6 : _maxSpeed.Y;
					_speed.Y = Mathf.Clamp(targetPosition.Y - centre.Y, -limit, limit);
				} 
				else
				{
					if (targetPosition.Y > centre.Y + 32)
					{
						_speed.Y = Mathf.Clamp(targetPosition.Y - centre.Y - 32, 0, _maxSpeed.Y);  
					}
					else if (targetPosition.Y < centre.Y - 32)
					{ 
						_speed.Y = Mathf.Clamp(targetPosition.Y - centre.Y + 32, -_maxSpeed.Y, 0);  
					} 
					else
					{
						_speed.Y = 0;
					}
				}
			}
			else
			{
				_speed.X = 0;
				_speed.Y = 0;
			}
		
			if (_shakeTimer > 0)
			{
				_shakeOffset.X = _shakeOffset.X switch
				{
					0 => _shakeTimer,
					< 0 => -1 - _shakeOffset.X,
					_ => -_shakeOffset.X
				};

				_shakeOffset.Y = _shakeOffset.Y switch
				{
					0 => _shakeTimer,
					< 0 => -1 - _shakeOffset.Y,
					_ => -_shakeOffset.Y
				};

				_shakeTimer--;
			}
			else
			{
				_shakeOffset = new Vector2I();
			}
			
			switch (_delay.X)
			{
				case 0:
					_rawPosition.X += _speed.X * processSpeedF;
					break;
				case > 0:
					_delay.X--;
					break;
			}
			
			switch (_delay.Y)
			{
				case 0:
					_rawPosition.Y += _speed.Y * processSpeedF;
					break;
				case > 0:
					_delay.Y--;
					break;
			}
		}
		
		// Update left boundary
		if (Mathf.Abs(_previousLimit.X - _limit.X) > Tolerance)
		{
			_bound.X = _limit.X;
		}
		else if (_limit.X < _bound.X)
		{	
			if (_position.X >= _bound.X)
			{
				_limit.X = _bound.X;
			}
			else
			{
				if (_position.X >= _limit.X)
				{
					_limit.X = Mathf.RoundToInt(_position.X);
				}
				
				_limit.X = Mathf.Min(_limit.X + boundSpeed * processSpeedF, _bound.X);
			}
		}
		else if (_limit.X > _bound.X)
		{
			_limit.X = Mathf.Max(_bound.X, _limit.X - boundSpeed * processSpeedF);
		}
	
		// Update right boundary
		if (Mathf.Abs(_previousLimit.Z - _limit.Z) > Tolerance)
		{
			_bound.Z = _limit.Z;
		}
		else if (_limit.Z < _bound.Z)
		{
			_limit.Z = Mathf.Min(_limit.Z + boundSpeed * processSpeedF, _bound.Z);
		}
		else if (_limit.Z > _bound.Z)
		{
			int width = FrameworkData.ViewSize.X;
			
			if (_position.X + width <= _bound.Z)
			{
				_limit.Z = _bound.Z;
			}
			else
			{	
				if (_position.X + width <= _limit.Z)
				{
					_limit.Z = Mathf.RoundToInt(_position.X + width);	
				}
				
				_limit.Z = Mathf.Max(_bound.Z, _limit.Z - boundSpeed * processSpeedF);
			}
		}
	
		// Update top boundary
		if (Mathf.Abs(_previousLimit.Y - _limit.Y) > Tolerance)
		{
			_bound.Y = _limit.Y;
		}
		else if (_limit.Y < _bound.Y)
		{
			if (_position.Y >= _bound.Y)
			{
				_limit.Y = _bound.Y;
			}
			else
			{	
				if (_position.Y >= _limit.Y)
				{
					_limit.Y = Mathf.RoundToInt(_position.Y);
				}
				
				_limit.Y = Mathf.Min(_limit.Y + boundSpeed * processSpeedF, _bound.Y);
			}
		}
		else if (_limit.Y > _bound.Y)
		{
			_limit.Y = Mathf.Max(_bound.Y, _limit.Y - boundSpeed * processSpeedF);
		}
		
		// Update bottom boundary
		if (Mathf.Abs(_previousLimit.W - _limit.W) > Tolerance)
		{
			_bound.W = _limit.W;
		}
		else if (_limit.W < _bound.W)
		{
			_limit.W = Mathf.Min(_limit.W + boundSpeed * processSpeedF, _bound.W);
		}
		else if (_limit.W > _bound.W)
		{
			int height = FrameworkData.ViewSize.Y;
			
			if (_position.Y + height <= _bound.W)
			{
				_limit.W = _bound.W;
			}
			else
			{
				if (_position.Y + height <= _limit.W)
				{
					_limit.W = Mathf.RoundToInt(_position.Y + height);
				}
				
				_limit.W = Mathf.Max(_bound.W, _limit.W - boundSpeed * processSpeedF);
			}
		}

		_previousLimit = _limit;
		
		_position.X = (int)Mathf.Clamp(_rawPosition.X + _offset.X, 
			_limit.X, _limit.Z - FrameworkData.ViewSize.X);
		_position.Y = (int)Mathf.Clamp(_rawPosition.Y + _offset.Y, 
			_limit.Y, _limit.W - FrameworkData.ViewSize.Y);
		_position += _shakeOffset;

		var finalPosition = new Vector2I(_position.X - Constants.RenderBuffer, _position.Y);
		
		Position = finalPosition;
		Bounds = new Vector4I(finalPosition.X, finalPosition.Y, 
			finalPosition.X + FrameworkData.ViewSize.X, finalPosition.Y + FrameworkData.ViewSize.Y);
	}
}