using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Spawnable.Shield;
using OrbinautNode = OrbinautFramework3.Framework.ObjectBase.AbstractTypes.OrbinautNode;

namespace OrbinautFramework3.Objects.Player;

public abstract partial class PlayerNode : OrbinautNode, IPlayerNode
{
	public enum Types : byte
	{
		Sonic, Tails, Knuckles, Amy
	}
	
	[Export] public Types Type { get; init; }
	[Export] public ShieldContainer Shield { get; init; }
	[Export] private Sprite.SpriteNode SpriteNode { get; init; }
	
	public IMemento Memento { get; }

	private readonly PlayerLogic _playerLogic;

	protected PlayerNode()
	{
		_playerLogic = new PlayerLogic(this, SpriteNode.PlayerSprite);
		Memento = new PlayerMemento(this);
		SpriteNode.SetPlayer(_playerLogic);
		Init();
	}

	public override void _ExitTree()
	{
		_playerLogic.ExitTree();
		base._ExitTree();
	}
	
	public override void _Process(double delta)
	{
		_playerLogic.Process();
		SpriteNode.Process();
	}

	public void Init()
	{
		_playerLogic.Init();
		SpriteNode.Process();
	}
}
