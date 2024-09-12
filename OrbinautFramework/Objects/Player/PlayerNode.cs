using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.ObjectBase.AbstractTypes;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Objects.Player;

public abstract partial class PlayerNode : OrbinautNode, IPlayerNode, ICullable
{
	public enum Types : byte //TODO: remove this somehow
	{
		Sonic, Tails, Knuckles, Amy
	}
	
	[Export] public Types Type { get; private set; }
	[Export] public ShieldContainer Shield { get; private set; }
	[Export] protected Sprite.SpriteNode SpriteNode { get; private set; }
	
	public IMemento Memento { get; private set; }

	protected PlayerLogic PlayerLogic;

	public override void _EnterTree()
	{
		base._EnterTree();
		Scene.Instance.Players.Add(PlayerLogic);
		Memento = new PlayerMemento(this);
	}

	public override void _Ready()
	{
		Init();
		Scene.Instance.Players.CountChanged.Subscribe(PlayerLogic);
	}
	
	public override void _ExitTree()
	{
		base._ExitTree();
		Scene.Instance.Players.Remove(PlayerLogic);
		Scene.Instance.Players.CountChanged.Unsubscribe(PlayerLogic);
	}
	
	public override void _Process(double delta) => PlayerLogic.Process();
	public void Init() => PlayerLogic.Init();
}
