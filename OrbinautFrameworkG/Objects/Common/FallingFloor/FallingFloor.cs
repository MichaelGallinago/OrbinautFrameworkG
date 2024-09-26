using Godot;
using Godot.Collections;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.Culling;
using OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;
using OrbinautFrameworkG.Framework.SceneModule;
using OrbinautFrameworkG.Framework.StaticStorages;
using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Spawnable.Piece;

namespace OrbinautFrameworkG.Objects.Common.FallingFloor;

public partial class FallingFloor(Sprite2D sprite, Array<AtlasTexture> piecesTextures, Vector2I piecesSize) : SolidNode
{
    private enum States : byte
    {
        Solid, Collapse, Fallen
    }

    private States _state;
    private float _stateTimer = 8f;
    private bool _isTouched;
    private Vector2I _corner;
    private Vector2I _size = (Vector2I)sprite.Texture.GetSize();

    public override void _Ready() => _corner = (Vector2I)(Position - (Vector2)_size / 2f);
    
    public override void _Process(double delta)
    {
        switch (_state)
        {
            case States.Solid: ActSolid(); break;
            case States.Collapse: HandlePlayerOnCollapse(); break;
        }
    }

    private void ActSolid()
    {
        CheckTarget();
                
        if (!_isTouched || _stateTimer > 0f)
        {
            _stateTimer -= Scene.Instance.Speed;
            return;
        }
        
        Collapse();
    }

    private void HandlePlayerOnCollapse()
    {
        // When falling apart, act as solid only for the players already standing on the object
        foreach (IPlayer player in Scene.Instance.Players.Values)
        {
            if (!player.CheckSolidCollision(SolidBox, Constants.CollisionSensor.Top)) continue;
            player.ActSolid(this, Constants.SolidType.Top);
        }
        
        if (_stateTimer > 0f)
        {
            _stateTimer -= Scene.Instance.Speed;
            return;
        }
			
        // Release all players from this object
        foreach (IPlayer player in Scene.Instance.Players.Values)
        {
            if (player.Data.Collision.OnObject.SolidBox != SolidBox) continue;
            player.Data.Movement.IsGrounded = false;
            player.Data.Collision.OnObject = null;
        }

        _state = States.Fallen;
    }

    private void CheckTarget()
    {
        foreach (IPlayer player in Scene.Instance.Players.Values)
        {
            player.ActSolid(this, Constants.SolidType.Top);
			
            if (_isTouched) continue;
            _isTouched = player.CheckSolidCollision(SolidBox, Constants.CollisionSensor.Top);
        }
    }

    private void Collapse()
    {
        Node parent = GetParent();
        if (parent == null) return;
        
        var index = 0;
        if (Scale.X >= 0f)
        {
            for (var y = 0; y < _size.Y; y += piecesSize.Y)
            for (var x = 0; x < _size.X; x += piecesSize.X)
            {
                CreatePiece(parent, x, y, index++, x / 4f + (_size.Y - y) / 8f);
            }
        }
        else
        {
            for (var y = 0; y < _size.Y; y += piecesSize.Y)
            for (int x = _size.X - piecesSize.X; x >= 0; x -= piecesSize.X)
            {
                CreatePiece(parent, x, y, index++, x / 4f + (_size.Y - y) / 8f);
            }
        }

        CullingType = ICullable.Types.Respawn;
        //TODO: audio
        //AudioPlayer.PlaySound(SoundStorage.Break);
		
        Visible = false;
        sprite.Visible = false;
        _stateTimer = _size.X / 4f + _size.Y / 8f;
        _state = States.Collapse;
    }

    private void CreatePiece(Node onNode, int x, int y, int index, float timer)
    {
        onNode.AddChild(new Piece(piecesTextures[index], timer, GravityType.Default)
        {
            Position = _corner + new Vector2I(x, y),
            Scale = Scale
        });
    }
}
