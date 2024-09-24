using Godot;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.ObjectBase;
using OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;
using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Logic;
using OrbinautFrameworkG.Objects.Spawnable.Shield;

namespace OrbinautFrameworkG.Objects.Player;

public abstract partial class PlayerNode : OrbinautNode, IPlayerNode, ICullable
{
	public enum Types : byte { Sonic, Tails, Knuckles, Amy } //TODO: remove this somehow
	
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
