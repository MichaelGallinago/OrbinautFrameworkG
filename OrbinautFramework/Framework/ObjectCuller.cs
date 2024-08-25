using System;
using System.Collections.Generic;
using Godot;
using OrbinautFramework3.Framework.View;

namespace OrbinautFramework3.Framework;

public class ObjectCuller
{
	public static ObjectCuller Local => Scene.Instance.Culler;
	
	private bool _isCullToggled = true;
	private readonly HashSet<ICullable> _hiddenObjectsInView = [];
	private readonly HashSet<ICullable> _stoppedObjects = [];
	public HashSet<ICullable> ActiveObjects { get; } = [];
	
	public void RemoveFromCulling(ICullable target)
	{
		if (ActiveObjects.Remove(target)) return;
		_stoppedObjects.Remove(target);
	}

	public void AddToCulling(ICullable target) => ActiveObjects.Add(target);
	
	public void EarlyCull()
	{
		if (StopAllObjets()) return;
		
		StopObjectsByBehaviour();
		
		if (_isCullToggled)
		{
			ResumeRegions(Views.Instance.Cameras);
			_isCullToggled = false;
			return;
		}

		ResumeRegions(Views.Instance.GetCamerasWithUpdatedRegions());
	}

    private bool StopAllObjets()
    {
	    if (Scene.Instance.State != Scene.States.Paused) return false;
	    
	    foreach (ICullable target in ActiveObjects)
	    {
		    _stoppedObjects.Add(target);
		    target.SetProcess(false);
	    }

	    ActiveObjects.Clear();

	    _isCullToggled = true;
	    return true;
    }
    
    private void ResumeRegions(ReadOnlySpan<ICamera> cameras)
    {
	    if (cameras.Length == 0) return;

	    foreach (ICullable target in _stoppedObjects)
	    {
		    var position = (Vector2I)target.Position;
		    foreach (ICamera camera in cameras)
		    {
			    if (!camera.CheckPositionInActiveRegion(position)) continue;
				_stoppedObjects.Remove(target);
			    ActiveObjects.Add(target);
			    target.SetProcess(true);
			    break;
		    }
	    }
    }
    
    private void StopObjectsByBehaviour()
    {
	    foreach (ICullable target in _hiddenObjectsInView)
	    {
		    var position = (Vector2I)target.Position;
		    foreach (ICamera camera in Views.Instance.Cameras)
		    {
			    if (camera.CheckPositionInActiveRegion(position)) continue;
			    
			    target.Memento.Reset();
			    _stoppedObjects.Add(target);
			    _hiddenObjectsInView.Remove(target);
			    break;
		    }
	    }
	    
	    foreach (ICullable target in ActiveObjects)
	    {
		    StopObjectByBehaviour(target);
	    }
    }
    
    private void StopObjectByBehaviour(ICullable orbinautData)
    {
	    switch (orbinautData.CullingType)
	    {
		    case ICullable.Types.Delete: DeleteObject(orbinautData); break;
		    case ICullable.Types.Reset: ResetObject(orbinautData); break;
		    case ICullable.Types.ResetX: ResetXObject(orbinautData); break;
		    case ICullable.Types.ResetY: ResetYObject(orbinautData); break;
		    case ICullable.Types.Pause: PauseObject(orbinautData); break;
	    }
    }

    private static void DeleteObject(ICullable target)
    {
	    var position = (Vector2I)target.Position;
	    foreach (ICamera camera in Views.Instance.Cameras)
	    {
		    if (camera.CheckPositionInSafeRegion(position) && target.Position.Y < camera.TargetBoundary.W) return;
	    }
	    
	    target.QueueFree();
    }

    private void ResetObject(ICullable target)
    {
	    var position = (Vector2I)target.Position;
	    foreach (ICamera camera in Views.Instance.Cameras)
	    {
		    if (camera.CheckPositionInActiveRegion(position)) return;
	    }
	    
	    ActiveObjects.Remove(target);
	    target.SetProcess(false);

	    var respawnPosition = (Vector2I)target.Memento.Position;
	    foreach (ICamera camera in Views.Instance.Cameras)
	    {
		    if (!camera.CheckPositionInActiveRegion(respawnPosition)) continue;
		    _hiddenObjectsInView.Add(target);
		    target.Hide();
		    return;
	    }
	    
	    target.Memento.Reset();
	    _stoppedObjects.Add(target);
    }
    
    private void ResetXObject(ICullable target)
    {
	    var position = (int)target.Position.X;
	    foreach (ICamera camera in Views.Instance.Cameras)
	    {
		    if (camera.CheckXInActiveRegion(position)) return;
	    }

	    ActiveObjects.Remove(target);
	    target.SetProcess(false);
	    
	    var respawnPosition = (int)target.Memento.Position.X;
	    foreach (ICamera camera in Views.Instance.Cameras)
	    {
		    if (!camera.CheckXInActiveRegion(respawnPosition)) continue;
		    _hiddenObjectsInView.Add(target);
		    target.Hide();
		    return;
	    }

	    target.Memento.Reset();
	    _stoppedObjects.Add(target);
    }
    
    private void ResetYObject(ICullable target)
    {
	    var position = (int)target.Position.Y;
	    foreach (ICamera camera in Views.Instance.Cameras)
	    {
		    if (camera.CheckYInActiveRegion(position)) return;
	    }
	    
	    ActiveObjects.Remove(target);
	    target.SetProcess(false);

	    var respawnPosition = (int)target.Memento.Position.Y;
	    foreach (ICamera camera in Views.Instance.Cameras)
	    {
		    if (!camera.CheckYInActiveRegion(respawnPosition)) continue;
		    _hiddenObjectsInView.Add(target);
		    target.Hide();
		    return;
	    }
	    
	    target.Memento.Reset();
	    _stoppedObjects.Add(target);
    }

    private void PauseObject(ICullable target)
    {
	    var position = (Vector2I)target.Position;
	    foreach (ICamera camera in Views.Instance.Cameras)
	    {
		    if (camera.CheckPositionInActiveRegion(position)) return;
	    }
	    
	    target.SetProcess(false);
	    ActiveObjects.Remove(target);
	    _stoppedObjects.Add(target);
    }
}
