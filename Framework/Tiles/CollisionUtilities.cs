using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using static OrbinautFramework3.Framework.Constants;
using static OrbinautFramework3.Framework.Tiles.Angles;

namespace OrbinautFramework3.Framework.Tiles;

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

	public static TileData GenerateTileData(Image tileMap, byte[] angleData, 
		Vector2I offset = new(), Vector2I separation = new())
	{
		var widths = new byte[TileLimit][];
		var heights = new byte[TileLimit][];
		var angles = new float[TileLimit];
		
		Vector2I cellSize = separation + new Vector2I(TileSize, TileSize);
		
		for (var j = 0; j < TileSize; j++)
		{
			widths[0][j] = 0;
			heights[0][j] = 0;
		}
		angles[0] = 360f;

		FillCollisionDataFromTileMap(tileMap, heights, widths, offset, cellSize);
		FillAnglesFromAngleData(angleData, angles);
		
		return new TileData(heights, widths, angles);
	}

	private static void FillCollisionDataFromTileMap(Image tileMap, 
		IList<byte[]> heights, IList<byte[]> widths, Vector2I offset, Vector2I cellSize)
	{
		for (var i = 1; i < TileLimit; i++)
		{
			Vector2I position = offset + cellSize * new Vector2I(i % TileSize, i / TileSize);

			heights[i] = new byte[TileSize];
			widths[i] = new byte[TileSize];
			
			for (var x = 0; x < TileSize; x++)
			for (var y = 0; y < TileSize; y++)
			{
				if (tileMap.GetPixelv(position + new Vector2I(x, y)).A == 0f) continue;
				heights[i][x]++;
				widths[i][y]++;
			}
		}
	}

	private static void FillAnglesFromAngleData(IReadOnlyList<byte> angleData, IList<float> angles)
	{
		if (angleData.Count == 0)
		{
			for (var j = 1; j < TileLimit; j++)
			{
				angles[j] = 0f;
			}
			return;
		}

		var i = 1;
		for (; i <= angleData.Count; i++)
		{
			angles[i] = GetFloatAngle(angleData[i - 1]);
		}
	
		for (; i < TileLimit; i++)
		{
			angles[i] = 360f;
		}
	}

	public static (sbyte, float?) FindTileTwoPositions(bool isVertical, 
		Vector2I position1, Vector2I position2, Direction direction, TileLayers type, 
		CollisionTileMap tileMap, GroundMode groundMode = GroundMode.Floor)
	{
		(sbyte distance1, float? angle1) = FindTile(isVertical, 
			position1, direction, type, tileMap, groundMode);
		(sbyte distance2, float? angle2) = FindTile(isVertical, 
			position2, direction, type, tileMap, groundMode);
		
		if (isVertical && FrameworkData.CDTileFixes
		               && direction == Direction.Positive && angle1 is not 360f
		               && distance1 == 0 && distance2 == 0 && angle1 is <= 90f and > 22.5f)
		{
			angle1 = 360f;
		}
		
		return distance1 <= distance2 ? (distance1, angle1) : (distance2, angle2);
	}
	
	public static (sbyte, float?) FindTile(bool isVertical, Vector2I position, Direction direction, 
		TileLayers type, CollisionTileMap tileMap, GroundMode groundMode = GroundMode.Floor)
	{
		// Get tile layer id
		var tileLayerId = (ushort)type;
	    
		// Return empty data if no tile data was found
		if (type == TileLayers.None || tileMap.GetLayersCount() < tileLayerId)
		{
			return (TileSize * 2, null);
		}

		// If above the room, use topmost valid level collision
		position.Y = Math.Max(0, position.Y);
		
		// Set the direction as an integer (-1 for leftwards, 1 for rightwards)
		var sign = (sbyte)direction;
		
		// Add check to the debug list
		//TODO: debug
		/*
		if (global.debug_collision)
		{
			ds_list_add(c_engine.collision.ds_sensors, position.X, position.Y, position.X - 
				Math.Floor(sprite_get_width(sprite_index) / 4) * sign, position.Y, sign == 1 ? 0x5961E9 : 0xF84AEA);
		}
		*/
		
		// Get tile at position
		sbyte shift;
		var tileSearcher = new TileSearcher(isVertical, position, tileMap, tileLayerId, direction, groundMode);
		FoundTileData tileData = tileSearcher.Search(shift = 0);
		
		// If no width found or tile is invalid, get a further tile
		if (tileData.Size == 0 || !tileData.IsValid)
		{
			tileData = tileSearcher.Search(TileSize);

			if (!tileData.IsValid)
			{
				return (TileSize * 2, null);
			}
		}
		
		// If width found is 16 and tile is valid, get a closer tile
		else if (tileData.Size == TileSize)
		{
			FoundTileData newTileData = tileSearcher.Search(shift = -TileSize);

			// If no width found or tile is invalid, return to back to the initial tile
			if (newTileData.Size == 0 || !newTileData.IsValid)
			{
				shift = 0;
			}
			else
			{
				tileData = newTileData;
			}
		}

		// Get tile angle
		float rawAngle = GetRawTileAngle(tileData.Index);
		float angle;
		if (rawAngle is not (float)Circle.Full)
		{
			angle = TransformTileAngle(rawAngle, tileData.Transforms);
		}
		else if (isVertical)
		{
			angle = (float)(direction == Direction.Positive ? Circle.Full : Circle.Half);
		}
		else
		{
			angle = (float)(direction == Direction.Positive ? Circle.Quarter : Circle.ThreeQuarters);
		}
		
		// Run an additional check from CD'96
		if (isVertical && FrameworkData.CDTileFixes && direction == Direction.Positive)
		{
			// If tile angle is in the bottom half, we assume it's bottom is flat, so in case it is flipped
			// we should treat it as a flat ground
			if (rawAngle is <= 90f or > 270f && tileData.Transforms.IsFlipped)
			{
				tileData.Size = TileSize;
				angle = 360f;
			}
		}

		// Calculate distance to the edge of the found tile
		int distancePosition = isVertical ? position.Y : position.X;
		int distance = ((distancePosition + shift * sign) / TileSize * TileSize - distancePosition) * sign - tileData.Size;
		if (direction == Direction.Positive)
		{
			distance += TileSize - 1;
		}
		
		// Return both the distance and the angle
		return ((sbyte)distance, angle);
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

	private static byte[] OpenBinaryByName(string fileName) => File.ReadAllBytes($"{BinariesPath}{fileName}.bin");
}
