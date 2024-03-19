using System;
using System.Linq;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Player;

namespace OrbinautFramework3;

public partial class Camera : Camera2D
{
	private const byte CentreOffset = 16;
	private const byte MaxViewTime = 120;
	private const byte SpeedCap = 16;
	
	//TODO: Replace to interface
	public static Camera Main { get; set; }
    
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
		Main = this;
		
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

	public override void _ExitTree()
	{
		if (Main != this) return;
		Main = null;
	}
	
	public override void _Process(double delta)
	{
		if (Main != this) return;
		
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
		Limit.X = MoveBoundaryForward(Limit.X, Bound.X, BufferPosition.X, boundSpeed); // Left
		Limit.Y = MoveBoundaryForward(Limit.Y, Bound.Y, BufferPosition.Y, boundSpeed); // Top
		Limit.Z = MoveBoundaryBackward(Limit.Z, Bound.Z, farBounds.X, boundSpeed); // Right
		Limit.W = MoveBoundaryBackward(Limit.W, Bound.W, farBounds.Y, boundSpeed); // Bottom

		_previousLimit = Limit;

		BufferPosition = _shakeOffset + (Vector2I)(_rawPosition + _bufferOffset).Clamp(
			new Vector2(Limit.X, Limit.Y), new Vector2(Limit.Z, Limit.W) - SharedData.ViewSize);

		var finalPosition = new Vector2I(BufferPosition.X - Constants.RenderBuffer, BufferPosition.Y);
		
		Position = finalPosition;
		Bounds = new Vector4I(finalPosition.X, finalPosition.Y, 
			finalPosition.X + SharedData.ViewSize.X, finalPosition.Y + SharedData.ViewSize.Y);
		
		ForceUpdateScroll();
	}

	public void UpdateDelay(int? delayX = null, int? delayY = null)
	{
		Delay = new Vector2(delayX ?? Delay.X, delayY ?? Delay.Y);
	}

	public void UpdateShakeTimer(int shakeTimer) => _shakeTimer = shakeTimer;

	public Vector2I GetActiveArea()
	{
		var position = (int)Position.X;
		
		// Adjust the position based on whether the camera is the main camera
		if (Main == this)
		{
			position += Constants.RenderBuffer;
		}

		position &= sbyte.MinValue;
		
		return new Vector2I(position + sbyte.MinValue, position + SharedData.ViewSize.X + 320);
	}

	public void UpdatePlayerCamera(PlayerData player)
	{
		if (Target != player || player.IsDead) return;

		if (SharedData.CDCamera)
		{
			const int shiftDistanceX = 64;
			const int shiftSpeedX = 2;

			int shiftDirectionX = player.GroundSpeed != 0f ? Math.Sign(player.GroundSpeed) : (int)player.Facing;

			if (Math.Abs(player.GroundSpeed) >= 6f || player.Action == Actions.SpinDash)
			{
				if (Delay.X == 0f && _bufferOffset.X != shiftDistanceX * shiftDirectionX)
				{
					_bufferOffset.X += shiftSpeedX * shiftDirectionX;
				}
			}
			else
			{
				_bufferOffset.X -= shiftSpeedX * Math.Sign(_bufferOffset.X);
			}
		}

		bool doShiftDown = player.Animation == Animations.Duck;
		bool doShiftUp = player.Animation == Animations.LookUp;

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

		int extraX = SharedData.CDCamera ? 0 : 8;
		
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
