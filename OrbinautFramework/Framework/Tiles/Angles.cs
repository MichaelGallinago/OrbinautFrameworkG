using System;
using Godot;

namespace OrbinautFramework3.Framework.Tiles;

public static class Angles
{
    public const short ByteAngleLimit = 256;
    public const float ByteAngleStep = 2.8125f;
    
    public enum Circle : ushort
    {
        Quarter = 90,
        Half = 180,
        ThreeQuarters = 270,
        Full = 360,
        OneAndAHalf = 540
    }

    public enum Quadrant : byte
    {
        Down,
        Right,
        Up,
        Left
    }
    
    public static float TransformTileAngle(float angle, TileTransforms tileTransforms)
    {
        if (tileTransforms.IsRotated)
        {
            angle = (450f - angle) % 360f;
        }
        if (tileTransforms.IsFlipped)
        {
            angle = (540f - angle) % 360f;
        }
        if (tileTransforms.IsMirrored)
        {
            angle = 360f - angle;
        }

        return angle;
    }
    
    public static Quadrant GetQuadrant(float angle)
    {
        return (Quadrant)(Mathf.FloorToInt(angle % 360f + 45f - Constants.AngleIncrement) / 90 & 3);
    }
    
    public static float GetVector256(Vector2 distance)
    {
        return GetFloatAngle(GetByteAngle(GetFloatVector(distance)));
    }

    public static float GetFloatVector(Vector2 distance)
    {
        //TODO: analyze the accuracy
        return (360f - Mathf.RadToDeg(MathF.Atan2(distance.Y, distance.X)) + 90f) % 360f;
    }
    
    public static byte GetByteAngle(float angle)
    {
        return (byte)MathF.Round(((float)Circle.Full - angle) / (float)Circle.Full * ByteAngleLimit);
    }
    
    public static float GetFloatAngle(byte angle)
    {
        return (ByteAngleLimit - angle) * (int)Circle.Full / (float)ByteAngleLimit;
    }
}