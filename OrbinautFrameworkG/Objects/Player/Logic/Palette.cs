﻿using System;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Objects.Player.Data;

namespace OrbinautFrameworkG.Objects.Player.Logic;

public readonly struct Palette(PlayerData data)
{
	private ReadOnlySpan<int> PlayerColourIds => data.Node.Type switch
	{
		PlayerNode.Types.Tails => [4, 5, 6],
		PlayerNode.Types.Knuckles => [7, 8, 9],
		PlayerNode.Types.Amy => [10, 11, 12],
		_ => [0, 1, 2, 3]
	};
	
    public void Process() // TODO: use once per player type
    {
    	ReadOnlySpan<int> colours = PlayerColourIds;
    	
    	int colour = PaletteUtilities.Index[colours[0]];
    	UpdateSuper(colour, out int colourLoop, out int colourLast, out int duration);
    	UpdateRegular(colour, ref colourLoop, ref colourLast, ref duration);
	    
    	PaletteUtilities.SetRotation(colours, colourLoop, colourLast, duration);
    }

    private void UpdateSuper(int colour, out int colourLoop, out int colourLast, out int duration)
    {
    	switch (data.Node.Type)
    	{
    		case PlayerNode.Types.Sonic:
    			duration = colour switch
    			{
    				< 2 => 19,
    				< 7 => 4,
    				_ => 8
    			};
    		    
    			colourLast = 16;
    			colourLoop = 7;
    			break;
    		
    		case PlayerNode.Types.Tails:
    			duration = colour < 2 ? 28 : 12;
    			colourLast = 7;
    			colourLoop = 2;
    			break;
    		
    		case PlayerNode.Types.Knuckles:
    			duration = colour switch
    			{
    				< 2 => 17,
    				< 3 => 15,
    				_ => 3
    			};

    			colourLast = 11;
    			colourLoop = 3;
    			break;
    		
    		case PlayerNode.Types.Amy:
    			duration = colour < 2 ? 19 : 4;
    			colourLast = 11;
    			colourLoop = 3;
    			break;
    		
    		default:
    			duration = 0;
    			colourLast = 0;
    			colourLoop = 0;
    			break;
    	}
    }
    
    private void UpdateRegular(int colour, ref int colourLoop, ref int colourLast, ref int duration)
    {
    	if (data.Super.IsSuper) return;
    	
	    colourLoop = 1;
	    
    	if (colour <= 1)
    	{
		    colourLast = 1;
		    duration = 0;
		    return;
    	}

	    if (data.Node.Type != PlayerNode.Types.Sonic) return;
	    
	    colourLast = 21;
	    duration = 4;
    }
}