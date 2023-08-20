public static class Constants
{
	public const byte RenderBuffer = 8;

	// Input (keyboard and first gamepad slot are treated as one device)
	public const byte MaxInputDevices = 4;

	public enum EnemyType : byte
	{
		Badnik,
		Boss
	}

	public enum FlipDirection : sbyte
	{
		Left = -1,
		Right = 1
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

	public enum TileLayer : byte
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
}
