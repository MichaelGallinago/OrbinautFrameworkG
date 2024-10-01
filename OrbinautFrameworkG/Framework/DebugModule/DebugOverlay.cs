using System;
using System.Collections.Generic;
using Godot;

namespace OrbinautFrameworkG.Framework.DebugModule;

public partial class DebugOverlay : Node2D
{
#if DEBUG
	public enum SensorTypes : byte { None, Collision, HitBox, SolidBox }
	public event Action<SensorTypes> SensorDebugToggled;
	public SensorTypes SensorType
	{
		get => _sensorType;
		set
		{
			if ((value == SensorTypes.None) ^ (_sensorType == SensorTypes.None)) return;
			SensorDebugToggled?.Invoke(value);
			_sensorType = value;
		}
	}
	private SensorTypes _sensorType = SensorTypes.None;
	
	public List<Rect2> Collisions { get; } = [];
	public List<Rect2> SolidBoxes { get; } = [];
	public List<Rect2> HitBoxes { get; } = [];
	
	public DebugOverlay() => ProcessPriority = int.MinValue;
	public override void _Process(double delta)
	{
		Collisions.Clear();
		SolidBoxes.Clear();
		HitBoxes.Clear();
	}

	public override void _Draw()
	{
		switch (_sensorType)
		{
			case SensorTypes.Collision: DrawCollisions(); break;
			case SensorTypes.SolidBox: DrawSolidBoxes(); break;
			case SensorTypes.HitBox: DrawHitBoxes(); break;
			case SensorTypes.None: break;
			default: throw new ArgumentOutOfRangeException();
		}
	}
	
	public void ChangeSensorType()
	{
		if (++SensorType <= SensorTypes.SolidBox) return;
		SensorType = SensorTypes.None;
	}
	
	private void DrawCollisions()
	{
		foreach (Rect2 rectangle in Collisions)
		{
			DrawCollision(rectangle);
		}
	}
	
	private void DrawCollision(Rect2 rectangle)
	{
		
	}
	
	private void DrawSolidBoxes()
	{
		foreach (Rect2 rectangle in SolidBoxes)
		{
			DrawSolidBox(rectangle);
		}
	}
	
	private void DrawSolidBox(Rect2 rectangle)
	{
		
	}
	
	private void DrawHitBoxes()
	{
		foreach (Rect2 rectangle in HitBoxes)
		{
			DrawHitBox(rectangle);
		}
	}
	
	private void DrawHitBox(Rect2 rectangle)
	{
		
	}
#endif
}