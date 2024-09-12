using System;
using Godot;

namespace OrbinautFramework3.Framework.Tiles;

public static class Angles
{
    public const short ByteLimit = 256;
    public const float ByteStep = 2.8125f;
    public const float ByteEpsilon = 1.40625f;
    
    public const float CircleQuarter = 90f;
    public const float CircleHalf = 180f;
    public const float CircleThreeQuarters = 270f;
    public const float CircleFull = 360f;
    public const float CircleFullAndQuarter = CircleFull + CircleQuarter;
    public const float CircleFullAndHalf = CircleFull + CircleHalf;

    public enum Quadrant : byte { Down, Right, Up, Left }
    
    public static float TransformTileAngle(float angle, TileTransforms tileTransforms)
    {
        if (tileTransforms.IsRotated)
        {
            angle = (CircleFullAndQuarter - angle) % CircleFull;
        }
        if (tileTransforms.IsFlipped)
        {
            angle = (CircleFullAndHalf - angle) % CircleFull;
        }
        if (tileTransforms.IsMirrored)
        {
            angle = CircleFull - angle;
        }

        return angle;
    }
    
    public static Quadrant GetQuadrant(float angle)
    {
        angle = angle % CircleFull + (angle < 0f ? 405f : 45f);
        return (Quadrant)(Mathf.FloorToInt(angle - ByteEpsilon) / 90 & 3);
    }
    
    public static float GetRoundedVector(Vector2 distance)
    {
        return GetFloatAngle(GetByteAngle(GetFloatVector(distance)));
    }

    public static float GetFloatVector(Vector2 distance)
    {
        //TODO: analyze the accuracy
        return (CircleFull - Mathf.RadToDeg(MathF.Atan2(distance.Y, distance.X)) + CircleQuarter) % CircleFull;
    }
    
    public static byte GetByteAngle(float angle)
    {
        return (byte)MathF.Round((CircleFull - angle) / CircleFull * ByteLimit);
    }
    
    public static float GetFloatAngle(byte angle)
    {
        return (ByteLimit - angle) * CircleFull / ByteLimit % CircleFull;
    }
}