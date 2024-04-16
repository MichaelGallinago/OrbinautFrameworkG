using System;
using System.Linq;
using Godot;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Player;

namespace OrbinautFramework3.Framework.View;

public partial class Camera : Camera2D
{
	private const byte CentreOffset = 16;
	private const byte MaxViewTime = 120;
	private const byte SpeedCap = 16;
	
	public Vector2I ActiveRegion
	{
		get => _activeRegion;
		private set
		{
			IsActiveRegionChanged = true;
			_activeRegion = value;
		}
	}
	private Vector2I _activeRegion;
	public bool IsActiveRegionChanged { get; private set; }
    
	[Export] public BaseObject Target
	{
		get => _target;
		set
		{
			_target = value;
			_viewTimer = MaxViewTime;
		}
	}
	private BaseObject _target;

	public Vector4I Bounds;
		
	private Vector2 _rawPosition;
	private Vector4 _previousLimit; // TODO: check if needed
	private Vector2I _bufferOffset;
	private Vector2I _maxSpeed;
	private Vector2 _speed;
	public Vector2I BufferPosition;
	public Vector2 Delay;
	public Vector2I BoundSpeed;
	public Vector4 Bound;
	public Vector4 Limit;

	private Vector2I _shakeOffset;
	private float _shakeTimer;
	private float _viewTimer;

	public Camera()
	{
		Bound = new Vector4I(LimitTop, LimitLeft, LimitBottom, LimitRight);
		Limit = Bound;
		_previousLimit = Bound;

		int maxSpeed = SharedData.NoCameraCap ? ushort.MaxValue : SpeedCap;
		_maxSpeed = new Vector2I(maxSpeed, maxSpeed);
		
		if (SharedData.CheckpointData is not null)
		{
			LimitBottom = SharedData.CheckpointData.BottomCameraBound;
		}
	}

	public override void _Ready()
	{
		if (Target != null || PlayerData.Players.Count == 0) return;
		Player playerTarget = PlayerData.Players.First();
		Target = playerTarget;
		BufferPosition = (Vector2I)playerTarget.Position - SharedData.ViewSize;
		BufferPosition.Y += 16;
		
		_rawPosition = BufferPosition;
	}
	
	public override void _Process(double delta)
	{
		IsActiveRegionChanged = false;
		
		var boundSpeed = 0f;
		
		if (FrameworkData.UpdateObjects)
		{
			float processSpeed = FrameworkData.ProcessSpeed;
			
			// Get boundary update speed
			boundSpeed = Math.Max(2, BoundSpeed.X) * processSpeed;
			
			FollowTarget(processSpeed);
		}
		
		// Update boundaries
		Vector2I farBounds = BufferPosition + SharedData.ViewSize;
		Limit = new Vector4(
			MoveBoundaryForward(Limit.X, Bound.X, BufferPosition.X, boundSpeed), // Left
			MoveBoundaryForward(Limit.Y, Bound.Y, BufferPosition.Y, boundSpeed), // Top
			MoveBoundaryBackward(Limit.Z, Bound.Z, farBounds.X, boundSpeed), // Right
			MoveBoundaryBackward(Limit.W, Bound.W, farBounds.Y, boundSpeed) // Bottom
		);
		
		_previousLimit = Limit;

		BufferPosition = _shakeOffset + (Vector2I)(_rawPosition + _bufferOffset).Clamp(
			new Vector2(Limit.X, Limit.Y), new Vector2(Limit.Z, Limit.W) - SharedData.ViewSize);

		var finalPosition = new Vector2I(BufferPosition.X - Constants.RenderBuffer, BufferPosition.Y);
		
		Position = finalPosition;
		Bounds = new Vector4I(finalPosition.X, finalPosition.Y, 
			finalPosition.X + SharedData.ViewSize.X, finalPosition.Y + SharedData.ViewSize.Y);
		
		ForceUpdateScroll();
		UpdateActiveArea();
	}

	public void Delete()
	{
		/*
		var _surface = camera_get_surface(_index);
	
		var _camera = c_framework.camera;
		if _camera.view_count > 0
		{
			_camera.view_count--;
		}
	
		// Clear the camera
		camera_destroy(_view.instance);
	
		if surface_exists(_surface.instance)
		{
			surface_free(_surface.instance);
		}
	
		if surface_exists(_surface.instance_overlay)
		{
			surface_free(_surface.instance_overlay);
		}
	
		delete _view;
		delete _surface;
	
		_camera.view_array[_index] = noone;
		_camera.surface_array[_index] = noone;

		view_visible[_index] = false;
		view_camera[_index] = -1;
		view_surface_id[_index] = -1;
		*/
	}

	public void UpdateDelay(int? delayX = null, int? delayY = null)
	{
		Delay = new Vector2(delayX ?? Delay.X, delayY ?? Delay.Y);
	}

	public void UpdateShakeTimer(int shakeTimer) => _shakeTimer = shakeTimer;

	public void UpdatePlayerCamera(PlayerData player)
	{
		if (Target != player || player.IsDead) return;

		UpdateCdCamera(player);

		bool doShiftDown = player.Animation == Objects.Player.Animations.Duck;
		bool doShiftUp = player.Animation == Objects.Player.Animations.LookUp;

		if (doShiftDown || doShiftUp)
		{
			if (_viewTimer > 0f)
			{
				_viewTimer -= FrameworkData.ProcessSpeed;
			}
		}
		else if (SharedData.SpinDash || SharedData.PeelOut)
		{
			_viewTimer = MaxViewTime;
		}

		if (_viewTimer > 0f)
		{
			if (_bufferOffset.Y != 0)
			{
				_bufferOffset.Y -= 2 * Math.Sign(_bufferOffset.Y);
			}
		}
		else
		{
			if (doShiftDown && _bufferOffset.Y < 88) 	
			{
				_bufferOffset.Y += 2;
			}
			else if (doShiftUp && _bufferOffset.Y > -104)
			{
				_bufferOffset.Y -= 2;
			}
		}
	}

	public void SetShakeTimer(float shakeTimer) => _shakeTimer = shakeTimer;

	public bool CheckRectInside(Rect2 rect)
	{
		var cameraRect = new Rect2(Position, SharedData.ViewSize);
	
		return rect.End.X >= cameraRect.Position.X && rect.Position.X < cameraRect.End.X
		    && rect.End.Y >= cameraRect.Position.Y && rect.Position.Y < cameraRect.End.Y;
	}

	public bool CheckPositionInActiveRegion(Vector2 position)
	{
		const int cullBufferX = 320;
		const int cullBufferY = 288;
		
		return position.X >= _activeRegion.X && 
		       position.Y >= _activeRegion.Y && 
		       position.X < _activeRegion.X + SharedData.ViewSize.X + cullBufferX &&
		       position.Y < _activeRegion.Y + SharedData.ViewSize.Y + cullBufferY;
	}

	private void UpdateCdCamera(PlayerData player)
	{
		if (!SharedData.CdCamera) return;
		
		const int shiftSpeedX = 2;

		int shiftDirectionX = player.GroundSpeed != 0f ? Math.Sign(player.GroundSpeed) : (int)player.Facing;

		if (Math.Abs(player.GroundSpeed) < 6f && player.Action != Actions.SpinDash)
		{
			_bufferOffset.X -= shiftSpeedX * Math.Sign(_bufferOffset.X);
			return;
		}

		const int shiftDistanceX = 64;
		if (Delay.X == 0f && _bufferOffset.X != shiftDistanceX * shiftDirectionX)
		{
			_bufferOffset.X += shiftSpeedX * shiftDirectionX;
		}
	}

	private void UpdateActiveArea()
	{
		Vector2I position = (Vector2I)Position + sbyte.MinValue * Vector2I.One;
		ActiveRegion = new Vector2I(position.X & sbyte.MinValue, position.Y & sbyte.MinValue);
	}
	
	private static float MoveBoundaryForward(float limit, float bound, float position, float boundSpeed)
	{
		if (limit < bound)
		{
			return position >= bound ? bound : Math.Min(bound, Math.Max(limit, position) + boundSpeed);
		}
		
		return limit > bound ? Math.Max(bound, limit - boundSpeed) : limit;
	}
	
	private static float MoveBoundaryBackward(float limit, float bound, float position, float boundSpeed)
	{
		if (limit > bound)
		{
			return position <= bound ? bound : Math.Max(bound, Math.Min(limit, position) - boundSpeed);
		}
		
		return limit < bound ? Math.Min(bound, limit + boundSpeed) : limit;
	}

	private void FollowTarget(float processSpeed)
	{
		if (Target != null && !IsInstanceValid(Target))
		{
			Target = null;
		}

		UpdateSpeed(processSpeed);
		UpdateShakeOffset(processSpeed);
		UpdateRawPosition(processSpeed);
	}

	private void UpdateSpeed(float processSpeed)
	{
		if (Target == null)
		{
			_speed = new Vector2I();
			return;
		}
		
		Vector2I distance = (Vector2I)Target.Position - (Vector2I)_rawPosition - SharedData.ViewSize / 2;
		distance.Y += CentreOffset;

		int extraX = SharedData.CdCamera ? 0 : 8;
		
		_speed.X = CalculateSpeed(distance.X + extraX, extraX, _maxSpeed.X * processSpeed);
		
		if (Target is Player { IsGrounded: true } playerTarget)
		{	
			if (playerTarget.IsSpinning)
			{
				distance.Y -= playerTarget.RadiusNormal.Y - playerTarget.Radius.Y;
			}
				
			float limit = Math.Abs(playerTarget.GroundSpeed) < 8f ? 6f : _maxSpeed.Y * processSpeed;
			_speed.Y = Math.Clamp(distance.Y, -limit, limit);
			return;
		}

		_speed.Y = CalculateSpeed(distance.Y, 32, _maxSpeed.Y * processSpeed);
	}
	
	private void UpdateShakeOffset(float processSpeed)
	{
		if (_shakeTimer <= 0f)
		{
			_shakeOffset = new Vector2I();
			return;
		}
		
		_shakeOffset.X = CalculateShakeOffset(_shakeTimer, _shakeOffset.X);
		_shakeOffset.Y = CalculateShakeOffset(_shakeTimer, _shakeOffset.Y);
		_shakeTimer -= processSpeed;
	}

	private void UpdateRawPosition(float processSpeed)
	{
		for (var i = 0; i < 2; i++)
		{
			if (Delay[i] > 0f)
			{
				Delay[i] -= processSpeed;
				continue;
			}
			
			_rawPosition[i] += _speed[i];
		}
	}

	private static float CalculateSpeed(int difference, int threshold, float maxSpeed)
	{
		int distance = Math.Abs(difference) - threshold;
		return distance <= 0 ? 0 : Math.Clamp(distance * Math.Sign(difference), -maxSpeed, maxSpeed);
	}
	
	
	private static int CalculateShakeOffset(float shakeTimer, int shakeOffset) => shakeOffset switch
	{
		0 => (int)shakeTimer,
		< 0 => -1 - shakeOffset,
		_ => -shakeOffset
	};
}
