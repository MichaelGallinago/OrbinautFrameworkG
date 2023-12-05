using Godot;
using OrbinautFramework3.Framework.CommonObject;

namespace OrbinautFramework3.Objects.Common.AirBubbler;

public partial class AirBubbler : CommonObject
{
    private int _state;
    private byte _waitTime;
    private int _waitCycle;
    
    bubbles_to_spawn = 0;
    bubble_id = 0;
    bubble_id_large = 0;
    type_array_to_use = 0;
    type_array =
    [
    [0,0,0,0,1,0],
    [0,0,0,1,0,0],
    [1,0,1,0,0,0],
    [0,1,0,0,1,0]
    ];
    
    
    public AirBubbler()
    {
        // Variables
        _state = 0;
        _waitTime = SetDelay();
        _waitCycle = 0;
        bubbles_to_spawn = 0;
        bubble_id = 0;
        bubble_id_large = 0;
        type_array_to_use = 0;
        type_array =
        [
        [0,0,0,0,1,0],
        [0,0,0,1,0,0],
        [1,0,1,0,0,0],
        [0,1,0,0,1,0]
            ];
	
        // Properties
        obj_set_priority(1);
        obj_set_behaviour(BEHAVE_RESET);
        ani_set(sprite_index, 16);
    }
    
    private byte SetDelay()
    {
        return (byte)(GD.Randi() & sbyte.MaxValue + 128);
    }
}