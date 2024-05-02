using System;
using Godot;
using static OrbinautFramework3.Framework.Constants;

namespace OrbinautFramework3.Framework.Tiles;

public class TileCollider
{
	private const string BinariesPath = "res://Collisions/Binaries/";
	private static readonly TilesData TilesData;
	
	static TileCollider()
	{
		TilesData = TilesData.LoadTileDataBinary(BinariesPath,
			"angles_tsz",
			"heights_tsz",
			"widths_tsz");
	}
	
    public Vector2I Position { set => _position = value; }
    public TileBehaviours TileBehaviour { set => _tileBehaviour = value; }

    public TileLayers LayerType
    {
	    set
	    {
		    _tileMap = value switch
		    {
			    TileLayers.Main => Scene.Local.CollisionTileMapMain,
			    TileLayers.Secondary => Scene.Local.CollisionTileMapSecondary,
			    _ => null
		    };
	    }
    }
    
    private Vector2I _position;
    private TileBehaviours _tileBehaviour;
    private CollisionTileMap _tileMap;
    private bool _isVertical;
    private Direction _direction;

    public void SetData(int x, int y, TileLayers type, TileBehaviours tileBehaviour = TileBehaviours.Floor)
    {
        Position = new Vector2I(x, y);
        LayerType = type;
        TileBehaviour = tileBehaviour;
    }

    public (int, float) FindTile(int x, int y, bool isVertical, Direction direction)
    {
	    _isVertical = isVertical;
	    _direction = direction;
        return FindTile(_position + new Vector2I(x, y));
    }
    
    public int FindDistance(int x, int y, bool isVertical, Direction direction)
    {
	    _isVertical = isVertical;
	    _direction = direction;
        return FindTileData(_position + new Vector2I(x, y)).Item1;
    }

    public (int, float) FindClosestTile(int x1, int y1, int x2, int y2, bool isVertical, Direction direction)
    {
	    _isVertical = isVertical;
	    _direction = direction;
	    
	    (int distance1, float angle1) = FindTile(_position + new Vector2I(x1, y1));
	    (int distance2, float angle2) = FindTile(_position + new Vector2I(x2, y2));
	    return distance1 <= distance2 ? (distance1, angle1) : (distance2, angle2);
    }
    
    public int FindClosestDistance(int x1, int y1, int x2, int y2,  bool isVertical, Direction direction)
    {
	    _isVertical = isVertical;
	    _direction = direction;
	    int distance1 = FindTileData(_position + new Vector2I(x1, y1)).Item1;
	    int distance2 = FindTileData(_position + new Vector2I(x2, y2)).Item1;
	    return distance1 <= distance2 ? distance1 : distance2;
    }

    private static float GetRawTileAngle(int index)
	{
		return index > 0 ? TilesData.Angles[index % TileLimit] : float.NaN;
	}
	
	private (int, float) FindTile(Vector2I position)
	{
		(int distance, FoundTileData tileData) = FindTileData(position);
		
		// Return both the distance and the angle
		return (distance, GetTileAngle(tileData));
	}
	
	private (int, FoundTileData) FindTileData(Vector2I position)
	{
		// Return empty data if no tile data was found
		if (_tileMap == null) return (DoubleTileSize, default);

		// If above the room, use topmost valid level collision
		if (!_isVertical)
		{
			position.Y = Math.Max(0, position.Y);
		}
		
		// Set the direction as an integer (-1 for leftwards, 1 for rightwards)
		var sign = (sbyte)_direction;
		
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
		FoundTileData tileData = Search(shift = 0, position);

		if (tileData.Size == 0 || !tileData.IsValid)
		{
			// If no width found or tile is invalid, get a further tile
			shift = TileSize;
			tileData = Search(shift, position);

			if (!tileData.IsValid) return (DoubleTileSize, default);
		}
		else if (tileData.Size == TileSize)
		{
			// If width found is TileSize and tile is valid, get a closer tile
			shift = -TileSize;
			FoundTileData newTileData = Search(shift, position);

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
		int distance = CalculateDistance(tileData.Size, sign, shift, 
			_isVertical ? position.Y : position.X);

		// Return both the distance and the tileData
		return (distance, tileData);
	}

	private static int CalculateDistance(byte size, int sign, int shift, int distancePosition)
	{
		const int tileSizeMinusOne = TileSize - 1;
		int distance = sign * (distancePosition / TileSize * TileSize - distancePosition) + shift - size;
		return sign == 1 ? distance + tileSizeMinusOne : distance;
	}

	private float GetTileAngle(FoundTileData tileData)
	{
		if (!tileData.IsValidAngle) return float.NaN;
		float rawAngle = GetRawTileAngle(tileData.Index);

		if (float.IsNaN(rawAngle)) return rawAngle;
		
		float angle;
		if (!Mathf.IsEqualApprox(rawAngle, (float)Angles.Circle.Full))
		{
			angle = Angles.TransformTileAngle(rawAngle, tileData.Transforms);
		}
		else if (_isVertical)
		{
			angle = _direction == Direction.Positive ? (float)Angles.Circle.Full : (float)Angles.Circle.Half;
			
			// Reset height if the tile was found from the opposite side. This only works correctly
			// with originals' tile sets since we can't pre-determine if the tile is flipped by default or not
			if (_direction == Direction.Positive == tileData.Transforms.IsFlipped)
			{
				tileData.Size = TileSize;
			}
		}
		else
		{
			angle = _direction == Direction.Positive ? (float)Angles.Circle.Quarter : (float)Angles.Circle.ThreeQuarters;
		}
		
		return angle;
	}
	
	private FoundTileData Search(int shift, Vector2I position)
	{
		Vector2I shiftedPosition = position;
		if (_isVertical)
		{
			shiftedPosition.Y += shift * (int)_direction;
		}
		else
		{
			shiftedPosition.X += shift * (int)_direction;
		}
        
		Vector2I mapPosition = _tileMap.LocalToMap(shiftedPosition);
		int index = _tileMap.GetTileIndex(_tileMap.GetCellAtlasCoords(mapPosition));
		var transforms = new TileTransforms(_tileMap.GetCellAlternativeTile(mapPosition));
		byte size = GetTileCollision(shiftedPosition, index, transforms);
		bool isValid = GetTileValidity(index, _direction, _tileBehaviour);

		return new FoundTileData(index, transforms, isValid, size);
	}
	
    private byte GetTileCollision(Vector2I position, int index, TileTransforms tileTransforms)
    {
        index %= TileLimit;
        if (index <= 0) return 0;
        var collisionIndex = (byte)((_isVertical ? position.X : position.Y) % TileSize);
	
        if (_isVertical && tileTransforms.IsMirrored || !_isVertical && tileTransforms.IsFlipped)
        {
            collisionIndex = (byte)(TileSize - 1 - collisionIndex);
        }
		
        return tileTransforms.IsRotated ^ _isVertical ? 
            TilesData.Heights[index][collisionIndex] : 
            TilesData.Widths[index][collisionIndex];
    }
    
    private bool GetTileValidity(int index, Direction direction, TileBehaviours tileBehaviours)
    {
        return (index / TileLimit) switch
        {
            2 => !CheckTileValidity(direction, tileBehaviours),
            1 => CheckTileValidity(direction, tileBehaviours),
            _ => true
        };
    }

    private bool CheckTileValidity(Direction direction, TileBehaviours tileBehaviours)
    {
        return tileBehaviours switch
        {
            TileBehaviours.Floor => _isVertical && direction == Direction.Positive,
            TileBehaviours.Ceiling => _isVertical && direction == Direction.Negative,
            TileBehaviours.RightWall => !_isVertical && direction == Direction.Positive,
            TileBehaviours.LeftWall => !_isVertical && direction == Direction.Negative,
            _ => false
        };
    }
}
