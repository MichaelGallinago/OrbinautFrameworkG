using System;
using Godot;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Player;

namespace OrbinautFramework3.Framework.View;

public partial class Camera : Camera2D, ICamera
{
	private static readonly Vector2I CullBuffer = new(320, 288);
	
	private const byte MaxViewTime = 120;
	private const byte SpeedCap = 16;
	
	public Rect2I ActiveRegion
	{
		get => _activeRegion;
		private set
		{
			IsActiveRegionChanged = true;
			_activeRegion = value;
		}
	}
	private Rect2I _activeRegion;
	public bool IsActiveRegionChanged { get; private set; }
    
	[Export] public BaseObject Target
	{
		get => _target;
		set
		{
			if (!Views.Local.TargetedCameras.TryAdd(value, this)) return;
			
			if (_target != null)
			{
				Views.Local.TargetedCameras.Remove(_target);
			}
				
			_target = value;
			_viewTimer = MaxViewTime;
				
			DrawPosition = (Vector2I)_target.Position - SharedData.ViewSize + 16 * Vector2I.Down;
			_rawPosition = PreviousPosition = DrawPosition;
		}
	}
	private BaseObject _target;

	public Vector4I Bounds;
	
	public Vector2I DrawPosition { get; private set; }
	public Vector2I PreviousPosition { get; private set; }
	public Vector2 Delay { get; set; }
	public int BoundSpeed { get; set; }
	public Vector4 TargetBoundary { get; set; }
	public Vector4 Boundary { get; private set; }
	public bool IsMovementAllowed { get; set; } = true;

	private Vector2I _shakeOffset;
	private float _shakeTimer;
	private float _viewTimer;
	private Vector2 _rawPosition;
	private Vector4 _previousLimit; // TODO: check if needed
	private Vector2I _bufferOffset;
	public Vector2I MaxVelocity { get; private set; }
	private Vector2 _velocity;

	public Camera()
	{
		TargetBoundary = new Vector4I(LimitTop, LimitLeft, LimitBottom, LimitRight);
		Boundary = TargetBoundary;
		
		_previousLimit = TargetBoundary;

		int maxSpeed = SharedData.NoCameraCap ? ushort.MaxValue : SpeedCap;
		MaxVelocity = new Vector2I(maxSpeed, maxSpeed);
		
		if (SharedData.CheckpointData is not null)
		{
			LimitBottom = SharedData.CheckpointData.BottomCameraBound;
		}
	}

	public override void _Process(double delta)
	{
		IsActiveRegionChanged = false;
		
		MoveCamera();

		_previousLimit = Boundary;

		PreviousPosition = DrawPosition;
		DrawPosition = _shakeOffset + ((Vector2I)_rawPosition + _bufferOffset).Clamp(
			new Vector2I((int)Boundary.X, (int)Boundary.Y), 
			new Vector2I((int)Boundary.Z, (int)Boundary.W) - SharedData.ViewSize);

		Views.Local.UpdateBottomCamera(this);
		
		Position = new Vector2(DrawPosition.X - Constants.RenderBuffer, DrawPosition.Y);
		
		ForceUpdateScroll();
		UpdateActiveRegion();
	}

	public void UpdateShakeTimer(int shakeTimer) => _shakeTimer = shakeTimer;
	
	public void SetShakeTimer(float shakeTimer) => _shakeTimer = shakeTimer;

	public bool CheckRectInside(Rect2 rect)
	{
		var cameraRect = new Rect2(Position, SharedData.ViewSize);
	
		return rect.End.X >= cameraRect.Position.X && rect.Position.X < cameraRect.End.X
		    && rect.End.Y >= cameraRect.Position.Y && rect.Position.Y < cameraRect.End.Y;
	}

	public bool CheckPositionInSafeRegion(Vector2I position)
	{
		int distanceY = position.Y - DrawPosition.Y + 128;
		position.X -= ActiveRegion.Position.X;

		return position.X >= 0 && position.X < ActiveRegion.Size.X && 
		       distanceY  >= 0 && distanceY  < ActiveRegion.Size.Y && 
		       position.Y < Boundary.W;
	}

	public bool CheckPositionInActiveRegion(Vector2I position) => ActiveRegion.HasPoint(position);
	
	public bool CheckXInActiveRegion(int position)
	{
		return position >= ActiveRegion.Position.X && position < ActiveRegion.Position.X + ActiveRegion.Size.X;
	}

	public bool CheckYInActiveRegion(int position)
	{
		return position >= ActiveRegion.Position.Y && position < ActiveRegion.Position.Y + ActiveRegion.Size.Y;
	}
	
	public void UpdatePlayerCamera(PlayerData player)
	{
		if (Target != player || Scene.Local.IsPaused && player.DeathState == DeathStates.Wait || player.IsDead) return;
		
		UpdateCdCamera(player);

		bool doShiftDown = player.Animation == Objects.Player.Animations.Duck;
		bool doShiftUp = player.Animation == Objects.Player.Animations.LookUp;

		if (doShiftDown || doShiftUp)
		{
			if (_viewTimer > 0f)
			{
				_viewTimer -= Scene.Local.ProcessSpeed;
			}
		}
		else if (SharedData.SpinDash || SharedData.Dash)
		{
			_viewTimer = MaxViewTime;
		}

		if (_viewTimer > 0f)
		{
			if (_bufferOffset.Y != 0)
			{
				_bufferOffset.Y -= 2 * Math.Sign(_bufferOffset.Y);
			}
			
			return;
		}
		
		if (doShiftDown && _bufferOffset.Y < 88) 	
		{
			_bufferOffset.Y += 2;
		}
		else if (doShiftUp && _bufferOffset.Y > -104)
		{
			_bufferOffset.Y -= 2;
		}
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
	
	private void MoveCamera()
	{
		if (!IsMovementAllowed || Scene.Local.IsPaused) return;
		
		float processSpeed = Scene.Local.ProcessSpeed;

		// Get boundary update speed
		float boundSpeed = Math.Max(2, BoundSpeed) * processSpeed;
		
		FollowTarget(processSpeed);
		UpdateShakeOffset(processSpeed);
		
		// Update boundaries
		Vector2I farBounds = DrawPosition + SharedData.ViewSize;
		Boundary = new Vector4(
			MoveBoundaryForward(Boundary.X, TargetBoundary.X, DrawPosition.X, boundSpeed), // Left
			MoveBoundaryForward(Boundary.Y, TargetBoundary.Y, DrawPosition.Y, boundSpeed), // Top
			MoveBoundaryBackward(Boundary.Z, TargetBoundary.Z, farBounds.X, boundSpeed), // Right
			MoveBoundaryBackward(Boundary.W, TargetBoundary.W, farBounds.Y, boundSpeed) // Bottom
		);
	}

	private void UpdateActiveRegion()
	{
		Vector2I position = DrawPosition + sbyte.MinValue * Vector2I.One;
		position.X &= sbyte.MinValue;
		position.Y &= sbyte.MinValue;
		ActiveRegion = new Rect2I(position, SharedData.ViewSize + CullBuffer);
	}
	
	private static float MoveBoundaryForward(float boundary, float target, int position, float speed)
	{
		if (boundary < target)
		{
			return position >= target ? target : Math.Min(target, Math.Max(boundary, position) + speed);
		}
		
		return boundary > target ? Math.Max(target, boundary - speed) : boundary;
	}
	
	private static float MoveBoundaryBackward(float boundary, float target, int position, float speed)
	{
		if (boundary > target)
		{
			return position <= target ? target : Math.Max(target, Math.Min(boundary, position) - speed);
		}
		
		return boundary < target ? Math.Min(target, boundary + speed) : boundary;
	}

	private void FollowTarget(float processSpeed)
	{
		if (!IsInstanceValid(Target))
		{
			Target = null;
			return;
		}
		
		const int freeSpaceX = 16;
		const int freeSpaceY = 32;
		const float offsetY = 16f;
			
		Vector2I targetPosition = (Vector2I)_target.Position - SharedData.ViewSize / 2;
		Vector2 distance = targetPosition - _rawPosition;
		distance.Y += offsetY;
		
		Vector2 delay = Delay;
		if (delay.X > 0f)
		{
			delay.X -= processSpeed;
		}
		else
		{
			float speed = MaxVelocity.X * processSpeed;
			_rawPosition.X = distance.X switch
			{
				> 0 => distance.X < speed ? targetPosition.X : _rawPosition.X + speed,
				< -freeSpaceX => distance.X + freeSpaceX >= -speed ? 
					targetPosition.X + freeSpaceX : _rawPosition.X - speed,
				_ => _rawPosition.X
			};
		}

		if (delay.Y > 0f)
		{
			delay.Y -= processSpeed;
		}
		else
		{
			float speed = MaxVelocity.Y * processSpeed;
			_rawPosition.Y = distance.Y switch
			{
				> freeSpaceY => distance.Y - freeSpaceY < speed ? 
					targetPosition.Y - freeSpaceY + offsetY : _rawPosition.Y + speed,
				< -freeSpaceY => distance.Y + freeSpaceY >= -speed ? 
					targetPosition.Y + freeSpaceY + offsetY : _rawPosition.Y - speed,
				_ => _rawPosition.Y
			};
		}
		
		Delay = delay;
	}
	
	private void UpdateShakeOffset(float processSpeed)
	{
		if (_shakeTimer <= 0f)
		{
			_shakeOffset = Vector2I.Zero;
			return;
		}
		
		_shakeOffset.X = CalculateShakeOffset(_shakeTimer, _shakeOffset.X);
		_shakeOffset.Y = CalculateShakeOffset(_shakeTimer, _shakeOffset.Y);
		_shakeTimer -= processSpeed;
	}

	private void UpdateRawPosition(float processSpeed)
	{
		Vector2 delay = Delay;
		for (var i = 0; i < 2; i++)
		{
			if (delay[i] > 0f)
			{
				delay[i] -= processSpeed;
				continue;
			}
			
			_rawPosition[i] += _velocity[i] * processSpeed;
		}

		Delay = delay;
	}
	
	private static int CalculateShakeOffset(float shakeTimer, int shakeOffset) => shakeOffset switch
	{
		0 => (int)shakeTimer,
		< 0 => -1 - shakeOffset,
		_ => -shakeOffset
	};
}
