using Godot;
using OrbinautFramework3.Framework.Animations;

namespace OrbinautFramework3.Objects.Player.Sprite;

[Tool]
public partial class SpriteNode : AdvancedAnimatedSprite, ISpriteNode, IPlayerSprite
{
	public SpriteLogic SpriteLogic { get; set; }

	public SpriteNode()
	{
#if TOOLS
		if (Engine.IsEditorHint()) return;
#endif
		AnimationFinished += () => SpriteLogic?.OnFinished();
	}
}
