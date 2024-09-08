using System;
using Godot;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Player;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Framework.View;

public partial class Camera : Camera2D, ICamera
{
	private static readonly Vector2I CullingBuffer = Vector2I.Right * 64;
	
	private const byte MaxViewTime = 120;
	private const byte SpeedCap = 16;
	
	public IPosition Target
	{
		get => _target;
		set
		{
			if (value != null && !Views.Instance.TargetedCameras.TryAdd(value, this)) return;
			
			if (_target != null)
			{
				Views.Instance.TargetedCameras.Remove(_target);
			}
			
			_target = value;
			_viewTimer = MaxViewTime;
			_playerTarget = value as IPlayer;
			_godotObjectTarget = value as GodotObject;
			
			if (value == null) return;
			
			DrawPosition = (Vector2I)_target.Position - SharedData.ViewSize + 16 * Vector2I.Down;
			_rawPosition = PreviousPosition = DrawPosition;
		}
	}
	
	private IPosition _target;
	private IPlayer _playerTarget;
	private GodotObject _godotObjectTarget;
	
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
	
	public Vector2I DrawPosition { get; private set; }
	public Vector2 PreviousPosition { get; private set; }
	public int BoundSpeed { get; set; }

	public Vector4 TargetBoundary { get; set; }
	public Vector4 Boundary { get; private set; }
	public bool IsMovementAllowed { get; set; } = true;

	public bool IsMoved => PreviousPosition == _rawPosition;
	
	private Vector4I _bounds;
	private Vector2 _delay;
	private Vector2I _shakeOffset;
	private float _shakeTimer;
	private float _viewTimer;
	private Vector2 _rawPosition;
	private Vector2 _bufferOffset;
	private Vector2I _maxVelocity;
	
	public Camera()
	{
		Vector2I size = Scene.Instance.Size;
		TargetBoundary = new Vector4I(0, 0, size.X, size.Y);
		Boundary = TargetBoundary;
		
		int maxSpeed = SharedData.NoCameraCap ? ushort.MaxValue : SpeedCap;
		_maxVelocity = new Vector2I(maxSpeed, maxSpeed);
		
		if (SharedData.CheckpointData is not null)
		{
			LimitBottom = SharedData.CheckpointData.BottomCameraBound;
		}
	}
	
	public override void _Process(double delta)
	{
		IsActiveRegionChanged = false;
		
		MoveCamera();
		
		DrawPosition = _shakeOffset + ((Vector2I)_rawPosition + (Vector2I)_bufferOffset).Clamp(
			new Vector2I((int)Boundary.X, (int)Boundary.Y),
			new Vector2I((int)Boundary.Z, (int)Boundary.W) - SharedData.ViewSize);
		
		Views.Instance.UpdateBottomCamera(this);
		
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
	
	private void FollowPlayer(Vector2 targetPosition, IPlayer player)
	{
		if (player.Data.State != PlayerStates.Death) return;
		
		FollowPlayerY(targetPosition.Y, player);
		if (SharedData.CdCamera)
		{
			FollowTargetCdX(targetPosition.X);
			UpdateCdCamera(player);
		}
		else
		{
			FollowTargetX(targetPosition.X);
		}

		Objects.Player.Sprite.Animations animation = player.Data.Sprite.Animation;
		bool doShiftDown = animation == Objects.Player.Sprite.Animations.Duck;
		bool doShiftUp = animation == Objects.Player.Sprite.Animations.LookUp;

		if (doShiftDown || doShiftUp)
		{
			if (_viewTimer > 0f)
			{
				_viewTimer -= Scene.Instance.Speed;
			}
		}
		else if (SharedData.SpinDash || SharedData.Dash)
		{
			_viewTimer = MaxViewTime;
		}

		float offsetSpeed = 2f * Scene.Instance.Speed;
		
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
	
	private void UpdateCdCamera(IPlayer player)
	{
		const int shiftSpeedX = 2;
		float delta = shiftSpeedX * Scene.Instance.Speed;
		
		float groundSpeed = player.Data.Movement.GroundSpeed.Value;
		if (Math.Abs(groundSpeed) < 6f && player.Action != ActionFsm.States.SpinDash)
		{
			_bufferOffset.X = _bufferOffset.X.MoveToward(0f, delta);
			return;
		}
		
		if (_delay.X > 0f) return;
		
		int shiftSign = groundSpeed != 0f ? Math.Sign(groundSpeed) : (int)player.Data.Visual.Facing;
		
		const int shiftDistanceX = 64;
		_bufferOffset.X = _bufferOffset.X.MoveToward(shiftDistanceX * shiftSign, delta);
	}
	
	private void MoveCamera()
	{
		if (!IsMovementAllowed || Scene.Instance.State == Scene.States.Paused) return;

		PreviousPosition = _rawPosition;
		FollowTarget();
		UpdateShakeOffset();
		
		// Update boundaries
		float boundSpeed = Math.Max(2, BoundSpeed) * Scene.Instance.Speed;
		Vector2I farBounds = DrawPosition + SharedData.ViewSize;
		Boundary = new Vector4(
			MoveBoundaryForward(Boundary.X, TargetBoundary.X, DrawPosition.X, boundSpeed), // Left
			MoveBoundaryForward(Boundary.Y, TargetBoundary.Y, DrawPosition.Y, boundSpeed), // Top
			MoveBoundaryBackward(Boundary.Z, TargetBoundary.Z, farBounds.X, boundSpeed), // Right
			MoveBoundaryBackward(Boundary.W, TargetBoundary.W, farBounds.Y, boundSpeed) // Bottom
		);
	}
	
	private void FollowTarget()
	{
		if (_godotObjectTarget != null && !IsInstanceValid(_godotObjectTarget))
		{
			Target = null;
		}
		
		if (Target == null) return;
		
		Vector2 targetPosition = _target.Position - SharedData.ViewSize / 2;
		if (_playerTarget != null)
		{
			FollowPlayer(targetPosition, _playerTarget);
			return;
		}
		
		FollowTargetX(targetPosition.X);
		FollowTargetY(targetPosition.Y);
	}
	
	private void FollowTargetX(float targetPosition)
	{
		if (_delay.X > 0f)
		{
			_delay.X -= Scene.Instance.Speed;
			return;
		}
		
		const float freeSpaceX = 16f;
		float distance = targetPosition - _rawPosition.X;
		float speed = _maxVelocity.X * Scene.Instance.Speed;
		
		_rawPosition.X = distance switch
		{
			> 0 => distance <= speed ? targetPosition : _rawPosition.X + speed,
			< -freeSpaceX => distance + freeSpaceX >= -speed ? targetPosition + freeSpaceX : _rawPosition.X - speed,
			_ => _rawPosition.X
		};
	}
	
	private void FollowTargetCdX(float targetPosition)
	{
		if (_delay.X > 0f)
		{
			_delay.X -= Scene.Instance.Speed;
			return;
		}
		
		_rawPosition.X = _rawPosition.X.MoveToward(targetPosition, _maxVelocity.X * Scene.Instance.Speed);
	}
	
	private void FollowTargetY(float targetPosition)
	{
		if (_delay.Y > 0f)
		{
			_delay.Y -= Scene.Instance.Speed;
			return;
		}
		
		const float freeSpaceY = 32f;
		const float offsetY = 16f;
		float distance = targetPosition - _rawPosition.Y + offsetY;
		float speed = _maxVelocity.Y * Scene.Instance.Speed;
		
		_rawPosition.Y = distance switch
		{
			> freeSpaceY => distance <= speed + freeSpaceY ? 
				targetPosition + (offsetY - freeSpaceY) : _rawPosition.Y + speed,
			< -freeSpaceY => -distance <= speed + freeSpaceY ? 
				targetPosition + (offsetY + freeSpaceY) : _rawPosition.Y - speed,
			_ => _rawPosition.Y
		};
	}
	
	private void FollowPlayerY(float targetPosition, IPlayer player)
	{
		if (_delay.Y > 0f)
		{
			_delay.Y -= Scene.Instance.Speed;
			return;
		}
		
		float distance = targetPosition - _rawPosition.Y;

		MovementData movement = player.Data.Movement;
		if (movement.IsGrounded)
		{
			if (movement.IsSpinning)
			{
				CollisionData collision = player.Data.Collision;
				int offset = collision.RadiusNormal.Y - collision.Radius.Y;
				distance -= offset;
				targetPosition -= offset;
			}

			float limit = Scene.Instance.Speed * (Math.Abs(movement.GroundSpeed) < 8f ? 6f : _maxVelocity.Y);
			_rawPosition.Y = distance <= limit && distance >= -limit ? targetPosition : _rawPosition.Y + limit * MathF.Sign(distance);
			
			return;
		}
		
		const float freeSpaceY = 32f;
		const float offsetY = 16f;
		
		distance += offsetY;
		float speed = _maxVelocity.Y * Scene.Instance.Speed;
		_rawPosition.Y = distance switch
		{
			> freeSpaceY => distance <= speed + freeSpaceY ? 
				targetPosition + (offsetY - freeSpaceY) : _rawPosition.Y + speed,
			< -freeSpaceY => -distance <= speed + freeSpaceY ? 
				targetPosition + (offsetY + freeSpaceY) : _rawPosition.Y - speed,
			_ => _rawPosition.Y
		};
	}
	
	private void UpdateShakeOffset()
	{
		if (_shakeTimer <= 0f)
		{
			_shakeOffset = Vector2I.Zero;
			return;
		}
		
		_shakeOffset.X = CalculateShakeOffset(_shakeTimer, _shakeOffset.X);
		_shakeOffset.Y = CalculateShakeOffset(_shakeTimer, _shakeOffset.Y);
		_shakeTimer -= Scene.Instance.Speed;
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
		ActiveRegion = new Rect2I(position, SharedData.ViewSize * 2 + CullingBuffer);
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
