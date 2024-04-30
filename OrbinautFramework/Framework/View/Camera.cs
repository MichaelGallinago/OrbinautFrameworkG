using System;
using Godot;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Player;

namespace OrbinautFramework3.Framework.View;

public partial class Camera : Camera2D, ICamera
{
	private static readonly Vector2I CullBuffer = new(320, 288);
	
	private const byte CentreOffset = 16;
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
			_target = value;
			_viewTimer = MaxViewTime;

			if (value is not ICameraHolder holder) return;
			BufferPosition = (Vector2I)holder.Position - SharedData.ViewSize + 16 * Vector2I.Down;
			_rawPosition = PreviousPosition = BufferPosition;
			
			if (holder.Camera != null)
			{
				holder.Camera.Target = null;
			}
			holder.Camera = this;
		}
	}
	private BaseObject _target;

	public Vector4I Bounds;
	
	public Vector2I BufferPosition { get; private set; }
	public Vector2I PreviousPosition { get; private set; }
	public Vector2 Delay { get; private set; }
	public int BoundSpeed { get; set; }
	public Vector4 Bound { get; set; }
	public Vector4 Limit { get; private set; }
	public bool IsMovementAllowed { get; set; }

	private Vector2I _shakeOffset;
	private float _shakeTimer;
	private float _viewTimer;
	private Vector2 _rawPosition;
	private Vector4 _previousLimit; // TODO: check if needed
	private Vector2I _bufferOffset;
	private Vector2I _maxSpeed;
	private Vector2 _speed;

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

	public override void _Process(double delta)
	{
		IsActiveRegionChanged = false;

		MoveCamera();

		_previousLimit = Limit;

		PreviousPosition = BufferPosition;
		BufferPosition = _shakeOffset + (Vector2I)(_rawPosition + _bufferOffset).Clamp(
			new Vector2(Limit.X, Limit.Y), new Vector2(Limit.Z, Limit.W) - SharedData.ViewSize);

		var finalPosition = new Vector2I(BufferPosition.X - Constants.RenderBuffer, BufferPosition.Y);
		
		Position = finalPosition;
		Bounds = new Vector4I(finalPosition.X, finalPosition.Y, 
			finalPosition.X + SharedData.ViewSize.X, finalPosition.Y + SharedData.ViewSize.Y);
		
		ForceUpdateScroll();
		UpdateActiveRegion();
	}

	public void UpdateDelay(int? delayX = null, int? delayY = null)
	{
		Delay = new Vector2(delayX ?? Delay.X, delayY ?? Delay.Y);
	}

	public void UpdateShakeTimer(int shakeTimer) => _shakeTimer = shakeTimer;

	public void UpdatePlayerCamera(PlayerData player)
	{
		if (Target != player || player.DeathState == DeathStates.Restart) return;

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

	public bool CheckPositionInSafeRegion(Vector2I position)
	{
		int distanceY = position.Y - BufferPosition.Y + 128;
		position.X -= ActiveRegion.Position.X;

		return position.X >= 0 && position.X < ActiveRegion.Size.X && 
		       distanceY  >= 0 && distanceY  < ActiveRegion.Size.Y && 
		       position.Y < Limit.W;
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
	
	private void MoveCamera()
	{
		if (Scene.Local.IsPaused || !IsMovementAllowed) return;
		
		float processSpeed = Scene.Local.ProcessSpeed;

		// Get boundary update speed
		float boundSpeed = Math.Max(2, BoundSpeed) * processSpeed;

		FollowTarget(processSpeed);
			
		// Update boundaries
		Vector2I farBounds = BufferPosition + SharedData.ViewSize;
		Limit = new Vector4(
			MoveBoundaryForward(Limit.X, Bound.X, BufferPosition.X, boundSpeed), // Left
			MoveBoundaryForward(Limit.Y, Bound.Y, BufferPosition.Y, boundSpeed), // Top
			MoveBoundaryBackward(Limit.Z, Bound.Z, farBounds.X, boundSpeed), // Right
			MoveBoundaryBackward(Limit.W, Bound.W, farBounds.Y, boundSpeed) // Bottom
		);
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

	private void UpdateActiveRegion()
	{
		Vector2I position = BufferPosition + sbyte.MinValue * Vector2I.One;
		position.X &= sbyte.MinValue;
		position.Y &= sbyte.MinValue;
		ActiveRegion = new Rect2I(position, SharedData.ViewSize + CullBuffer);
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
		Vector2 delay = Delay;
		for (var i = 0; i < 2; i++)
		{
			if (delay[i] > 0f)
			{
				delay[i] -= processSpeed;
				continue;
			}
			
			_rawPosition[i] += _speed[i];
		}

		Delay = delay;
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
