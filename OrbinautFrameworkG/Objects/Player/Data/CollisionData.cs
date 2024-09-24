using System.Collections.Generic;
using Godot;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.ObjectBase;

namespace OrbinautFrameworkG.Objects.Player.Data;

public class CollisionData
{
    public Vector2I Radius { get; set; }
    public Vector2I RadiusNormal { get; private set; }
    public Vector2I RadiusSpin { get; private set; }
    
    public bool IsStickToConvex { get; set; }
    public Constants.TileLayers TileLayer { get; set; }
    public Constants.TileBehaviours TileBehaviour { get; set; }
    
    public ISolid OnObject { get; set; }
    public HashSet<SolidBox> PushObjects { get; } = [];
    public Dictionary<SolidBox, Constants.TouchState> TouchObjects { get; } = [];

    public void Init(PlayerNode.Types type)
    {
        IsStickToConvex = false;
        TileBehaviour = Constants.TileBehaviours.Floor;
        TileLayer = Constants.TileLayers.Main;
        OnObject = null;
        
        (RadiusNormal, RadiusSpin) = type switch
        {
            PlayerNode.Types.Tails => (new Vector2I(9, 15), new Vector2I(7, 14)),
            PlayerNode.Types.Amy => (new Vector2I(9, 16), new Vector2I(7, 12)),
            _ => (new Vector2I(9, 19), new Vector2I(7, 14))
        };
        Radius = RadiusNormal;
    }
}
