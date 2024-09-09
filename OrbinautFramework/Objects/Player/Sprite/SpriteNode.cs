using Godot;
using OrbinautFramework3.Framework.Animations;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Extensions;

namespace OrbinautFramework3.Objects.Player.Sprite;

[Tool]
public partial class SpriteNode : AdvancedAnimatedSprite, ISpriteNode
{
	public SpriteLogic SpriteLogic
	{
		private get => _spriteLogic;
		set
		{
#if TOOLS
			if (Engine.IsEditorHint()) return;
#endif
			if (_spriteLogic != null)
			{
				AnimationFinished -= SpriteLogic.OnFinished;
			}
			
			_spriteLogic = value;
			AnimationFinished += SpriteLogic.OnFinished;
		}
	}
	private SpriteLogic _spriteLogic;
	
	public IPlayerSprite PlayerSprite => SpriteLogic;
	
	public int GetAnimationFrameCount(Animations animation)
	{
		return SpriteFrames.GetFrameCount(animation.ToStringFast());
	}
}
