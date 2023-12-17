using System;
using System.Linq;
using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3;

public partial class Camera : Camera2D
{
	private const byte CameraCentreOffset = 16;

	public static Camera MainCamera { get; set; }
    
	[Export] public Framework.CommonObject.CommonObject Target { get; set; }
    
	public Vector4I Bounds { get; private set; }
		
	public Vector2I MaxSpeed;
	public Vector2I Speed;
	public Vector2I BufferPosition;
	public Vector2 RawPosition;
	public Vector2I Delay;
	public Vector2I BufferOffset;
	public Vector2I BoundSpeed;
	public Vector4 Bound;
	public Vector4 Limit;
	public Vector4 PreviousLimit; // TODO: check if needed

	public Vector2I ShakeOffset;
	private int _shakeTimer;

	public Camera()
	{
		Bound = new Vector4I(LimitTop, LimitLeft, LimitBottom, LimitRight);
		Limit = Bound;
		PreviousLimit = Bound;
		MaxSpeed = new Vector2I(16, 16);

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
		BufferPosition = (Vector2I)playerTarget.Position - FrameworkData.ViewSize;
		BufferPosition.Y += 16;
		
		RawPosition = BufferPosition;
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

	public void UpdateDelay(int? delayX = null, int? delayY = null)
	{
		Delay = new Vector2I(delayX ?? Delay.X, delayY ?? Delay.Y);
	}

	public void UpdateShakeTimer(int shakeTimer) => _shakeTimer = shakeTimer;

	public Vector2I GetActiveArea()
	{
		var position = (int)Position.X;
		
		// Adjust the view_x based on whether the camera is the framework camera
		if (MainCamera == this)
		{
			position += Constants.RenderBuffer;
		}

		position &= -128;
		
		return new Vector2I(position - 128, position + FrameworkData.ViewSize.X + 320);
	}

	private void EndStep(double processSpeed)
	{
		if (MainCamera != this) return;
		var boundSpeed = 0f;

		if (FrameworkData.UpdateObjects)
		{
			var processSpeedF = (float)processSpeed;
			
			// Get boundary update speed
			boundSpeed = Math.Max(2, BoundSpeed.X) * processSpeedF;
			
			FollowTarget(processSpeedF);
		}
		
		// Update boundaries
		Vector2I farBounds = BufferPosition + FrameworkData.ViewSize;
		Limit.X = MoveBoundaryForward(Limit.X, Bound.X, BufferPosition.X, boundSpeed); // Left
		Limit.Y = MoveBoundaryForward(Limit.Y, Bound.Y, BufferPosition.Y, boundSpeed); // Top
		Limit.Z = MoveBoundaryBackward(Limit.Z, Bound.Z, farBounds.X, boundSpeed); // Right
		Limit.W = MoveBoundaryBackward(Limit.W, Bound.W, farBounds.Y, boundSpeed); // Bottom

		PreviousLimit = Limit;

		BufferPosition = ShakeOffset + (Vector2I)(RawPosition + BufferOffset).Clamp(
			new Vector2(Limit.X, Limit.Y), new Vector2(Limit.Z, Limit.W) - FrameworkData.ViewSize);

		var finalPosition = new Vector2I(BufferPosition.X - Constants.RenderBuffer, BufferPosition.Y);
		
		Position = finalPosition;
		Bounds = new Vector4I(finalPosition.X, finalPosition.Y, 
			finalPosition.X + FrameworkData.ViewSize.X, finalPosition.Y + FrameworkData.ViewSize.Y);
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

	private void FollowTarget(float processSpeedF)
	{
		if (Target != null && !IsInstanceValid(Target))
		{
			Target = null;
		}

		UpdateSpeed();
		UpdateShakeOffset();
		UpdateRawPosition(processSpeedF);
	}

	private void UpdateSpeed()
	{
		if (Target == null)
		{
			Speed = new Vector2I();
			return;
		}
		
		Vector2I targetPosition = (Vector2I)Target.Position - (Vector2I)RawPosition - FrameworkData.ViewSize / 2;
		targetPosition.Y += CameraCentreOffset;

		int extraX = FrameworkData.CDCamera ? 0 : 8;
			
		Speed.X = CalculateSpeed(targetPosition.X + extraX, extraX, MaxSpeed.X);
			
		if (Target is Objects.Player.Player { IsGrounded: true } playerTarget)
		{	
			if (playerTarget.IsSpinning)
			{
				targetPosition.Y -= playerTarget.RadiusNormal.Y - playerTarget.InteractData.Radius.Y;
			}
				
			int limit = Math.Abs(playerTarget.GroundSpeed) < 8 ? 6 : MaxSpeed.Y;
			Speed.Y = Math.Clamp(targetPosition.Y, -limit, limit);
			return;
		}

		Speed.Y = CalculateSpeed(targetPosition.Y, 32, MaxSpeed.Y);
	}
	
	private void UpdateShakeOffset()
	{
		if (_shakeTimer > 0)
		{
			ShakeOffset.X = CalculateShakeOffset(_shakeTimer, ShakeOffset.X);
			ShakeOffset.Y = CalculateShakeOffset(_shakeTimer, ShakeOffset.Y);
			_shakeTimer--;
		}
		else
		{
			ShakeOffset = new Vector2I();
		}
	}

	private void UpdateRawPosition(float processSpeedF)
	{
		for (var i = 0; i < 2; i++)
		{
			if (Delay[i] > 0)
			{
				Delay[i]--;
				continue;
			}
			
			RawPosition[i] += Speed[i] * processSpeedF;
		}
	}

	private static int CalculateSpeed(int difference, int threshold, int maxSpeed)
	{
		int distance = Math.Abs(difference);
		return distance <= threshold ? 0 : 
			Math.Clamp((distance - threshold) * Math.Sign(difference), -maxSpeed, maxSpeed);
	}
	
	
	private static int CalculateShakeOffset(int shakeTimer, int shakeOffset)
	{
		return shakeOffset switch
		{
			0 => shakeTimer,
			< 0 => -1 - shakeOffset,
			_ => -shakeOffset
		};
	}
}
