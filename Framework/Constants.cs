namespace OrbinautFramework3.Framework;

public static class Constants
{
	public const byte RenderBuffer = 8;

	// Input (keyboard and first gamepad slot are treated as one device)
	public const byte MaxInputDevices = 4;

	public const float AngleIncrement = 1.40625f;
	public const ushort TileLimit = 256;
	public const byte TileSize = 16;
	public const float AirValueMax = 1800f;

	public enum EnemyType : byte
	{
		Badnik, Boss
	}

	public enum Direction : sbyte
	{
		Negative = -1, Positive = 1
	}
	
	public enum GroundMode : byte
	{
		Floor, RightWall, Ceiling, LeftWall
	}

	public enum CollisionSensor : byte
	{
		Hitbox, HitboxExtra, SolidU, SolidD, SolidL, SolidR, SolidAny
	}

	public enum SolidType : byte
	{
		All, AllReset, Top, TopReset, Sides, ItemBox
	}

	public enum DepthType : sbyte
	{
		Background = -128,
		Level = 0,
		Lowest,
		Below,
		Enemy,
		Default,
		Above,
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
		None, Up, Down, Left, Right
	}
}
