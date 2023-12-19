using Godot;

namespace OrbinautFramework3.Framework;

public static class VectorUtilities
{
    public static Vector2I Shuffle(this Vector2I vector, bool swap, int multiplierX, int multiplierY)
    {
        vector.X *= multiplierX;
        vector.Y *= multiplierY;
        
        if (!swap) return vector;
        
        (vector.X, vector.Y) = (vector.Y, vector.X);
        
        return vector;
    }

    public static Vector2 Shuffle(this Vector2 vector, bool swap, int multiplierX, int multiplierY)
    {
        vector.X *= multiplierX;
        vector.Y *= multiplierY;

        if (!swap) return vector;

        (vector.X, vector.Y) = (vector.Y, vector.X);

        return vector;
    }
}