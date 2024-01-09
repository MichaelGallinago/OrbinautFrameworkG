using System;
using System.Collections.Generic;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Input;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Player;

public class EditMode
{
	private const byte AccelerationMultiplier = 4;
	private const float Acceleration = 0.046875f;
	private const byte SpeedLimit = 16;

	private int _index;
	private float _speed;
	private readonly List<Type> _objects;
    
    public EditMode()
    {
	    _objects =
	    [
		    typeof(Common.Ring.Ring), typeof(Common.GiantRing.GiantRing), typeof(Common.ItemBox.ItemBox),
		    typeof(Common.Springs.Spring), typeof(Common.Motobug.Motobug), typeof(Common.Signpost.Signpost)
	    ];
	    
	    switch (FrameworkData.CurrentScene)
	    {
		    case Stages.TSZ.StageTsz:
			    // TODO: debug objects
			    _objects.AddRange(new List<Type>
			    {
				    //typeof(obj_platform_swing_tsz), typeof(obj_platform_tsz), typeof(obj_falling_floor_tsz), typeof(obj_block_tsz)
			    });
			    break;
	    }
    }
    
    public bool Update(float processSpeed, PlayerData playerData, InputContainer input)
	{
		if (playerData.Id > 0 || !(FrameworkData.PlayerEditMode || FrameworkData.DeveloperMode)) return false;

		bool debugButton;
		
		// If in developer mode, remap debug button to SpaceBar
		if (FrameworkData.DeveloperMode)
		{
			debugButton = InputUtilities.DebugButtonPress;
			
			if (playerData.IsEditMode)
			{
				debugButton = debugButton || input.Press.B;
			}
		}
		else
		{
			debugButton = input.Press.B;
		}
		
		if (debugButton)
		{
			if (!playerData.IsEditMode)
			{
				if (FrameworkData.CurrentScene.IsStage)
				{
					//TODO: audio
					//stage_reset_bgm();
				}
				
				ResetGravity();
				ResetState();
				ResetZIndex();

				FrameworkData.UpdateAnimations = true;
				FrameworkData.UpdateObjects = true;
				FrameworkData.UpdateTimer = true;
				FrameworkData.AllowPause = true;
				
				playerData.ObjectInteraction = false;
				
				_speed = 0;
				playerData.IsEditMode = true;
				
				playerData.Visible = true;
			}
			else
			{
				playerData.Speed = new Vector2();
				playerData.GroundSpeed = 0f;

				playerData.Sprite.AnimationType = Animations.Move;
				
				playerData.ObjectInteraction = true;
				playerData.IsEditMode = false;
				playerData.IsDead = false;
			}
		}
		
		// Continue if Edit mode is enabled
		if (!playerData.IsEditMode) return false;

		// Update speed and position (move faster if in developer mode)
		if (input.Down.Up || input.Down.Down || input.Down.Left || input.Down.Right)
		{
			_speed = MathF.Min(_speed + (FrameworkData.DeveloperMode ? 
				Acceleration * AccelerationMultiplier : Acceleration), SpeedLimit);

			Vector2 position = playerData.Position;

			if (input.Down.Up)
			{
				position.Y -= _speed * processSpeed;
			}
			
			if (input.Down.Down)
			{
				position.Y += _speed * processSpeed;
			}
			
			if (input.Down.Left)
			{
				position.X -= _speed * processSpeed;
			}
			
			if (input.Down.Right)
			{
				position.X += _speed * processSpeed;
			}

			playerData.Position = position;
		}
		else
		{
			_speed = 0;
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
			
			newObject.Scale = new Vector2(newObject.Scale.X * (int)playerData.Facing, newObject.Scale.Y);
			newObject.SetBehaviour(BaseObject.BehaviourType.Delete);
			FrameworkData.CurrentScene.AddChild(newObject);
		}
		
		return true;
	}
}