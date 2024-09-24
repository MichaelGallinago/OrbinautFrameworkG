using System.IO;
using Godot;
using static OrbinautFrameworkG.Framework.Constants;

namespace OrbinautFrameworkG.Framework.Tiles;

public readonly struct TilesData(byte[][] heights, byte[][] widths, float[] angles)
{
    public readonly byte[][] Heights = heights;
    public readonly byte[][] Widths = widths;
    public readonly float[] Angles = angles;
    
    public static TilesData LoadTileDataBinary(string binaryPath, 
	    string anglesFileName, string heightsFileName, string widthsFileName)
	{
		var widths = new byte[TileLimit][];
		var heights = new byte[TileLimit][];
		var angles = new float[TileLimit];

		binaryPath = ProjectSettings.GlobalizePath(binaryPath);

		LoadAngleArray(angles, OpenBinaryByName(binaryPath, anglesFileName));
		LoadCollisionArrays(heights, OpenBinaryByName(binaryPath, heightsFileName));
		LoadCollisionArrays(widths, OpenBinaryByName(binaryPath, widthsFileName));

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
		angles[0] = 0f;

		FillCollisionDataFromTileMap(tileMap, heights, widths, offset, cellSize);
		FillAnglesFromAngleData(angleData, angles);
		
		return new TilesData(heights, widths, angles);
	}
	
	private static byte[] OpenBinaryByName(string binaryPath, string fileName)
	{
		return File.ReadAllBytes($"{binaryPath}{fileName}.bin");
	}

	private static void FillCollisionDataFromTileMap(Image tileMap, 
		byte[][] heights, byte[][] widths, Vector2I offset, Vector2I cellSize)
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

	private static void FillAnglesFromAngleData(byte[] angleData, float[] angles)
	{
		if (angleData.Length == 0)
		{
			for (var j = 1; j < TileLimit; j++)
			{
				angles[j] = 0f;
			}
			return;
		}

		var i = 1;
		for (; i <= angleData.Length; i++)
		{
			angles[i] = Tiles.Angles.GetFloatAngle(angleData[i - 1]);
		}
	
		for (; i < TileLimit; i++)
		{
			angles[i] = 0f;
		}
	}
	private static void LoadCollisionArrays(byte[][] collisions, byte[] fileData)
	{
		if (fileData.Length <= 0) return;
		var i = 0;
		for (; i < fileData.Length / TileSize; i++)
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

	private static void LoadAngleArray(float[] angles, byte[] fileData)
	{
		if (fileData.Length <= 0) return;
		var i = 0;
		for (; i < fileData.Length; i++)
		{
			angles[i] = Tiles.Angles.GetFloatAngle(fileData[i]);
		}

		for (; i < TileLimit; i++)
		{
			angles[i] = 0f;
		}
	}
}