using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.InputModule;
using OrbinautFramework3.Objects.Player;

namespace OrbinautFramework3.Objects;

public class DebugMode(IEditor editor)
{
#if DEBUG
	private const float Acceleration = 0.046875f;
#else
	private const float Acceleration = 0.1875f;
#endif
	private const byte SpeedLimit = 16;

	public bool IsEnabled { get; private set; }
	
	private readonly PackedScene[] _prefabs = Scene.Instance.DebugModePrefabs;
	
	private int _index;
	private float _speed;

	public bool Update()
    {
#if !DEBUG
	    if (!SharedData.IsDebugModeEnabled) return false;  
#endif
		if (IsDebugButtonPressed(IsEnabled, editor.Input.Press.B))
		{
			SwitchMode();
		}
		

		if (!IsEnabled) return false;
		
		Process();
		return true;
	}

	private void SwitchMode()
	{
		if (IsEnabled)
		{
			editor.OnDisableEditMode();
			IsEnabled = false;
			return;
		}

		if (Scene.Instance.IsStage)
		{
			Scene.Instance.Players.FirstOrDefault().ResetMusic();
		}
			
		_speed = 0f;

		Scene.Instance.State = Scene.States.Normal;
		Scene.Instance.AllowPause = true;
			
		editor.OnEnableEditMode();
		IsEnabled = true;
	}

	private void Process()
	{
		UpdateSpeedAndPosition();

		if (SwapPrefab() || !editor.Input.Press.C) return;
		
		SpawnPrefab();
	}

	private bool SwapPrefab()
	{
		if (editor.Input.Down is { A: true, C: true })
		{
			if (--_index < 0)
			{
				_index = _prefabs.Length - 1;
			}

			return true;
		}

		if (!editor.Input.Press.A) return false;
		
		if (++_index >= _prefabs.Length)
		{
			_index = 0;
		}
		return true;
	}

	private void SpawnPrefab()
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

	private void UpdateSpeedAndPosition()
	{
		Buttons input = editor.Input.Down;
		if (input is { Up: false, Down: false, Left: false, Right: false })
		{
			_speed = 0f;
			return;
		}

		_speed = MathF.Min(_speed + Acceleration, SpeedLimit);
		
		Vector2 position = editor.Position;

		float speed = _speed * Scene.Instance.Speed;

		if (input.Up) position.Y -= speed;
		if (input.Down) position.Y += speed;
		if (input.Left) position.X -= speed;
		if (input.Right) position.X += speed;

		editor.Position = position;
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
