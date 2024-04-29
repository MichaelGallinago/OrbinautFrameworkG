using System;
using System.Collections.Generic;
using Godot;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.View;

namespace OrbinautFramework3.Framework;

public class ObjectCuller
{
	public static ObjectCuller Local => Scene.Local.Culler;
	
	private bool _isCullToggled = true;
	private readonly HashSet<BaseObject> _waitingObjects = [];
	private readonly HashSet<BaseObject> _stoppedObjects = [];
	public HashSet<BaseObject> ActiveObjects { get; } = [];
	
	public void RemoveFromCulling(BaseObject baseObject)
	{
		if (ActiveObjects.Remove(baseObject)) return;
		_stoppedObjects.Remove(baseObject);
	}

	public void AddToCulling(BaseObject baseObject) => ActiveObjects.Add(baseObject);
	
	public void EarlyCull()
	{
		if (StopAllObjets()) return;
		
		StopObjectsByBehaviour();
		
		if (_isCullToggled)
		{
			ResumeRegions(Views.Local.Cameras);
			_isCullToggled = false;
			return;
		}

		ResumeRegions(Views.Local.GetCamerasWithUpdatedRegions());
	}

    private bool StopAllObjets()
    {
	    if (!Scene.Local.IsPaused && Scene.Local.UpdateObjects) return false;

	    foreach (BaseObject baseObject in ActiveObjects)
	    {
		    _stoppedObjects.Add(baseObject);
		    baseObject.SetProcess(false);
	    }

	    ActiveObjects.Clear();

	    _isCullToggled = true;
	    return true;
    }
    
    private void ResumeRegions(ReadOnlySpan<ICamera> cameras)
    {
	    if (cameras.Length == 0) return;

	    foreach (BaseObject baseObject in _stoppedObjects)
	    {
		    var position = (Vector2I)baseObject.Position;
		    foreach (ICamera camera in cameras)
		    {
			    if (!camera.CheckPositionInActiveRegion(position)) continue;
				_stoppedObjects.Remove(baseObject);
			    ActiveObjects.Add(baseObject);
			    baseObject.SetProcess(true);
			    break;
		    }
	    }
    }
    
    private void StopObjectsByBehaviour()
    {
	    foreach (BaseObject baseObject in ActiveObjects)
	    {
		    baseObject.InteractData.IsInteract = true;
		    StopObjectByBehaviour(baseObject);
	    }
    }
    
    private void StopObjectByBehaviour(BaseObject baseObject)
    {
	    switch (baseObject.Culling)
	    {
		    case BaseObject.CullingType.Delete: DeleteObject(baseObject); break;
		    case BaseObject.CullingType.Reset: ResetObject(baseObject); break;
		    case BaseObject.CullingType.ResetX: ResetXObject(baseObject); break;
		    case BaseObject.CullingType.ResetY: ResetYObject(baseObject); break;
	    }
    }

    private static void DeleteObject(Node2D baseObject)
    {
	    var position = (Vector2I)baseObject.Position;
	    foreach (ICamera camera in Views.Local.Cameras)
	    {
		    if (camera.CheckPositionInSafeRegion(position) && baseObject.Position.Y < camera.Bound.W) return;
	    }
	    
	    baseObject.QueueFree();
    }

    private void ResetObject(BaseObject baseObject)
    {
	    var position = (Vector2I)baseObject.Position;
	    foreach (ICamera camera in Views.Local.Cameras)
	    {
		    if (camera.CheckPositionInActiveRegion(position)) return;
	    }

	    var respawnPosition = (Vector2I)baseObject.ResetData.Position;
	    foreach (ICamera camera in Views.Local.Cameras)
	    {
		    if (!camera.CheckPositionInActiveRegion(respawnPosition)) continue;
		    ActiveObjects.Remove(baseObject);
		    baseObject.SetProcess(false);
		    baseObject.Hide();
		    return;
	    }
	    
	    baseObject.Reset();
    }
    
    private void ResetXObject(BaseObject baseObject)
    {
	    var position = (int)baseObject.Position.X;
	    foreach (ICamera camera in Views.Local.Cameras)
	    {
		    if (camera.CheckXInActiveRegion(position)) return;
	    }

	    ActiveObjects.Remove(baseObject);
	    baseObject.SetProcess(false);
	    
	    var respawnPosition = (int)baseObject.ResetData.Position.X;
	    foreach (ICamera camera in Views.Local.Cameras)
	    {
		    if (!camera.CheckXInActiveRegion(respawnPosition)) continue;
		    _waitingObjects.Add(baseObject);
		    baseObject.Hide();
		    return;
	    }

	    baseObject.Reset();
	    baseObject.SetProcess(false);
	    baseObject.Hide();
    }
    
    private void ResetYObject(BaseObject baseObject)
    {
	    var position = (int)baseObject.Position.Y;
	    foreach (ICamera camera in Views.Local.Cameras)
	    {
		    if (camera.CheckYInActiveRegion(position)) return;
	    }

	    var respawnPosition = (int)baseObject.ResetData.Position.Y;
	    foreach (ICamera camera in Views.Local.Cameras)
	    {
		    if (!camera.CheckYInActiveRegion(respawnPosition)) continue;
		    ActiveObjects.Remove(baseObject);
		    baseObject.SetProcess(false);
		    baseObject.Hide();
		    return;
	    }
	    
	    baseObject.Reset();
    }
}
