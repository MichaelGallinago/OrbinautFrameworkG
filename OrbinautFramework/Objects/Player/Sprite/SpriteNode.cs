using Godot;
using OrbinautFramework3.Framework.Animations;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Extensions;

namespace OrbinautFramework3.Objects.Player.Sprite;

[Tool]
public abstract partial class SpriteNode : AdvancedAnimatedSprite, ISpriteNode
{
	[Export] private SpriteLogic _spriteLogic;

	public override void _Ready()
	{
		base._Ready();
#if TOOLS
		if (Engine.IsEditorHint()) return;
#endif
		AnimationFinished += _spriteLogic.OnFinished;
		AnimationChanged += _spriteLogic.UpdateData;
	}
	
	public void Process() => _spriteLogic.Process();

	public SpriteData SetPlayer(IPlayer player) => _spriteLogic.SetPlayer(player, this);
	
	public int GetAnimationFrameCount(Animations animation)
	{
		return SpriteFrames.GetFrameCount(animation.ToStringFast());
	}
}
