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
    
    private void StopObjectByBehaviour(BaseObject commonObject)
    {
	    switch (commonObject.Culling)
	    {
		    case BaseObject.CullingType.Delete: DeleteObject(commonObject); break;
		    case BaseObject.CullingType.Reset: ResetObject(commonObject); break;
		    case BaseObject.CullingType.ResetX: ResetXObject(commonObject); break;
		    case BaseObject.CullingType.ResetY: ResetYObject(commonObject); break;
	    }
    }

    private static void DeleteObject(Node2D commonObject)
    {
	    var position = (Vector2I)commonObject.Position;
	    foreach (ICamera camera in Views.Local.Cameras)
	    {
		    if (camera.CheckPositionInSafeRegion(position) && commonObject.Position.Y < camera.Bound.W) return;
	    }
	    
	    commonObject.QueueFree();
    }

    private void ResetObject(BaseObject commonObject)
    {
	    var position = (Vector2I)commonObject.Position;
	    foreach (ICamera camera in Views.Local.Cameras)
	    {
		    if (camera.CheckPositionInActiveRegion(position)) return;
	    }

	    if (commonObject.Spawner == null)
	    {
		    commonObject.QueueFree();
		    return;
	    }

	    var respawnPosition = (Vector2I)commonObject.Spawner.Position;
	    foreach (ICamera camera in Views.Local.Cameras)
	    {
		    if (!camera.CheckPositionInActiveRegion(respawnPosition)) continue;
		    ActiveObjects.Remove(commonObject);
		    commonObject.SetProcess(false);
		    commonObject.Hide();
		    return;
	    }
	    
	    commonObject.Spawner.Reset();
    }
    
    private void ResetXObject(BaseObject commonObject)
    {
	    var position = (int)commonObject.Position.X;
	    foreach (ICamera camera in Views.Local.Cameras)
	    {
		    if (camera.CheckXInActiveRegion(position)) return;
	    }

	    ActiveObjects.Remove(commonObject);
	    commonObject.SetProcess(false);
	    
	    var respawnPosition = (int)commonObject.Spawner.Position.X;
	    foreach (ICamera camera in Views.Local.Cameras)
	    {
		    if (!camera.CheckXInActiveRegion(respawnPosition)) continue;
		    _waitingObjects.Add(commonObject);
		    commonObject.Hide();
		    return;
	    }

	    commonObject.Reset();
	    commonObject.SetProcess(false);
	    commonObject.Hide();
    }
    
    private void ResetYObject(BaseObject commonObject)
    {
	    var position = (int)commonObject.Position.Y;
	    foreach (ICamera camera in Views.Local.Cameras)
	    {
		    if (camera.CheckYInActiveRegion(position)) return;
	    }

	    var respawnPosition = (int)commonObject.Spawner.Position.Y;
	    foreach (ICamera camera in Views.Local.Cameras)
	    {
		    if (!camera.CheckYInActiveRegion(respawnPosition)) continue;
		    ActiveObjects.Remove(commonObject);
		    commonObject.SetProcess(false);
		    commonObject.Hide();
		    return;
	    }
	    
	    commonObject.Spawner.Reset();
    }
}
