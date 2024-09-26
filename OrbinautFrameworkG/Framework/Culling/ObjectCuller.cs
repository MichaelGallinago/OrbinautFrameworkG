using System;
using System.Collections.Generic;
using Godot;
using OrbinautFrameworkG.Framework.View;

namespace OrbinautFrameworkG.Framework.Culling;

public class ObjectCuller
{
	private bool _isCullToggled = true;
	private readonly HashSet<ICullable> _hiddenObjectsInView = [];
	private readonly HashSet<ICullable> _pausedObjects = [];
	private readonly HashSet<ICullable> _activeObjects = [];
	
	public void Remove(ICullable target)
	{
		if (_activeObjects.Remove(target)) return;
		_pausedObjects.Remove(target);
	}

	public void Add(ICullable target) => _activeObjects.Add(target);
	
	public void EarlyCull()
	{
		if (PauseAllObjets()) return;
		
		CullObjects();
		
		if (_isCullToggled)
		{
			ResumeRegions(Views.Instance.Cameras);
			_isCullToggled = false;
			return;
		}

		ResumeRegions(Views.Instance.GetCamerasWithUpdatedRegions());
	}

    private bool PauseAllObjets()
    {
	    if (SceneModule.Scene.Instance.State != SceneModule.Scene.States.Paused) return false;
	    
	    foreach (ICullable target in _activeObjects)
	    {
		    _pausedObjects.Add(target);
		    target.SetProcess(false);
	    }

	    _activeObjects.Clear();

	    _isCullToggled = true;
	    return true;
    }
    
    private void ResumeRegions(ReadOnlySpan<ICamera> cameras)
    {
	    if (cameras.Length == 0) return;

	    foreach (ICullable target in _pausedObjects)
	    {
		    var position = (Vector2I)target.Position;
		    foreach (ICamera camera in cameras)
		    {
			    if (!camera.CheckPositionInActiveRegion(position)) continue;
				_pausedObjects.Remove(target);
			    _activeObjects.Add(target);
			    target.SetProcess(true);
			    break;
		    }
	    }
    }
    
    private void CullObjects()
    {
	    foreach (ICullable target in _hiddenObjectsInView)
	    {
		    var position = (Vector2I)target.Position;
		    foreach (ICamera camera in Views.Instance.Cameras)
		    {
			    if (camera.CheckPositionInActiveRegion(position)) continue;
			    
			    target.Memento.Reset();
			    _pausedObjects.Add(target);
			    _hiddenObjectsInView.Remove(target);
			    break;
		    }
	    }
	    
	    foreach (ICullable target in _activeObjects)
	    {
		    CullObjectByBehaviour(target);
	    }
    }
    
    private void CullObjectByBehaviour(ICullable orbinautData) // TODO: culling
    {
	    switch (orbinautData.CullingType)
	    {
		    case ICullable.Types.None or ICullable.Types.PauseOnly or ICullable.Types.Active: break;
		    //case ICullable.Types.Delete: DeleteObject(orbinautData); break;
		    //case ICullable.Types.Reset: ResetObject(orbinautData); break;
		    //case ICullable.Types.Basic: PauseObject(orbinautData); break;
	    }
    }

    private static void RemoveObject(ICullable target)
    {
	    var position = (Vector2I)target.Position;
	    foreach (ICamera camera in Views.Instance.Cameras)
	    {
		    if (camera.CheckPositionInSafeRegion(position) && target.Position.Y < camera.TargetBoundary.W) return;
	    }
	    
	    target.QueueFree();
    }

    private void RespawnObject(ICullable target)
    {
	    var position = (Vector2I)target.Position;
	    foreach (ICamera camera in Views.Instance.Cameras)
	    {
		    if (camera.CheckPositionInActiveRegion(position)) return;
	    }
	    
	    _activeObjects.Remove(target);
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
	    _pausedObjects.Add(target);
    }

    private void PauseObject(ICullable target)
    {
	    var position = (Vector2I)target.Position;
	    foreach (ICamera camera in Views.Instance.Cameras)
	    {
		    if (camera.CheckPositionInActiveRegion(position)) return;
	    }
	    
	    target.SetProcess(false);
	    _activeObjects.Remove(target);
	    _pausedObjects.Add(target);
    }
}
