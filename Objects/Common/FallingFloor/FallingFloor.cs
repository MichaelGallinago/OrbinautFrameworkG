using Godot;
using Godot.Collections;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Spawnable.Piece;

namespace OrbinautFramework3.Objects.Common.FallingFloor;

using Player;

public partial class FallingFloor(Sprite2D sprite, Array<AtlasTexture> piecesTextures, Vector2I piecesSize) : BaseObject
{
    private enum States
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
            case States.Solid:
                CheckTarget();
                
                if (!_isTouched || _stateTimer > 0)
                {
                    _stateTimer -= FrameworkData.ProcessSpeed;
                    break;
                }
                
                Collapse();
                break;
		
            case States.Collapse:
                // When falling apart, act as solid only for the players already standing on the object
                foreach (Player player in Player.Players)
                {
                    if (!CheckCollision(player, Constants.CollisionSensor.SolidU)) continue;
                    ActSolid(player, Constants.SolidType.Top);
                }
			
                if (_stateTimer > 0f)
                {
                    _stateTimer -= FrameworkData.ProcessSpeed;
                    break;
                }
			
                // Release all players from this object
                foreach (Player player in Player.Players)
                {
                    if (player.OnObject != this) continue;
                    player.OnObject = null;
                    player.IsGrounded = false;
                }

                _state = States.Fallen;
                break;
        }
    }

    private void CheckTarget()
    {
        foreach (Player player in Player.Players)
        {
            ActSolid(player, Constants.SolidType.Top);
			
            if (_isTouched) continue;
            _isTouched = CheckCollision(player, Constants.CollisionSensor.SolidU);
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
        
        SetBehaviour(BehaviourType.Reset);
        //TODO: audio
        //audio_play_sfx(sfx_break);
		
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