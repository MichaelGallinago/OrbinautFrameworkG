using Godot;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.View;

namespace OrbinautFramework3.Framework;

public class ObjectCuller
{
	private static readonly Vector2I CullSizeBuffer = new(320, 288);

	private bool _isCullToggled;
	private Vector2I _activeArea;
	private Vector2I _cullSize;

	public ObjectCuller()
	{
		
		SharedData.ViewSizeChanged += viewSize => _cullSize = viewSize + CullSizeBuffer;
	}
	
    public void Cull()
    {
		if (DeactivateAllObjects()) return;

		if (_isCullToggled)
		{
			foreach (BaseObject commonObject in BaseObject.Objects)
			{
				commonObject.SetProcess(true);
			}
			_isCullToggled = false;
		}
		
		// Deactivate or reset objects outside the new active area
		if (Camera.Main == null) return;

	    _activeArea = Camera.Main.GetActiveArea();
		int limitBottom = Camera.Main.LimitBottom;
		
		foreach (BaseObject commonObject in BaseObject.Objects)
		{
			DeactivateObjectsByBehaviour(commonObject, limitBottom);
		
			// Activate objects within the new active area and reset interaction flag for all active objects
			if (commonObject.Position.X < _activeArea.X || commonObject.Position.Y < 0f || 
			    commonObject.Position.X >= _activeArea.Y || commonObject.Position.Y >= limitBottom) continue;
			
			commonObject.SetProcess(true);
			commonObject.InteractData.IsInteract = true;
		}
    }

    private bool DeactivateAllObjects()
    {
	    if (!FrameworkData.IsPaused && FrameworkData.UpdateObjects) return false;
	    
	    foreach (BaseObject commonObject in BaseObject.Objects)
	    {
		    if (commonObject.Culling == BaseObject.CullingType.None) continue;
		    commonObject.SetProcess(false);
	    }

	    _isCullToggled = true;

	    return true;
    }
    
    private void DeactivateObjectsByBehaviour(BaseObject commonObject, int limitBottom)
    {		
	    switch (commonObject.Culling)
	    {
		    case BaseObject.CullingType.None or BaseObject.CullingType.Active: break;
		    case BaseObject.CullingType.Reset: ResetObject(commonObject); break;
		    case BaseObject.CullingType.Delete: DeleteObject(commonObject); break;
		    default: 
			    if (commonObject.Position.X >= _activeArea.X && commonObject.Position.X <= _activeArea.Y) break;
					
			    float respawnX = commonObject.RespawnData.Position.X;
			    if (respawnX >= _activeArea.X && respawnX <= _activeArea.Y) break;
			    commonObject.SetProcess(false);
			    break;
	    }
    }

    private void ResetObject(BaseObject commonObject)
    {
	    var _dist_x = (int)(commonObject.Position.X - _activeArea.X) & -128;
					
	    // We're not too far away, do not check next camera
	    if _dist_x >= 0 && _dist_x <= _cull_width
	    {
		    break;
	    }
					
	    _dist_x = (xstart - _view.coarse_x) & -128;
					
	    // If initial x-position is still in the same horizontal area, "vanish"
	    if _dist_x >= 0 && _dist_x <= _cull_width
	    {
		    _action_flag = 2; continue;
	    }
					
	    _action_flag = 1; continue;
	    
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
