using System;
using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player;

public struct Rotation
{
    public void Process()
    {
        bool isSmoothRotation = SharedData.RotationMode > 0;
		
        if (!IsGrounded)
        {
            VisualAngle = Angle;
        }
        else
        {
            float rangeAngle = Angle is > 22.5f and < 337.5f ? Angle : 0f;
            VisualAngle = isSmoothRotation ? CalculateSmoothVisualAngle(rangeAngle) : rangeAngle;
        }
		
        if (!isSmoothRotation)
        {
            VisualAngle = MathF.Ceiling((VisualAngle - 22.5f) / 45f) * 45f;
        }

        RotationDegrees = Animation == Animations.Move ? 360f - VisualAngle : 0f;
    }

    private float CalculateSmoothVisualAngle(float rangeAngle)
    {
        float angleDifference = rangeAngle - VisualAngle;
		
        float delta = Math.Abs(angleDifference);
        float clockwiseDelta = Math.Abs(angleDifference + 360f);
        float counterclockwiseDelta = Math.Abs(angleDifference - 360f);
		
        if (delta >= counterclockwiseDelta)
        {
            angleDifference += counterclockwiseDelta < clockwiseDelta ? -360f : 360f;
        }
        else if (delta >= clockwiseDelta)
        {
            angleDifference += 360f;
        }

        return (VisualAngle + angleDifference * (Math.Abs(GroundSpeed) >= 6f ? 0.5f : 0.25f)) % 360f;
    }
}
