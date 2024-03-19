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
		return index > 0 ? FrameworkData.TilesData.Angles[index % TileLimit] : float.NaN;
	}

	public static TilesData LoadTileDataBinary(string anglesFileName, string heightsFileName, string widthsFileName)
	{
		var widths = new byte[TileLimit][];
		var heights = new byte[TileLimit][];
		var angles = new float[TileLimit];

		LoadAngleArray(angles, OpenBinaryByName(anglesFileName));
		LoadCollisionArrays(heights, OpenBinaryByName(heightsFileName));
		LoadCollisionArrays(widths, OpenBinaryByName(widthsFileName));

		return new TilesData(heights, widths, angles);
	}

	public static TilesData GenerateTileData(Image tileMap, byte[] angleData, 
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
		
		return new TilesData(heights, widths, angles);
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

	public static (int, float) FindClosestTile(bool isVertical, 
		Vector2I position1, Vector2I position2, Direction direction, TileLayers type, 
		CollisionTileMap tileMap, TileLayerBehaviours tileLayerBehaviours = TileLayerBehaviours.Floor)
	{
		(int distance1, float angle1) = FindTile(isVertical, position1, direction, type, tileMap, tileLayerBehaviours);
		(int distance2, float angle2) = FindTile(isVertical, position2, direction, type, tileMap, tileLayerBehaviours);
		
		if (isVertical && SharedData.CdTileFixes && direction == Direction.Positive 
		    && distance1 == 0 && distance2 == 0 && angle1 is <= 90f and > 22.5f)
		{
			return (distance1, 360f);
		}
		
		return distance1 <= distance2 ? (distance1, angle1) : (distance2, angle2);
	}
	
	public static int FindClosestDistance(bool isVertical, 
		Vector2I position1, Vector2I position2, Direction direction, TileLayers type, 
		CollisionTileMap tileMap, TileLayerBehaviours tileLayerBehaviours = TileLayerBehaviours.Floor)
	{
		int distance1 = FindTileData(isVertical, position1, direction, type, tileMap, tileLayerBehaviours).Item1;
		int distance2 = FindTileData(isVertical, position2, direction, type, tileMap, tileLayerBehaviours).Item1;
		return distance1 <= distance2 ? distance1 : distance2;
	}
	
	public static (int, float) FindTile(bool isVertical, Vector2I position, Direction direction, 
		TileLayers type, CollisionTileMap tileMap, TileLayerBehaviours tileLayerBehaviours = TileLayerBehaviours.Floor)
	{
		(int distance, FoundTileData tileData) = 
			FindTileData(isVertical, position, direction, type, tileMap, tileLayerBehaviours);
		
		// Return both the distance and the angle
		return (distance, GetTileAngle(tileData, isVertical, direction));
	}
	
	public static int FindDistance(bool isVertical, Vector2I position, Direction direction, 
		TileLayers type, CollisionTileMap tileMap, TileLayerBehaviours tileLayerBehaviours = TileLayerBehaviours.Floor)
	{
		return FindTileData(isVertical, position, direction, type, tileMap, tileLayerBehaviours).Item1;
	}

	private static (int, FoundTileData) FindTileData(bool isVertical, Vector2I position, Direction direction, 
		TileLayers type, CollisionTileMap tileMap, TileLayerBehaviours tileLayerBehaviours)
	{
		// Get tile layer id
		var tileLayerId = (ushort)type;
	    
		// Return empty data if no tile data was found
		if (type == TileLayers.None || tileMap == null || tileMap.GetLayersCount() < tileLayerId)
		{
			return (DoubleTileSize, null);
		}

		// If above the room, use topmost valid level collision
		if (!isVertical)
		{
			position.Y = Math.Max(0, position.Y);
		}
		
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
		int shift;
		var tileSearcher = new TileSearcher(isVertical, position, tileMap, tileLayerId, direction, tileLayerBehaviours);
		FoundTileData tileData = tileSearcher.Search(shift = 0);

		if (tileData.Size == 0 || !tileData.IsValid)
		{
			// If no width found or tile is invalid, get a further tile
			shift = TileSize;
			tileData = tileSearcher.Search(shift);

			if (!tileData.IsValid)
			{
				return (DoubleTileSize, null);
			}
		}
		else if (tileData.Size == TileSize)
		{
			// If width found is TileSize and tile is valid, get a closer tile
			shift = -TileSize;
			FoundTileData newTileData = tileSearcher.Search(shift);

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

		// Calculate distance to the edge of the found tile
		int distance = CalculateDistance(tileData.Size, sign, shift, isVertical ? position.Y : position.X);

		// Return both the distance and the tileData
		return (distance, tileData);
	}

	private static int CalculateDistance(byte size, int sign, int shift, int distancePosition)
	{
		const int tileSizeMinusOne = TileSize - 1;
		int distance = sign * (distancePosition / TileSize * TileSize - distancePosition) + shift - size;
		return sign == 1 ? distance + tileSizeMinusOne : distance;
	}

	private static float GetTileAngle(FoundTileData tileData, bool isVertical, Direction direction)
	{
		if (tileData == null) return float.NaN;
		float rawAngle = GetRawTileAngle(tileData.Index);

		if (float.IsNaN(rawAngle)) return rawAngle;
		
		float angle;
		if (!Mathf.IsEqualApprox(rawAngle, (float)Circle.Full))
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
		
		// A fix from Sonic CD 1996's PC release. If tile angle is in the lower half, we assume its bottom is flat, 
		// so in case it is flipped, we should treat it as a flat ground
		if (!isVertical || !SharedData.CdTileFixes || direction != Direction.Positive) return angle;
		if (rawAngle is > 90f and <= 270f || !tileData.Transforms.IsFlipped) return angle;
		tileData.Size = TileSize;
		return 360f;
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
