using Godot;

namespace OrbinautFramework3.Framework;

public static class VectorUtilities
{
    public static Vector2I Shuffle(this Vector2I vector, bool swap, bool signX, bool signY)
    {
        if (signX)
        {
            vector.X *= -1;
        }
        
        if (signY)
        {
            vector.Y *= -1;
        }

        if (!swap) return vector;
        
        (vector.X, vector.Y) = (vector.Y, vector.X);
        
        return vector;
    }
    
    public static Vector2 Shuffle(this Vector2 vector, bool swap, bool signX, bool signY)
    {
        if (signX)
        {
            vector.X *= -1;
        }
        
        if (signY)
        {
            vector.Y *= -1;
        }

        if (!swap) return vector;
        
        (vector.X, vector.Y) = (vector.Y, vector.X);
        
        return vector;
    }

}