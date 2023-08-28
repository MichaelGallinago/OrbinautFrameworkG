public static class Constants
{
	public const byte RenderBuffer = 8;

	// Input (keyboard and first gamepad slot are treated as one device)
	public const byte MaxInputDevices = 4;
	
	public const ushort TileLimit = 256;
	public const byte TileSize = 16;

	public enum EnemyType : byte
	{
		Badnik,
		Boss
	}

	public enum Direction : sbyte
	{
		Negative = -1,
		Positive = 1
	}
	
	public enum GroundMode : byte
	{
		Floor,
		RightWall,
		Ceiling,
		LeftWall
	}

	public enum CollisionSensor : byte
	{
		Hitbox,
		HitboxExtra,
		Trigger,
		SolidU,
		SolidD,
		SolidL,
		SolidR,
		SolidAny
	}

	public enum SolidType : byte
	{
		All,
		AllReset,
		Top,
		TopReset,
		Sides,
		ItemBox
	}

	public enum ProcessType : byte
	{
		Active,
		Reset,
		Pause,
		Delete,
		Default
	}

	public enum DepthType : sbyte
	{
		Highest = -45,
		Above = -25,
		Default = 0,
		Enemy = 10,
		Below = 25,
		Lowest = 45
	}

	public enum FadeState : byte
	{
		None,
		Active,
		Max
	}

	public enum FadeMode : byte
	{
		Out,
		In
	}

	public enum FadeBlend : byte
	{
		Black,
		White,
		Flash
	}

	public enum TileLayer : sbyte
	{
		Main,
		Secondary,
		None
	}
	
	public enum Barrier : byte
	{
		None,
		Normal,
		Thunder,
		Flame,
		Water
	}
	
	public enum TouchState : byte
	{
		None,
		Up,
		Down,
		Left,
		Right
	}
}
