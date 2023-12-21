using System;
using Godot;

namespace OrbinautFramework3.Framework.Tiles;

public static class Angles
{
    public const short ByteAngleLimit = 256;
    
    public enum Circle : ushort
    {
        Quarter = 90,
        Half = 180,
        ThreeQuarters = 270,
        Full = 360,
        OneAndAHalf = 540
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
    
    public static byte GetQuadrant(float angle)
    {
        return (byte)(Mathf.FloorToInt(angle % 360f + 45f - Constants.AngleIncrement) / 90 & 3);
    }
    
    public static float GetVector256(Vector2 distance)
    {
        float ang360 = GetVector360(distance);
        byte ang256 = GetByteAngle(ang360);
	
        return GetFloatAngle(ang256);
    }
    
    public static float GetVector360(Vector2 distance)
    {
        return (MathF.Atan2(distance.Y, distance.X) + 90f) % 360f;
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