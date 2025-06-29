namespace OrbinautFrameworkG.Framework.StaticStorages;

public static class Constants
{
	public const byte BaseFrameRate = 60;
	public const byte RenderBuffer = 8;
	
	public const ushort TileLimit = 256;
	public const byte TileSize = 16;
	public const byte DoubleTileSize = TileSize * 2;
	public const float DefaultAirTimer = 1800f;
	
	public enum ZIndexes : ushort
	{
		AboveForeground = 1024
	}

	public enum EnemyType : byte
	{
		Badnik, Boss
	}

	public enum Direction : sbyte
	{
		Negative = -1, Positive = 1
	}
	
	public enum TileBehaviours : byte
	{
		Floor, RightWall, Ceiling, LeftWall
	}

	public enum CollisionSensor : byte
	{
		Top, Bottom, Left, Right, Any
	}


	public enum SolidType : byte
	{
		Full, Top, Sides, ItemBox
	}
	
	public enum AttachType : byte
	{
		Default, None, ResetPlayer
	}

	public enum FadeState : byte
	{
		None, Active, Max
	}

	public enum FadeMode : byte
	{
		Out, In
	}

	public enum FadeBlend : byte
	{
		Black, White, Flash
	}

	public enum TileLayers : sbyte
	{
		Main, Secondary, None
	}
	
	public enum TouchState : byte
	{
		None, Top, Bottom, Left, Right
	}
}
