using System;
using System.Linq;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.CommonObject;
using OrbinautFramework3.Objects.Player;

namespace OrbinautFramework3;

public partial class Camera : Camera2D
{
	private const byte CameraCentreOffset = 16;

	public static Camera MainCamera { get; set; }
    
	[Export] public CommonObject Target { get; set; }
    
	public Vector4I Bounds { get; private set; }
		
	public readonly Vector2I MaxSpeed;
	public Vector2 Speed;
	public Vector2I BufferPosition;
	public Vector2 RawPosition;
	public Vector2 Delay;
	public Vector2I BufferOffset;
	public Vector2I BoundSpeed;
	public Vector4 Bound;
	public Vector4 Limit;
	public Vector4 PreviousLimit; // TODO: check if needed

	public Vector2I ShakeOffset;
	public float ShakeTimer;

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
		if (Target != null || Player.Players.Count == 0) return;
		Player playerTarget = Player.Players.First();
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
		Delay = new Vector2(delayX ?? Delay.X, delayY ?? Delay.Y);
	}

	public void UpdateShakeTimer(int shakeTimer) => ShakeTimer = shakeTimer;

	public Vector2I GetActiveArea()
	{
		var position = (int)Position.X;
		
		// Adjust the view_x based on whether the camera is the framework camera
		if (MainCamera == this)
		{
			position += Constants.RenderBuffer;
		}

		position &= sbyte.MinValue;
		
		return new Vector2I(position + sbyte.MinValue, position + FrameworkData.ViewSize.X + 320);
	}

	private void EndStep(float processSpeed)
	{
		if (MainCamera != this) return;
		var boundSpeed = 0f;
		
		if (FrameworkData.UpdateObjects)
		{
			// Get boundary update speed
			boundSpeed = Math.Max(2, BoundSpeed.X) * processSpeed;
			
			FollowTarget(processSpeed);
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
			Speed = new Vector2I();
			return;
		}
		
		Vector2I distance = (Vector2I)Target.Position - (Vector2I)RawPosition - FrameworkData.ViewSize / 2;
		distance.Y += CameraCentreOffset;

		int extraX = FrameworkData.CDCamera ? 0 : 8;
			
		Speed.X = CalculateSpeed(distance.X + extraX, extraX, MaxSpeed.X * processSpeed);
			
		if (Target is Player { IsGrounded: true } playerTarget)
		{	
			if (playerTarget.IsSpinning)
			{
				distance.Y -= playerTarget.RadiusNormal.Y - playerTarget.InteractData.Radius.Y;
			}
				
			float limit = Math.Abs(playerTarget.GroundSpeed) < 8f ? 6f : MaxSpeed.Y * processSpeed;
			Speed.Y = Math.Clamp(distance.Y, -limit, limit);
			return;
		}

		Speed.Y = CalculateSpeed(distance.Y, 32, MaxSpeed.Y * processSpeed);
	}
	
	private void UpdateShakeOffset(float processSpeed)
	{
		if (ShakeTimer <= 0f)
		{
			ShakeOffset = new Vector2I();
			return;
		}
		
		ShakeOffset.X = CalculateShakeOffset(ShakeTimer, ShakeOffset.X);
		ShakeOffset.Y = CalculateShakeOffset(ShakeTimer, ShakeOffset.Y);
		ShakeTimer -= processSpeed;
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
			
			RawPosition[i] += Speed[i];
		}
	}

	private static float CalculateSpeed(int difference, int threshold, float maxSpeed)
	{
		float distance = Math.Abs(difference);
		return distance <= threshold ? 0 : 
			Math.Clamp((distance - threshold) * Math.Sign(difference), -maxSpeed, maxSpeed);
	}
	
	
	private static int CalculateShakeOffset(float shakeTimer, int shakeOffset)
	{
		return shakeOffset switch
		{
			0 => (int)shakeTimer,
			< 0 => -1 - shakeOffset,
			_ => -shakeOffset
		};
	}
}
