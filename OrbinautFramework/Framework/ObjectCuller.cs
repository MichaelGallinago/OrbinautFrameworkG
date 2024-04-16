using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.View;

namespace OrbinautFramework3.Framework;

public class ObjectCuller
{
	private static readonly Stack<Camera> CamerasWithResumingRegions = [];
	
	private bool _isCullToggled;
	
	public void EarlyCull()
	{
		if (StopAllObjets()) return;
		if (ResumeAllObjects()) return;
        ResumeRegions();
	}
	
    public void LateCull()
    {
	    if (FrameworkData.IsPaused) return;
	    
	    Parallel.ForEach(BaseObject.StoppedObjects.Keys, baseObject =>
	    {
		    baseObject.InteractData.IsInteract = true;
		    
		    ViewStorage.Local.InvokeInCameras(camera =>
		    {
			    //DeactivateObjectsByBehaviour();
		    });
	    });
    }

    private bool StopAllObjets()
    {
	    if (!FrameworkData.IsPaused && FrameworkData.UpdateObjects) return false;

	    Parallel.ForEach(BaseObject.ActiveObjects.Keys, baseObject =>
	    {
		    if (baseObject.Culling == BaseObject.CullingType.None) return;
		    BaseObject.ActiveObjects.Remove(baseObject, out _);
		    BaseObject.StoppedObjects.TryAdd(baseObject, 0);
		    baseObject.SetProcess(false);
	    });

	    _isCullToggled = true;
	    return true;
    }

    private bool ResumeAllObjects()
    {
	    if (!_isCullToggled) return false;
	    
	    Parallel.ForEach(BaseObject.StoppedObjects.Keys, baseObject =>
	    {
		    baseObject.SetProcess(true);
		    BaseObject.ActiveObjects.TryAdd(baseObject, 0);
	    });
	    BaseObject.StoppedObjects.Clear();
	    
	    _isCullToggled = false;
	    return true;
    }
    
    private static void ResumeRegions()
    {
	    ViewStorage.Local.FillStackOfWithResumingRegions(CamerasWithResumingRegions);
	    
	    if (CamerasWithResumingRegions.Count == 0) return;
	    
	    Parallel.ForEach(BaseObject.StoppedObjects.Keys, baseObject =>
	    {
		    foreach (Camera camera in CamerasWithResumingRegions)
		    {
			    if (!camera.CheckPositionInActiveRegion(baseObject.Position)) continue;
			    BaseObject.StoppedObjects.Remove(baseObject, out _);
			    BaseObject.ActiveObjects.TryAdd(baseObject, 0);
			    baseObject.SetProcess(true);
			    break;
		    }
	    });
	    
	    CamerasWithResumingRegions.Clear();
    }
    
    private void DeactivateObjectsByBehaviour(BaseObject commonObject, int limitBottom)
    {		
	    switch (commonObject.Culling)
	    {
		    case BaseObject.CullingType.Reset: ResetObject(commonObject); break;
		    case BaseObject.CullingType.Delete: DeleteObject(commonObject); break;
		    case BaseObject.CullingType.Pause:
			    if (commonObject.Position.X >= _activeArea.X && commonObject.Position.X <= _activeArea.Y) break;
					
			    float respawnX = commonObject.RespawnData.Position.X;
			    if (respawnX >= _activeArea.X && respawnX <= _activeArea.Y) break;
			    commonObject.SetProcess(false);
			    break;
	    }
    }

    private void ResetObject(BaseObject commonObject)
    {
	    int distanceX = (int)(commonObject.Position.X - _activeArea.X) & -128;
					
	    // We're not too far away, do not check next camera
	    if (distanceX >= 0 && distanceX <= _cull_width) return;
					
	    distanceX = (xstart - _view.coarse_x) & -128;
					
	    // If initial x-position is still in the same horizontal area, "vanish"
	    if distanceX >= 0 && distanceX <= _cull_width
	    {
		    _action_flag = 2; continue;
	    }
					
	    _action_flag = 1; continue;
	    /*
	    if (commonObject.Position.X >= _activeArea.X && commonObject.Position.X <= _activeArea.Y) return;
			    
	    float resetX = commonObject.RespawnData.Position.X;
	    if (resetX >= _activeArea.X && resetX <= _activeArea.Y)
	    {
		    commonObject.Position = new Vector2(sbyte.MinValue, sbyte.MinValue);
		    commonObject.Hide();
						
		    return;
	    }
			    
	    commonObject.Reset();
	    commonObject.SetProcess(false);
	    */
    }

    private void DeleteObject(BaseObject commonObject)
    {
	    Vector2 position = commonObject.Position;
	    if (position.X < _activeArea.X || position.X > _activeArea.Y || 
	        position.Y < 0f || position.Y > limitBottom)
	    {
		    commonObject.QueueFree();
	    }
    }
}
