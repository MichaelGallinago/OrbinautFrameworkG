using System;
using System.Linq;
using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3;

public partial class Camera : Camera2D
{
	private const byte CameraCentreOffset = 16;
	
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
	private Vector4 _previousLimit; // TODO: check if needed

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
		var boundSpeed = 0f;

		if (FrameworkData.UpdateObjects)
		{
			var processSpeedF = (float)processSpeed;
			
			// Get boundary update speed
			boundSpeed = Math.Max(2, _boundSpeed.X) * processSpeedF;
			
			FollowTarget(processSpeedF);
		}
		
		// Update boundaries
		Vector2I farBounds = _position + FrameworkData.ViewSize;
		_limit.X = MoveBoundaryForward(_limit.X, _bound.X, _position.X, boundSpeed); // Left
		_limit.Y = MoveBoundaryForward(_limit.Y, _bound.Y, _position.Y, boundSpeed); // Top
		_limit.Z = MoveBoundaryBackward(_limit.Z, _bound.Z, farBounds.X, boundSpeed); // Right
		_limit.W = MoveBoundaryBackward(_limit.W, _bound.W, farBounds.Y, boundSpeed); // Bottom

		_previousLimit = _limit;
		
		_position.X = (int)Math.Clamp(_rawPosition.X + _offset.X, _limit.X, _limit.Z - FrameworkData.ViewSize.X);
		_position.Y = (int)Math.Clamp(_rawPosition.Y + _offset.Y, _limit.Y, _limit.W - FrameworkData.ViewSize.Y);
		_position += _shakeOffset;

		var finalPosition = new Vector2I(_position.X - Constants.RenderBuffer, _position.Y);
		
		Position = finalPosition;
		Bounds = new Vector4I(finalPosition.X, finalPosition.Y, 
			finalPosition.X + FrameworkData.ViewSize.X, finalPosition.Y + FrameworkData.ViewSize.Y);
	}

	private static float MoveBoundaryForward(float limit, float bound, float position, float boundSpeed)
	{
		if (limit < bound)
		{
			return position >= bound ? bound : Math.Min(Math.Max(limit, position) + boundSpeed, bound);
		}
		
		return limit > bound ? Math.Max(bound, limit - boundSpeed) : limit;
	}
	
	private static float MoveBoundaryBackward(float limit, float bound, float position, float boundSpeed)
	{
		if (limit > bound)
		{
			return position <= bound ? bound : Math.Max(bound, Math.Min(limit, position) - boundSpeed);
		}
		
		return limit < bound ? Math.Min(limit + boundSpeed, bound) : limit;
	}

	private void FollowTarget(float processSpeedF)
	{
		Vector2I centre = FrameworkData.ViewSize / 2;
		centre.Y -= CameraCentreOffset;

		if (Target != null && !IsInstanceValid(Target))
		{
			Target = null;
		}
		
		if (Target != null)
		{
			Vector2I targetPosition = (Vector2I)Target.Position - (Vector2I)_rawPosition;

			int extX = FrameworkData.CDCamera ? 0 : 8;
			
			_speed.X = UpdateSpeed(targetPosition.X - centre.X + extX, extX, _maxSpeed.X);
			
			if (Target is Objects.Player.Player { IsGrounded: true } playerTarget)
			{	
				if (playerTarget.IsSpinning)
				{
					targetPosition.Y -= playerTarget.RadiusNormal.Y - playerTarget.InteractData.Radius.Y;
				}
				
				int limit = Math.Abs(playerTarget.GroundSpeed) < 8 ? 6 : _maxSpeed.Y;
				_speed.Y = Math.Clamp(targetPosition.Y - centre.Y, -limit, limit);
			} 
			else
			{
				_speed.Y = UpdateSpeed(targetPosition.Y - centre.Y, 32, _maxSpeed.Y);
			}
		}
		else
		{
			_speed = new Vector2I();
		}
	
		if (_shakeTimer > 0)
		{
			_shakeOffset.X = UpdateShakeOffset(_shakeTimer, _shakeOffset.X);
			_shakeOffset.Y = UpdateShakeOffset(_shakeTimer, _shakeOffset.Y);
			_shakeTimer--;
		}
		else
		{
			_shakeOffset = new Vector2I();
		}

		for (var i = 0; i < 2; i++)
		{
			if (_delay[i] > 0)
			{
				_delay[i]--;
				continue;
			}
			
			_rawPosition[i] += _speed[i] * processSpeedF;
		}
	}

	private static int UpdateShakeOffset(int shakeTimer, int shakeOffset)
	{
		return shakeOffset switch
		{
			0 => shakeTimer,
			< 0 => -1 - shakeOffset,
			_ => -shakeOffset
		};
	}

	private static int UpdateSpeed(int difference, int threshold, int maxSpeed)
	{
		int distance = Math.Abs(difference);
		return distance <= threshold ? 0 : 
			Math.Clamp((distance - threshold) * Math.Sign(difference), -maxSpeed, maxSpeed);
	}
}
