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
			if (value != null && !Views.Local.TargetedCameras.TryAdd(value, this)) return;
			
			if (_target != null)
			{
				Views.Local.TargetedCameras.Remove(_target);
			}
			
			_target = value;
			_viewTimer = MaxViewTime;
			_isTargetPlayer = value is PlayerData;
			
			if (value == null) return;
			
			DrawPosition = (Vector2I)_target.Position - SharedData.ViewSize + 16 * Vector2I.Down;
			_rawPosition = PreviousPosition = DrawPosition;
		}
	}
	private BaseObject _target;
	private bool _isTargetPlayer;

	public Vector4I Bounds;
	
	public Vector2I DrawPosition { get; private set; }
	public Vector2I PreviousPosition { get; private set; }
	public int BoundSpeed { get; set; }
	public Vector4 TargetBoundary { get; set; }
	public Vector4 Boundary { get; private set; }
	public bool IsMovementAllowed { get; set; } = true;

	private Vector2 _delay;
	private Vector2I _shakeOffset;
	private float _shakeTimer;
	private float _viewTimer;
	private Vector2 _rawPosition;
	private Vector4 _previousLimit; // TODO: check if needed
	private Vector2 _bufferOffset;
	private Vector2I _maxVelocity;
	private Vector2 _velocity;

	public Camera()
	{
		TargetBoundary = new Vector4I(LimitTop, LimitLeft, LimitBottom, LimitRight);
		Boundary = TargetBoundary;
		
		_previousLimit = TargetBoundary;

		int maxSpeed = SharedData.NoCameraCap ? ushort.MaxValue : SpeedCap;
		_maxVelocity = new Vector2I(maxSpeed, maxSpeed);
		
		if (SharedData.CheckpointData is not null)
		{
			LimitBottom = SharedData.CheckpointData.BottomCameraBound;
		}
	}

	public Camera(Vector2I maxVelocity)
	{
		_maxVelocity = maxVelocity;
	}

	public override void _Process(double delta)
	{
		IsActiveRegionChanged = false;
		
		MoveCamera();

		_previousLimit = Boundary;

		PreviousPosition = DrawPosition;
		DrawPosition = _shakeOffset + ((Vector2I)_rawPosition + (Vector2I)_bufferOffset).Clamp(
			new Vector2I((int)Boundary.X, (int)Boundary.Y), 
			new Vector2I((int)Boundary.Z, (int)Boundary.W) - SharedData.ViewSize);

		Views.Local.UpdateBottomCamera(this);
		
		Position = new Vector2(DrawPosition.X - Constants.RenderBuffer, DrawPosition.Y);
		
		ForceUpdateScroll();
		UpdateActiveRegion();
	}

	public void SetCameraDelayX(float delay) => _delay.X = delay;
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
	
	private void FollowPlayer(float processSpeed, Vector2 targetPosition, PlayerData player)
	{
		if (Scene.Local.State == Scene.States.Paused && player.DeathState == DeathStates.Wait || player.IsDead) return;
		
		FollowPlayerY(processSpeed, targetPosition.Y, player);
		if (SharedData.CdCamera)
		{
			FollowTargetCdX(processSpeed, targetPosition.X);
			UpdateCdCamera(player, processSpeed);
		}
		else
		{
			FollowTargetX(processSpeed, targetPosition.X);
		}

		bool doShiftDown = player.Animation == Objects.Player.Animations.Duck;
		bool doShiftUp = player.Animation == Objects.Player.Animations.LookUp;

		if (doShiftDown || doShiftUp)
		{
			if (_viewTimer > 0f)
			{
				_viewTimer -= processSpeed;
			}
		}
		else if (SharedData.SpinDash || SharedData.Dash)
		{
			_viewTimer = MaxViewTime;
		}

		float offsetSpeed = 2f * processSpeed;
		
		if (UpdateBufferOffset(offsetSpeed)) return;
		
		if (doShiftDown && _bufferOffset.Y < 88f) 	
		{
			_bufferOffset.Y += offsetSpeed;
		}
		else if (doShiftUp && _bufferOffset.Y > -104f)
		{
			_bufferOffset.Y -= offsetSpeed;
		}
	}

	private bool UpdateBufferOffset(float offsetSpeed)
	{
		if (_viewTimer <= 0f) return false;
		if (_bufferOffset.Y == 0f) return true;
		
		_bufferOffset.Y = _bufferOffset.Y.MoveToward(0f, offsetSpeed);
		return true;
	}
	
	private void UpdateCdCamera(PlayerData player, float processSpeed)
	{
		const int shiftSpeedX = 2;
		
		if (Math.Abs(player.GroundSpeed) < 6f && player.Action != Actions.SpinDash)
		{
			_bufferOffset.X = _bufferOffset.X.MoveToward(0f, shiftSpeedX * processSpeed);
			return;
		}
		
		if (_delay.X > 0f) return;
		
		const int shiftDistanceX = 64;
		int shiftSign = player.GroundSpeed != 0f ? Math.Sign(player.GroundSpeed) : (int)player.Facing;
		_bufferOffset.X = _bufferOffset.X.MoveToward(shiftDistanceX * shiftSign, shiftSpeedX * processSpeed);
	}
	
	private void MoveCamera()
	{
		if (!IsMovementAllowed || Scene.Local.State == Scene.States.Paused) return;
		
		float processSpeed = Scene.Local.ProcessSpeed;

		FollowTarget(processSpeed);
		UpdateShakeOffset(processSpeed);
		
		// Update boundaries
		float boundSpeed = Math.Max(2, BoundSpeed) * processSpeed;
		Vector2I farBounds = DrawPosition + SharedData.ViewSize;
		Boundary = new Vector4(
			MoveBoundaryForward(Boundary.X, TargetBoundary.X, DrawPosition.X, boundSpeed), // Left
			MoveBoundaryForward(Boundary.Y, TargetBoundary.Y, DrawPosition.Y, boundSpeed), // Top
			MoveBoundaryBackward(Boundary.Z, TargetBoundary.Z, farBounds.X, boundSpeed), // Right
			MoveBoundaryBackward(Boundary.W, TargetBoundary.W, farBounds.Y, boundSpeed) // Bottom
		);
	}

	private void FollowTarget(float processSpeed)
	{
		if (!IsInstanceValid(Target))
		{
			Target = null;
			return;
		}
		
		Vector2 targetPosition = _target.Position - SharedData.ViewSize / 2;
		if (_isTargetPlayer)
		{
			FollowPlayer(processSpeed, targetPosition, Target as PlayerData);
			return;
		}
		
		FollowTargetX(processSpeed, targetPosition.X);
		FollowTargetY(processSpeed, targetPosition.Y);
	}
		
	private void FollowTargetX(float processSpeed, float targetPosition)
	{
		if (_delay.X > 0f)
		{
			_delay.X -= processSpeed;
			return;
		}
		
		const float freeSpaceX = 16f;
		float distance = targetPosition - _rawPosition.X;
		float speed = _maxVelocity.X * processSpeed;
		
		_rawPosition.X = distance switch
		{
			> 0 => distance <= speed ? 
				targetPosition : _rawPosition.X + speed,
			< -freeSpaceX => distance + freeSpaceX >= -speed ? 
				targetPosition + freeSpaceX : _rawPosition.X - speed,
			_ => _rawPosition.X
		};
	}
	
	private void FollowTargetCdX(float processSpeed, float targetPosition)
	{
		if (_delay.X > 0f)
		{
			_delay.X -= processSpeed;
			return;
		}
		
		float distance = targetPosition - _rawPosition.X;
		float speed = _maxVelocity.X * processSpeed;
		
		if (distance == 0f) return;
		
		_rawPosition.X = Math.Abs(distance) > speed ? _rawPosition.X + speed * Math.Sign(distance) : targetPosition;
	}
	
	private void FollowTargetY(float processSpeed, float targetPosition)
	{
		if (_delay.Y > 0f)
		{
			_delay.Y -= processSpeed;
			return;
		}
		
		const float freeSpaceY = 32f;
		const float offsetY = 16f;
		float distance = targetPosition - _rawPosition.Y + offsetY;
		float speed = _maxVelocity.Y * processSpeed;
		
		_rawPosition.Y = distance switch
		{
			> freeSpaceY => distance <= speed + freeSpaceY ? 
				targetPosition + (offsetY - freeSpaceY) : _rawPosition.Y + speed,
			< -freeSpaceY => -distance <= speed + freeSpaceY ? 
				targetPosition + (offsetY + freeSpaceY) : _rawPosition.Y - speed,
			_ => _rawPosition.Y
		};
	}
	
	private void FollowPlayerY(float processSpeed, float targetPosition, PlayerData player)
	{
		if (_delay.Y > 0f)
		{
			_delay.Y -= processSpeed;
			return;
		}
		
		float distance = targetPosition - _rawPosition.Y;

		if (player.IsGrounded)
		{
			if (player.IsSpinning)
			{
				int offset = player.RadiusNormal.Y - player.Radius.Y;
				distance -= offset;
				targetPosition -= offset;
			}

			float limit = processSpeed * (Math.Abs(player.GroundSpeed) < 8f ? 6f : _maxVelocity.Y);
			
			_rawPosition.Y = distance <= limit && distance >= -limit ? 
				targetPosition : _rawPosition.Y + limit * MathF.Sign(distance);
			
			return;
		}
		
		const float freeSpaceY = 32f;
		const float offsetY = 16f;
		
		distance += offsetY;
		float speed = _maxVelocity.Y * processSpeed;
		_rawPosition.Y = distance switch
		{
			> freeSpaceY => distance <= speed + freeSpaceY ? 
				targetPosition + (offsetY - freeSpaceY) : _rawPosition.Y + speed,
			< -freeSpaceY => -distance <= speed + freeSpaceY ? 
				targetPosition + (offsetY + freeSpaceY) : _rawPosition.Y - speed,
			_ => _rawPosition.Y
		};
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
	
	private static int CalculateShakeOffset(float shakeTimer, int shakeOffset) => shakeOffset switch
	{
		0 => (int)shakeTimer,
		< 0 => -1 - shakeOffset,
		_ => -shakeOffset
	};

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
}
