using Godot;
using OrbinautFramework3.Framework.Animations;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Extensions;

namespace OrbinautFramework3.Objects.Player.Sprite;

[Tool]
public partial class SpriteNode : AdvancedAnimatedSprite, ISpriteNode
{
	[Export] private SpriteLogic _spriteLogic;
	
	public IPlayerSprite PlayerSprite => _spriteLogic;

	public override void _Ready()
	{
		base._Ready();
#if TOOLS
		if (Engine.IsEditorHint()) return;
#endif
		AnimationFinished += _spriteLogic.OnFinished;
	}
	
	public void Process() => _spriteLogic.Process();

	public void SetPlayer(IPlayer player) => _spriteLogic.SetPlayer(player, this);
	
	public int GetAnimationFrameCount(Animations animation)
	{
		return SpriteFrames.GetFrameCount(animation.ToStringFast());
	}
}
