using Godot;
using OrbinautFrameworkG.Framework.Animations;

namespace OrbinautFrameworkG.Objects.Player.Sprite;

[Tool]
public partial class SpriteNode : AdvancedAnimatedSprite, ISpriteNode, IPlayerSprite
{
	public SpriteLogic SpriteLogic { get; set; }
	
	public SpriteNode()
	{
#if TOOLS
		if (Engine.IsEditorHint()) return;
#endif
		AnimationFinished += OnAnimationFinished;
	}
	
	private void OnAnimationFinished() => SpriteLogic.OnFinished();
}
