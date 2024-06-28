using System;
using Godot;
using JetBrains.Annotations;
using OrbinautFramework3.Scenes;
using static OrbinautFramework3.Framework.Constants;

namespace OrbinautFramework3.Framework.Tiles;

public struct TileCollider
{
	private const string BinariesPath = "res://Collisions/Binaries/";
	private const int MaxDistance = TileSize * 2;
	private static readonly TilesData TilesData;
	
	//TODO: better setup 
	static TileCollider()
	{
		TilesData = TilesData.LoadTileDataBinary(BinariesPath,
			"angles_tsz",
			"heights_tsz",
			"widths_tsz");
	}
	
    public Vector2I Position { get; set; }
    public TileBehaviours TileBehaviour { set => _tileBehaviour = value; }

    public TileLayers LayerType
    {
	    set
	    {
		    _tileMap = value switch
		    {
			    TileLayers.Main => _scene.CollisionTileMapMain,
			    TileLayers.Secondary => _scene.CollisionTileMapSecondary,
			    _ => null
		    };
	    }
    }
    
    [UsedImplicitly] private IScene _scene;
    
    private bool _isVertical;
    private Direction _direction;
    private Vector2I _searchPosition;
    private CollisionTileMap _tileMap;
    private FoundTileData _foundTileData;
    private TileBehaviours _tileBehaviour;
    
    public void SetData(int x, int y, TileLayers type, TileBehaviours tileBehaviour = TileBehaviours.Floor)
    {
        Position = new Vector2I(x, y);
        LayerType = type;
        TileBehaviour = tileBehaviour;
    }
    
    public void SetData(Vector2I position, TileLayers type, TileBehaviours tileBehaviour = TileBehaviours.Floor)
    {
	    Position = position;
	    LayerType = type;
	    TileBehaviour = tileBehaviour;
    }

    public (int, float) FindTile(int x, int y, bool isVertical, Direction direction)
    {
	    _isVertical = isVertical;
	    _direction = direction;
	    
	    _searchPosition = Position + new Vector2I(x, y);
        return GetTile();
    }
    
    public int FindDistance(int x, int y, bool isVertical, Direction direction)
    {
	    _isVertical = isVertical;
	    _direction = direction;
	    
	    _searchPosition = Position + new Vector2I(x, y);
	    return GetDistance();
    }

    public (int, float) FindClosestTile(int x1, int y1, int x2, int y2, bool isVertical, Direction direction)
    {
	    _isVertical = isVertical;
	    _direction = direction;

	    _searchPosition = Position + new Vector2I(x1, y1);
	    (int distance, float) tile1 = GetTile();
	    
	    _searchPosition = Position + new Vector2I(x2, y2);
	    (int distance, float) tile2 = GetTile();
	    
	    return tile1.distance <= tile2.distance ? tile1 : tile2;
    }
    
    public int FindClosestDistance(int x1, int y1, int x2, int y2, bool isVertical, Direction direction)
    {
	    _isVertical = isVertical;
	    _direction = direction;
	    
	    _searchPosition = Position + new Vector2I(x1, y1);
	    int distance1 = GetDistance();

	    _searchPosition = Position + new Vector2I(x2, y2);
	    int distance2 = GetDistance();
	    
	    return distance1 <= distance2 ? distance1 : distance2;
    }

    private static float GetRawTileAngle(int index)
	{
		return index > 0 ? TilesData.Angles[index % TileLimit] : float.NaN;
	}

	private int GetDistance()
	{
		if (!FindTileData()) return MaxDistance;
		
		ValidateHeight();
		return CalculateDistance();
	}
	
	private (int, float) GetTile()
	{
		if (!FindTileData()) return (MaxDistance, float.NaN);
		
		float angle = GetTileAngle();
		return (CalculateDistance(), angle);
	}
	
	private bool FindTileData()
	{
		// Return empty data if no tile data was found
		if (_tileMap == null) return false;
		
		if (!_isVertical)
		{
			_searchPosition.Y = Math.Max(0, _searchPosition.Y);
		}
		
		// Add check to the debug list
		//TODO: debug
		/*
		if (global.debug_collision)
		{
			var sign = (sbyte)_direction;
			ds_list_add(c_engine.collision.ds_sensors, position.X, position.Y, position.X -
				Math.Floor(sprite_get_width(sprite_index) / 4) * sign, position.Y, sign == 1 ? 0x5961E9 : 0xF84AEA);
		}
		*/
		
		// Get tile at position
		sbyte shift = 0;
		FoundTileData tileData = Search(shift);

		if (tileData.Size == 0 || !tileData.IsValid)
		{
			// If no width found or tile is invalid, get a further tile
			shift = (sbyte)TileSize;
			tileData = Search(shift);

			if (!tileData.IsValid) return false;
		}
		else if (tileData.Size == TileSize)
		{
			// If width found is TileSize and tile is valid, get a closer tile
			shift = -TileSize;
			FoundTileData newTileData = Search(shift);

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

		tileData.Shift = shift;
		_foundTileData = tileData;
		return true;
	}

	private int CalculateDistance()
	{
		int distancePosition = _isVertical ? _searchPosition.Y : _searchPosition.X;
		
		int distance = (int)_direction * (distancePosition / TileSize * TileSize - distancePosition) + 
			_foundTileData.Shift - _foundTileData.Size;
		
		return _direction == Direction.Positive ? distance + (TileSize - 1) : distance;
	}

	private float GetTileAngle()
	{
		if (!_foundTileData.IsValidAngle) return float.NaN;
		float rawAngle = GetRawTileAngle(_foundTileData.Index);

		// TODO: check this (rawAngle != 360f)
		switch (rawAngle)
		{
			case float.NaN: return rawAngle;
			case > 0f: return Angles.TransformTileAngle(rawAngle, _foundTileData.Transforms);
		}
		
		if (!_isVertical)
		{
			return _direction == Direction.Positive ? (float)Angles.Circle.Quarter : (float)Angles.Circle.ThreeQuarters;
		}
		
		// Reset height if the tile was found from the opposite side. This only works correctly
		// with originals' tile sets since we can't pre-determine if the tile is flipped by default or not
		if (_direction == Direction.Positive == _foundTileData.Transforms.IsFlipped)
		{
			_foundTileData.Size = TileSize;
		}
			
		return _direction == Direction.Positive ? 0f : (float)Angles.Circle.Half;
	}

	private void ValidateHeight()
	{
		if (!_foundTileData.IsValidAngle) return;
		if (GetRawTileAngle(_foundTileData.Index) > 0f) return;
		if (!_isVertical) return;
		
		if (_direction == Direction.Positive == _foundTileData.Transforms.IsFlipped)
		{
			_foundTileData.Size = TileSize;
		}
	}
	
	private FoundTileData Search(sbyte shift)
	{
		Vector2I shiftedPosition = _searchPosition;
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

		return new FoundTileData(index, shift, transforms, isValid, size);
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
