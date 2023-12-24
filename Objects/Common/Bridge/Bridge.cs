using Godot;
using OrbinautFramework3.Framework.CommonObject;

namespace OrbinautFramework3.Objects.Common.Bridge;

public partial class Bridge(Texture2D logTexture, byte logCount, int logSeparation) : CommonObject
{
    private int _activeLogId;
    private int _maxDip;
    private float _angle;

    public override void _Ready()
    {
        /*
        log_amount = floor(sprite_width / log_size);

        for (var i = 0; i < log_amount; i++)
        {
            log_x[i] = x - log_amount * log_size_half + log_size * i + log_size_half
            log_y[i] = y;
            dip[i] = i < floor(log_amount / 2) ? (i + 1) * 2 : (log_amount - i) * 2;
        }

        // Player should not balance on this object
        data_solid.no_balance = true;

        // Properties
        obj_set_solid(log_amount * log_size_half, log_size_half);
        obj_set_priority(4);
        obj_set_behaviour(BEHAVE_PAUSE);
        */
    }

    public override void _Draw()
    {
        int width = logCount * logSeparation;
        for (var drawX = 0; drawX < width; drawX += logSeparation)
        {
            DrawTexture(logTexture, Position + new Vector2(drawX, 0f));
        }
    }
}
