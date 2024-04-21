using System;
using Godot;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.View;

namespace OrbinautFramework3.Framework;

public class ObjectCuller
{
	private bool _isCullToggled;
	
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
	    if (!FrameworkData.IsPaused && FrameworkData.UpdateObjects) return false;

	    foreach (BaseObject baseObject in BaseObject.ActiveObjects)
	    {
		    BaseObject.StoppedObjects.Add(baseObject);
		    baseObject.SetProcess(false);
	    }

	    BaseObject.ActiveObjects.Clear();

	    _isCullToggled = true;
	    return true;
    }
    
    private static void ResumeRegions(ReadOnlySpan<ICamera> cameras)
    {
	    if (cameras.Length == 0) return;

	    foreach (BaseObject baseObject in BaseObject.StoppedObjects)
	    {
		    var position = (Vector2I)baseObject.Position;
		    foreach (ICamera camera in cameras)
		    {
			    if (!camera.CheckPositionInActiveRegion(position)) continue;
			    BaseObject.StoppedObjects.Remove(baseObject);
			    BaseObject.ActiveObjects.Add(baseObject);
			    baseObject.SetProcess(true);
			    break;
		    }
	    }
    }
    
    private static void StopObjectsByBehaviour()
    {
	    foreach (BaseObject baseObject in BaseObject.ActiveObjects)
	    {
		    baseObject.InteractData.IsInteract = true;
		    StopObjectByBehaviour(baseObject);
	    }
    }
    
    private static void StopObjectByBehaviour(BaseObject commonObject)
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

    private static void ResetObject(BaseObject commonObject)
    {
	    var position = (Vector2I)commonObject.Position;
	    foreach (ICamera camera in Views.Local.Cameras)
	    {
		    if (camera.CheckPositionInActiveRegion(position)) return;
	    }

	    var respawnPosition = (Vector2I)commonObject.CullingData.Position;
	    foreach (ICamera camera in Views.Local.Cameras)
	    {
		    if (!camera.CheckPositionInActiveRegion(respawnPosition)) continue;
		    commonObject.Position = Vector2.One * sbyte.MinValue;
		    commonObject.Visible = false;
	    }
	    
	    commonObject.Reset();
    }
    
    private static void ResetXObject(BaseObject commonObject)
    {
	    var position = (int)commonObject.Position.X;
	    foreach (ICamera camera in Views.Local.Cameras)
	    {
		    if (camera.CheckXInActiveRegion(position)) return;
	    }

	    var respawnPosition = (int)commonObject.CullingData.Position.X;
	    foreach (ICamera camera in Views.Local.Cameras)
	    {
		    if (!camera.CheckXInActiveRegion(respawnPosition)) continue;
		    commonObject.Position = Vector2.One * sbyte.MinValue;
		    commonObject.Visible = false;
	    }
	    
	    commonObject.Reset();
    }
    
    private static void ResetYObject(BaseObject commonObject)
    {
	    var position = (int)commonObject.Position.Y;
	    foreach (ICamera camera in Views.Local.Cameras)
	    {
		    if (camera.CheckYInActiveRegion(position)) return;
	    }

	    var respawnPosition = (int)commonObject.CullingData.Position.Y;
	    foreach (ICamera camera in Views.Local.Cameras)
	    {
		    if (!camera.CheckYInActiveRegion(respawnPosition)) continue;
		    commonObject.Position = Vector2.One * sbyte.MinValue;
		    commonObject.Visible = false;
	    }
	    
	    commonObject.Reset();
    }
}
