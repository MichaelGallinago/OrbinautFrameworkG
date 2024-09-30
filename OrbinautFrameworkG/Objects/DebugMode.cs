using System;
using Godot;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.Culling;
using OrbinautFrameworkG.Framework.InputModule;
using OrbinautFrameworkG.Framework.SceneModule;
using OrbinautFrameworkG.Framework.View;
using OrbinautFrameworkG.Objects.Player;
#if !DEBUG
using OrbinautFrameworkG.Framework.StaticStorages;
#endif

namespace OrbinautFrameworkG.Objects;

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

	public bool Switch()
	{
#if !DEBUG
	    if (!SharedData.IsDebugModeEnabled) return false;  
#endif
		if (!IsDebugButtonPressed(IsEnabled, editor.Input.Press)) return false;
		
		SwitchMode();
		return true;
	}

	public void Update()
    {
		if (!IsEnabled) return;
		Process();
	}

	private void SwitchMode()
	{
		_speed = 0f;
		
		if (IsEnabled)
		{
			editor.OnDisableDebugMode();
			IsEnabled = false;
			return;
		}
		
		Scene.Instance.State = Scene.States.Normal;
		Scene.Instance.AllowPause = true;
		
		editor.OnEnableDebugMode();
		IsEnabled = true;
	}

	private void Process()
	{
		UpdateSpeedAndPosition();
		
		if (SwapPrefab() || !editor.Input.Press.X) return;
		
		SpawnPrefab();
	}

	private bool SwapPrefab()
	{
		if (editor.Input.Down is { A: true, X: true })
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
			cullable.CullingType = ICullable.Types.Remove;
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
		
		if (editor.IsCameraTarget(out ICamera camera))
		{
			Vector4 boundary = camera.Boundary;
			position = position.Clamp(new Vector2(boundary.X, boundary.Y), new Vector2(boundary.Z, boundary.W));
		}
		
		editor.Position = position;
	}

    private static bool IsDebugButtonPressed(bool isDebugMode, Buttons press)
    {
#if DEBUG
	    return isDebugMode && press.B || press.X;
#else
		return press.B;
#endif
    }
}
