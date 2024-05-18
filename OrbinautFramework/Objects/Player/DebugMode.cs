using System;
using System.Collections.Generic;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.InputModule;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Player;

//TODO: replace by prefabs
public class DebugMode
{
	private const byte AccelerationMultiplier = 4;
	private const float Acceleration = 0.046875f;
	private const byte SpeedLimit = 16;

	private int _index;
	private float _speed;
	private readonly List<Type> _objects;
    
    public DebugMode()
    {
	    _objects =
	    [
		    typeof(Common.Ring.Ring), typeof(Common.GiantRing.GiantRing), typeof(Common.ItemBox.ItemBox),
		    typeof(Common.Springs.Spring), typeof(Common.Motobug.Motobug), typeof(Common.Signpost.Signpost)
	    ];
	    
	    switch (Scene.Local)
	    {
		    case Stages.TSZ.StageTsz:
			    // TODO: debug objects
			    _objects.AddRange(new List<Type>
			    {
				    //typeof(obj_platform_swing_tsz), typeof(obj_platform_tsz),
				    //typeof(obj_falling_floor_tsz), typeof(obj_block_tsz)
			    });
			    break;
	    }
    }
    
    public bool Update(IEditor editor, IInputContainer input)
    {
		if (!SharedData.DevMode) return false;

		bool debugButton = IsDebugButtonPressed(editor.IsDebugMode, input.Press.B);
		
		if (debugButton)
		{
			if (!editor.IsDebugMode)
			{
				if (Scene.Local.IsStage)
				{
					Scene.Local.Players.First().ResetMusic();
				}
				
				_speed = 0;
				
				Scene.Local.UpdateObjects = true;
				Scene.Local.AllowPause = true;
				
				editor.OnEnableEditMode();
				editor.IsDebugMode = true;
			}
			else
			{
				editor.OnDisableEditMode();
				editor.IsDebugMode = false;
			}
		}
		
		// Continue if Debug mode is enabled
		if (!editor.IsDebugMode) return false;

		// Update speed and position (move faster if in developer mode)
		if (input.Down.Up || input.Down.Down || input.Down.Left || input.Down.Right)
		{
			_speed = MathF.Min(_speed + (SharedData.DevMode ? 
				Acceleration * AccelerationMultiplier : Acceleration), SpeedLimit);
			
			Vector2 position = editor.Position;

			float speed = _speed * Scene.Local.ProcessSpeed;
			
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
				_index = _objects.Count - 1;
			}
		}
		else if (input.Press.A)
		{
			if (++_index >= _objects.Count)
			{
				_index = 0;
			}
		}
		else if (input.Press.C)
		{
			//TODO: replace by prefabs
			if (Activator.CreateInstance(_objects[_index]) is not BaseObject newObject) return true;
			
			newObject.Scale = new Vector2(newObject.Scale.X * (int)editor.Facing, newObject.Scale.Y);
			newObject.Culling = BaseObject.CullingType.Delete;
			Scene.Local.AddChild(newObject);
		}
		
		return true;
	}

    private static bool IsDebugButtonPressed(bool isDebugMode, bool isPressB)
    {
	    // If in developer mode, remap debug button to SpaceBar
	    if (!SharedData.DevMode) return isPressB;
	    
	    bool debugButton = InputUtilities.DebugButtonPress;
			
	    if (isDebugMode)
	    {
		    return debugButton || isPressB;
	    }

	    return debugButton;
    }
}
