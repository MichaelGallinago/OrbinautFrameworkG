using Godot;

namespace OrbinautFramework3.Framework;

public static class VectorUtilities
{
    public static Vector2I Shuffle(this Vector2I vector, int multiplierX, int multiplierY, bool swap = false)
    {
        vector.X *= multiplierX;
        vector.Y *= multiplierY;
        
        if (!swap) return vector;
        
        (vector.X, vector.Y) = (vector.Y, vector.X);
        
        return vector;
    }

    public static Vector2 Shuffle(this Vector2 vector, int multiplierX, int multiplierY, bool swap = false)
    {
        vector.X *= multiplierX;
        vector.Y *= multiplierY;

        if (!swap) return vector;

        (vector.X, vector.Y) = (vector.Y, vector.X);

        return vector;
    }
}