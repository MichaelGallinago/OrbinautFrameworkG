using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Spawnable.Shield;
using OrbinautNode = OrbinautFramework3.Framework.ObjectBase.AbstractTypes.OrbinautNode;

namespace OrbinautFramework3.Objects.Player;

public abstract partial class PlayerNode : OrbinautNode, ICarryTarget, IPlayerNode
{
	public enum Types : byte
	{
		Sonic, Tails, Knuckles, Amy
	}
	
	[Export] public ShieldContainer Shield { get; init; }
	[Export] public PlayerAnimatedSprite Sprite { get; init; }
	[Export] public Types Type { get; init; }
	
	public IMemento Memento { get; }

	private readonly PlayerLogic _logic;

	protected PlayerNode()
	{
		_logic = new PlayerLogic(this);
		Memento = new PlayerMemento(this);
		Init();
	}
	
	public void Init()
	{
		_logic.Init();
		Sprite.Animate(this);
	}
	
	public override void _ExitTree()
	{
		_logic.ExitTree();
		base._ExitTree();
	}
	
	public override void _Process(double delta)
	{
		_logic.Process();
		Sprite.Animate(this);
	}

	public bool IsInstanceValid() => GodotObject.IsInstanceValid(this);
}
