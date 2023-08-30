using Godot;
using System;

public partial class Camera : Camera2D
{
    private static int[] _shakeData = {
        1, 2, 1, 3, 1, 2, 2, 1, 2, 3, 1, 2, 1, 2, 0, 0,
        2, 0, 3, 2, 2, 3, 2, 2, 1, 3, 0, 0, 1, 0, 1, 3
    };
    
    public Node2D Target { get; set; }
    
    private Vector2 _maxSpeed;
    private Vector2 _speed;
    private Vector2 _position;
    private Vector2 _delay;
    private Vector2 _offset;
    private Vector4I _previousLimit; 
    
    private Vector2 _boundSpeed;
    private Vector4I _bound;

    private Vector2 _shakeOffset;
    private int _shakeTimer;

    public Camera()
    {
        _bound = new Vector4I(LimitTop, LimitBottom, LimitLeft, LimitRight);
        _previousLimit = _bound;
        _maxSpeed = new Vector2(16, 16);

        if (FrameworkData.CheckpointData is not null)
        {
            LimitBottom = FrameworkData.CheckpointData.BottomCameraBound;
        }
    }
    
    public override void _EnterTree()
    {
        FrameworkData.CurrentScene.EndStep += EndStep;
    }

    public override void _ExitTree()
    {
        FrameworkData.CurrentScene.EndStep -= EndStep;
    }

    private void EndStep(double processSpeed)
    {
        float boundSpeed;
		
		if (FrameworkData.UpdateObjects)
		{
			Vector2I size = FrameworkData.ViewSize / 2;
			size.Y -= 16;

			// Get boundary update speed
			boundSpeed = Mathf.Max(2f, _boundSpeed.X);

			if (Target != null && !IsInstanceValid(Target))
			{
				Target = null;
			}
			
			if (Target != null)
			{
				Vector2 targetPosition = (Vector2I)Target.Position - _position;

				var extX = !FrameworkData.CDCamera * 16;
				
				if (targetX > halfWidth)
				{ 
					spd_x = clamp(targetX - halfWidth, 0, spd_x_max);    
				}
				else if (targetX < halfWidth - extX)
				{ 
					spd_x = clamp(targetX - halfWidth + extX, -spd_x_max, 0);  
				}
				else
				{
					spd_x = 0;
				}
				
				if (target.object_index == global.player_obj && target.is_grounded)
				{	
					if (target.is_spinning)
					{
						targetY -= (target.radius_y_normal - target.radius_y);
					}
				
					var _limit = spd_y_max;
					if (abs(target.gsp) < 8)
					{
						_limit = 6;
					}
				
					spd_y = clamp(targetY - halfHeight, -_limit, _limit);
				} 
				else
				{
					if (targetY > halfHeight + 32)
					{
						spd_y = clamp(targetY - halfHeight - 32, 0, spd_y_max);  
					}
					else if (targetY < halfHeight - 32)
					{ 
						spd_y = clamp(targetY - halfHeight + 32, -spd_y_max, 0);  
					} 
					else
					{
						spd_y = 0;
					}
				}
			}
			else
			{
				spd_x = 0;
				spd_y = 0;
			}
		
			if (shake_timer > 0)
			{
				shake_timer--;
			
				var _shake_offset = 1;
				if shake_timer % 2 != 0
				{
					_shake_offset = -1;
				}
			
				shake_x = shake_data[ shake_timer       % 31] * _shake_offset;
				shake_y = shake_data[(shake_timer + 15) % 31] * _shake_offset;
			}
			else
			{
				shake_x = 0;
				shake_y = 0;
			}
		
			if (delay_x == 0)
			{
				_position.X += spd_x;
			}
			else if (delay_x > 0)
			{
				delay_x--;
			}
			
			_position.Y += spd_y;
		}
		
		// Update left boundary
		if (view_x_min_prev != view_x_min)
		{
			bound_left = view_x_min;
		}
		else if (view_x_min < bound_left)
		{	
			if (_position.X >= bound_left)
			{
				view_x_min = bound_left;
			}
			else
			{
				if (_position.X >= view_x_min)
				{
					view_x_min = _position.X;
				}
				
				view_x_min = min(view_x_min + boundSpeed, bound_left);
			}
		}
		else if (view_x_min > bound_left)
		{
			view_x_min = max(bound_left, view_x_min - boundSpeed);
		}
	
		// Update right boundary
		if (view_x_max_prev != view_x_max)
		{
			bound_right = view_x_max;
		}
		else if (view_x_max < bound_right)
		{
			view_x_max = min(view_x_max + boundSpeed, bound_right);
		}
		else if (view_x_max > bound_right)
		{
			var _width = global.render_width;
			
			if (_position.X + _width <= bound_right)
			{
				view_x_max = bound_right;
			}
			else
			{	
				if (_position.X + _width <= view_x_max)
				{
					view_x_max = _position.X + _width;	
				}
				
				view_x_max = max(bound_right, view_x_max - boundSpeed);
			}
		}
	
		// Update top boundary
		if (view_y_min_prev != view_y_min)
		{
			bound_top = view_y_min;
		}
		else if (view_y_min < bound_top)
		{
			if (_position.Y >= bound_top)
			{
				view_y_min = bound_top
			}
			else
			{	
				if (_position.Y >= view_y_min)
				{
					view_y_min = _position.Y
				}
				
				view_y_min = min(view_y_min + boundSpeed, bound_top);
			}
		}
		else if (view_y_min > bound_top)
		{
			view_y_min = max(bound_top, view_y_min - boundSpeed);
		}
		
		// Update bottom boundary
		if (view_y_max_prev != view_y_max)
		{
			bound_bottom = view_y_max;
		}
		else if (view_y_max < bound_bottom)
		{
			view_y_max = min(view_y_max + boundSpeed, bound_bottom);
		}
		else if (view_y_max > bound_bottom)
		{
			var _height = global.render_height;
			
			if (_position.Y + _height <= bound_bottom)
			{
				view_y_max = bound_bottom;
			}
			else
			{
				if (_position.Y + _height <= view_y_max)
				{
					view_y_max = _position.Y + _height;
				}
				
				view_y_max = max(bound_bottom, view_y_max - boundSpeed);
			}
		}
		
		view_x_min_prev = view_x_min;
		view_x_max_prev = view_x_max;
		view_y_min_prev = view_y_min;
		view_y_max_prev = view_y_max;
		
		_position.X = clamp(_position.X + offset_x, view_x_min, view_x_max - global.render_width)  + shake_x;
		_position.Y = clamp(_position.Y + offset_y, view_y_min, view_y_max - global.render_height) + shake_y;
		
		camera_set_view_size(instance, global.render_width + RENDER_BUFFER * 2, global.render_height);
		Position = new Vector2(_position.X - Constants.RenderBuffer, _position.Y);
    }
}
