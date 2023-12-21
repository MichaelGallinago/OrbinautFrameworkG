using Godot;

namespace OrbinautFramework3.Framework.Tiles;

public static class Angles
{
    public enum Circle : ushort
    {
        Quarter = 90,
        Half = 180,
        ThreeQuarters = 270,
        Full = 360,
        OneAndAHalf = 540
    }

    public static float GetFloatAngle(byte angle)
    {
        return (256 - angle) * 360 / 256f;
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
}