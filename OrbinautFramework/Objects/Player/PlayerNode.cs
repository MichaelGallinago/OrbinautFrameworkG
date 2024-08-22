using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.ObjectBase.AbstractTypes;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Objects.Player;

public abstract partial class PlayerNode : OrbinautNode, IPlayerNode
{
	public enum Types : byte
	{
		Sonic, Tails, Knuckles, Amy
	}
	
	[Export] public Types Type { get; private set; }
	[Export] public ShieldContainer Shield { get; private set; }
	[Export] private Sprite.SpriteNode _spriteNode;
	
	public IMemento Memento { get; }

	protected readonly PlayerLogic PlayerLogic;

	protected PlayerNode()
	{
		PlayerLogic = new PlayerLogic(this, _spriteNode.PlayerSprite);
		Memento = new PlayerMemento(this);
		_spriteNode.SetPlayer(PlayerLogic);
		Init();
	}

	public override void _ExitTree()
	{
		PlayerLogic.ExitTree();
		base._ExitTree();
	}
	
	public override void _Process(double delta)
	{
		PlayerLogic.Process();
		_spriteNode.Process();
	}

	public void Init()
	{
		PlayerLogic.Init();
		_spriteNode.Process();
	}
}
