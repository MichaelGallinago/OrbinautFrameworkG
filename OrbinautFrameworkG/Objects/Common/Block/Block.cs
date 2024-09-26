using OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;
using OrbinautFrameworkG.Framework.SceneModule;
using OrbinautFrameworkG.Framework.StaticStorages;
using OrbinautFrameworkG.Objects.Player.Data;

namespace OrbinautFrameworkG.Objects.Common.Block;


public partial class Block : SolidNode
{
    public override void _Process(double delta)
    {
        foreach (IPlayer player in Scene.Instance.Players.Values)
        {
            // Combo counter and spin flag are cleared when player lands, so back them up
            bool isSpinning = player.Data.Movement.IsSpinning;
            uint comboCount = player.Data.Item.ComboCounter;

            Constants.AttachType attachType = player.Data.Movement.IsSpinning ? Constants.AttachType.None : Constants.AttachType.Default;
            player.ActSolid(this, Constants.SolidType.Full, attachType);
            /*
            if (!isSpinning || !obj_check_collision(_player, COL_SOLID_U)) continue;
            
            // Release all players from this object
            if (player.OnObject == this)
            {
                player.OnObject = null;
                player.IsGrounded = false;
            }

            player.Animation = PlayerConstants.Animation.Spin;
            player.IsJumping = true;
            player.IsSpinning = true;
            player.Radius = player.RadiusSpin;
            player.ComboCounter = comboCount + 1;
            player.Speed = new Vector2(player.Speed.X, player.Speed.Y - 3);
			
            player_inc_combo_score(_player);
			
            instance_create(x, y, obj_score,
            {
                Combo_Counter: _player.combo_counter
            });
			
            AudioPlayer.PlaySound(SoundStorage.Break);
			
            for (var i = 0; i < 2; i++)
            {
                for (var j = 0; j < 2; j++)
                {
                    var _spd_x = -2;
                    var _spd_y = -2;
					
                    if i > 0
                    {
                        _spd_x = -_spd_x;
                    }
					
                    if j > 0
                    {
                        _spd_y = _spd_y / 2;
                        _spd_x = _spd_x / 2;
                    }
					
                    instance_create(x - 8 + i * 16, y - 8 + j * 16, obj_piece,
                    {
                        Draw_Sprite: sprite_index,
                        Draw_X:	16 * i,
                        Draw_Y: 16 * j,						
                        Draw_Width: 16,
                        Draw_Height: 16,
						
                        Move_Xsp: _spd_x,
                        Move_Ysp: _spd_y,
                        Move_Grv: GRV_DEFAULT
                    });
                }	
            }
			
            instance_destroy();
			
            break;
            */
        }
    }
}