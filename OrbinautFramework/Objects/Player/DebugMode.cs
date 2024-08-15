using System;
using System.Collections.Generic;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.InputModule;

namespace OrbinautFramework3.Objects.Player;

public class DebugMode(IEditor editor)
{
#if DEBUG
	private const float Acceleration = 0.046875f;
#else
	private const float Acceleration = 0.1875f;
#endif
	private const byte SpeedLimit = 16;

	public bool IsEnabled { get; set; }
	
	private readonly PackedScene[] _prefabs = Scene.Instance.DebugModePrefabs;
	
	private int _index;
	private float _speed;

	public bool Update(IInputContainer input)
    {
#if !DEBUG
	    if (!SharedData.IsDebugModeEnabled) return false;  
#endif

		bool debugButton = IsDebugButtonPressed(IsEnabled, input.Press.B);
		
		if (debugButton)
		{
			if (!IsEnabled)
			{
				if (Scene.Instance.IsStage)
				{
					Scene.Instance.Players.First().ResetMusic();
				}
				
				_speed = 0f;

				Scene.Instance.State = Scene.States.Normal;
				Scene.Instance.AllowPause = true;
				
				editor.OnEnableEditMode();
				IsEnabled = true;
			}
			else
			{
				editor.OnDisableEditMode();
				IsEnabled = false;
			}
		}
		
		// Continue if Debug mode is enabled
		if (!IsEnabled) return false;

		// Update speed and position (move faster if in developer mode)
		if (input.Down.Up || input.Down.Down || input.Down.Left || input.Down.Right)
		{
			_speed = MathF.Min(_speed + Acceleration, SpeedLimit);
			
			Vector2 position = editor.Position;

			float speed = _speed * Scene.Instance.ProcessSpeed;
			
			if (input.Down.Up) position.Y -= speed;
			if (input.Down.Down) position.Y += speed;
			if (input.Down.Left) position.X -= speed;
			if (input.Down.Right) position.X += speed;

			editor.Position = position;
		}
		else
		{
			_speed = 0f;
		}

		if (input.Down.A && input.Press.C)
		{
			if (--_index < 0)
			{
				_index = _prefabs.Length - 1;
			}
		}
		else if (input.Press.A)
		{
			if (++_index >= _prefabs.Length)
			{
				_index = 0;
			}
		}
		else if (input.Press.C)
		{
			Node node = _prefabs[_index].Instantiate();
			
			if (node is ICullable cullable)
			{
				cullable.CullingType = ICullable.Types.Delete;
			}

			if (node is Node2D node2D)
			{
				node2D.Scale = new Vector2(node2D.Scale.X * (float)editor.Facing, node2D.Scale.Y);
				node2D.Position = editor.Position;
			}
			
			Scene.Instance.AddChild(node);
		}
		
		return true;
	}

    private static bool IsDebugButtonPressed(bool isDebugMode, bool isPressB)
    {
#if DEBUG
	    if (isDebugMode)
	    {
		    return InputUtilities.DebugButtonPress || isPressB;
	    }

	    return InputUtilities.DebugButtonPress;
#else
		return isPressB;
#endif
    }
}
