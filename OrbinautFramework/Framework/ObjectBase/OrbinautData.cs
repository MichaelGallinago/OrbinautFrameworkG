using System;
using Godot;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player;
using OrbinautFramework3.Objects.Player.Data;
using static OrbinautFramework3.Framework.Constants;

namespace OrbinautFramework3.Framework.ObjectBase;

[GlobalClass]
public partial class OrbinautData : Resource
{
	public Vector2 PreviousPosition { get; set; }

	public bool CheckPushCollision(PlayerData player)
	{
		//TODO: debug collision
		/*
		if _do_debug
		{
			var _left = x - _rx + _ox;
			var _right = x + _rx + _ox;
			var _width = 4;
						
			ds_list_add(_ds_list, _left, y - _ry + _oy, _left + _width, y + _ry + _oy, _push_colour);
			ds_list_add(_ds_list, _right - _width, y - _ry + _oy, _right, y + _ry + _oy, _push_colour);
		}
		*/
		
		return player.PushObjects.Contains(this);
	}
}
