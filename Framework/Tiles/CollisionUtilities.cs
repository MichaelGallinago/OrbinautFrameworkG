using Godot;
using System.Collections.Generic;
using System.IO;
using static Constants;
using static Angles;

public static class CollisionUtilities
{
	private static readonly string BinariesPath = "res://Collisions/Binaries/";
	
	static CollisionUtilities()
	{
		BinariesPath = ProjectSettings.GlobalizePath(BinariesPath);
	}

	public static float GetRawTileAngle(int index)
	{
		return index > 0 ? FrameworkData.TileData.Angles[index % TileLimit] : 360f;
	}

	public static TileData LoadTileDataBinary(string anglesFileName, string heightsFileName, string widthsFileName)
	{
		var widths = new byte[TileLimit][];
		var heights = new byte[TileLimit][];
		var angles = new float[TileLimit];

		LoadAngleArray(angles, OpenBinaryByName(anglesFileName));
		LoadCollisionArrays(heights, OpenBinaryByName(heightsFileName));
		LoadCollisionArrays(widths, OpenBinaryByName(widthsFileName));

		return new TileData(heights, widths, angles);
	}

	public static (sbyte, float?) FindTileTwoPositions(bool isVertical, 
		Vector2I position1, Vector2I position2, Direction direction, TileLayer tileLayerType, 
		CollisionTileMap tileMap, GroundMode groundMode = GroundMode.Floor)
	{
		(sbyte distance1, float? angle1) = FindTile(isVertical, 
			position1, direction, tileLayerType, tileMap, groundMode);
		(sbyte distance2, float? angle2) = FindTile(isVertical, 
			position2, direction, tileLayerType, tileMap, groundMode);
		
		if (isVertical && FrameworkData.CDTileFixes
		    && direction == Direction.Positive && angle1 is not 360f
		    && distance1 == 0 && distance2 == 0 && angle1 is <= 90f and > 22.5f)
		{
			angle1 = 360f;
		}
		
		return distance1 <= distance2 ? (distance1, angle1) : (distance2, angle2);
	}
	
	public static (sbyte, float?) FindTile(bool isVertical, Vector2I position, Direction direction, 
		TileLayer tileLayerType, CollisionTileMap tileMap, GroundMode groundMode = GroundMode.Floor)
    {
	    // Get tile layer id
	    var tileLayerId = (ushort)tileLayerType;
	    
	    // Return empty data if no tile data was found
		if (tileLayerType == TileLayer.None || tileMap.GetLayersCount() < tileLayerId)
		{
			return (TileSize * 2, null);
		}

		// If above the room, use topmost valid level collision
		position.Y = Mathf.Max(0, position.Y);

		// Set the direction as an integer (-1 for leftwards, 1 for rightwards)
		var sign = (sbyte)direction;
		
		// Add check to the debug list
		//TODO: debug
		/*
		if (global.debug_collision)
		{
			ds_list_add(c_engine.collision.ds_sensors, position.X, position.Y, position.X - 
				Mathf.Floor(sprite_get_width(sprite_index) / 4) * sign, position.Y, sign == 1 ? 0x5961E9 : 0xF84AEA);
		}
		*/
		
		// Get tile at position
		var shift = 0;
		Vector2I mapPosition = tileMap.LocalToMap(position);
		int index = tileMap.GetTileIndex(tileMap.GetCellAtlasCoords(tileLayerId, mapPosition));
		var transforms = new TileTransforms(tileMap.GetCellAlternativeTile(tileLayerId, mapPosition));
		byte size = GetTileCollision(isVertical, position, index, transforms);
		bool isValid = GetTileValidity(index, isVertical, direction, groundMode);
		
		// Remember this tile for later use
		Vector2I positionBuffer = position;
		Vector2I mapPositionBuffer = mapPosition;
		int indexBuffer = index;
		byte sizeBuffer = size;
		TileTransforms transformsBuffer = transforms;

		// If no width found or tile is invalid, get a further tile
		if (size == 0 || !isValid)
		{
			shift = TileSize;
			if (isVertical)
			{
				position.Y += shift * sign;	
			}
			else
			{
				position.X += shift * sign;	
			}
			mapPosition = tileMap.LocalToMap(position);
			index = tileMap.GetTileIndex(tileMap.GetCellAtlasCoords(tileLayerId, mapPosition));
			transforms = new TileTransforms(tileMap.GetCellAlternativeTile(tileLayerId, mapPosition));
			size = GetTileCollision(isVertical, position, index, transforms);
			isValid = GetTileValidity(index, isVertical, direction, groundMode);
			position = positionBuffer;
			mapPosition = mapPositionBuffer;
			
			// If tile is invalid, return empty data
			if (!isValid)
			{
				return (TileSize * 2, null);
			}
		}
		
		// If width found is 16 and tile is valid, get a closer tile
		else if (size == TileSize)
		{
			shift = -TileSize;
			if (isVertical)
			{
				position.Y += shift * sign;	
			}
			else
			{
				position.X += shift * sign;	
			}
			mapPosition = tileMap.LocalToMap(position);
			index = tileMap.GetTileIndex(tileMap.GetCellAtlasCoords(tileLayerId, mapPosition));
			transforms = new TileTransforms(tileMap.GetCellAlternativeTile(tileLayerId, mapPosition));
			size = GetTileCollision(isVertical, position, index, transforms);
			isValid = GetTileValidity(index, isVertical, direction, groundMode);
			position = positionBuffer;
			mapPosition = mapPositionBuffer;

			// If no width found or tile is invalid, return to back to the initial tile
			if (size == 0 || !isValid)
			{
				shift = 0;
				index = indexBuffer;
				size = sizeBuffer;
				transforms = transformsBuffer;
			}
		}

		// Get tile angle
		float rawAngle = GetRawTileAngle(index);
		float angle;
		if (rawAngle is not (float)Circle.Full)
		{
			angle = TransformTileAngle(rawAngle, transforms);
		}
		else if (isVertical)
		{
			angle = (float)(sign == 1 ? Circle.Full : Circle.Half);
		}
		else
		{
			angle = (float)(sign == 1 ? Circle.Quarter : Circle.ThreeQuarters);
		}
		
		// Run an additional check from CD'96
		if (isVertical && FrameworkData.CDTileFixes && sign == 1)
		{
			// If tile angle is in the bottom half, we assume it's bottom is flat, so in case it is flipped
			// we should treat it as a flat ground
			if (rawAngle is <= 90f or > 270f && transforms.IsFlipped)
			{
				size = TileSize;
				angle = 360f;
			}
		}

		// Calculate distance to the edge of the found tile
		int distancePosition = isVertical ? position.Y : position.X;
		int distance = ((distancePosition + shift * sign) / TileSize * TileSize - distancePosition) * sign - size;
		if (sign == 1)
		{
			distance += TileSize - 1;
		}
		
		// Return both the distance and the angle
		return ((sbyte)distance, angle);
    }
	
	private static bool GetTileValidity(int index, bool isVertical, Direction direction, GroundMode groundMode)
	{
		return index switch
		{
			>= TileLimit * 2 when isVertical => groundMode switch
			{
				GroundMode.Floor => direction == Direction.Negative,
				GroundMode.Ceiling => direction == Direction.Positive,
				GroundMode.RightWall => true,
				GroundMode.LeftWall => true,
				_ => direction == Direction.Negative
			},
			>= TileLimit when isVertical => groundMode switch
			{
				GroundMode.Floor => direction == Direction.Positive,
				GroundMode.Ceiling => direction == Direction.Negative,
				GroundMode.RightWall => false,
				GroundMode.LeftWall => false,
				_ => direction == Direction.Positive
			},
			>= TileLimit * 2 => groundMode switch
			{
				GroundMode.Floor => true,
				GroundMode.Ceiling => true,
				GroundMode.RightWall => direction == Direction.Negative,
				GroundMode.LeftWall => direction == Direction.Positive,
				_ => true
			},
			>= TileLimit => groundMode switch
			{
				GroundMode.Floor => false,
				GroundMode.Ceiling => false,
				GroundMode.RightWall => direction == Direction.Positive,
				GroundMode.LeftWall => direction == Direction.Negative,
				_ => false
			},
			_ => true
		};
	}
    
	private static byte GetTileCollision(bool isHeight, Vector2I position, int index, TileTransforms tileTransforms)
	{
		index %= TileLimit;
		if (index <= 0) return 0;
		var collisionIndex = (byte)((isHeight ? position.X : position.Y) % TileSize);
	
		if (tileTransforms.IsMirrored)
		{
			collisionIndex = (byte)(TileSize - 1 - collisionIndex);
		}
		
		return tileTransforms.IsRotated ^ isHeight ?
			FrameworkData.TileData.Heights[index][collisionIndex] : 
			FrameworkData.TileData.Widths[index][collisionIndex];
	}

	private static void LoadCollisionArrays(IList<byte[]> collisions, IReadOnlyList<byte> fileData)
	{
		if (fileData.Count <= 0) return;
		var i = 0;
		for (; i < fileData.Count / TileSize; i++)
		{
			int tileIndex = i * TileSize;
			collisions[i] = new byte[TileSize];
			for (var j = 0; j < TileSize; j++)
			{
				collisions[i][j] = fileData[tileIndex + j];
			}
		}

		for (; i < TileLimit; i++)
		{
			collisions[i] = new byte[TileSize];
			for (var j = 0; j < TileSize; j++)
			{
				collisions[i][j] = 0;
			}
		}
	}

	private static void LoadAngleArray(IList<float> angles, IReadOnlyList<byte> fileData)
	{
		if (fileData.Count <= 0) return;
		var i = 0;
		for (; i < fileData.Count; i++)
		{
			angles[i] = GetFloatAngle(fileData[i]);
		}

		for (; i < TileLimit; i++)
		{
			angles[i] = 0f;
		}
	}

	private static byte[] OpenBinaryByName(string fileName)
	{
		return File.ReadAllBytes($"{BinariesPath}{fileName}.bin");
	}
}
