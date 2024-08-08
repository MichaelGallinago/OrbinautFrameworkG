using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player.Modules;

public struct AngleRotation(PlayerData data)
{
    public void Process()
    {
        float visualAngle = CalculateVisualAngle();
        data.Rotation.VisualAngle = visualAngle;
        data.PlayerNode.RotationDegrees = data.Visual.Animation == Animations.Move ? 360f - visualAngle : 0f;
    }

    private float CalculateVisualAngle()
    {
        bool isSmoothRotation = SharedData.RotationMode > 0;
        float angle = data.Rotation.Angle;
        float visualAngle;
        
        if (!data.Physics.IsGrounded)
        {
            if (isSmoothRotation) return angle;
            visualAngle = angle;
        }
        else
        {
            visualAngle = angle is > 22.5f and < 337.5f ? angle : 0f;
            if (isSmoothRotation) return CalculateSmoothVisualAngle(visualAngle);
        }
        
        return MathF.Ceiling((visualAngle - 22.5f) / 45f) * 45f;
    }

    private float CalculateSmoothVisualAngle(float rangeAngle)
    {
        float angleDifference = rangeAngle - data.Rotation.VisualAngle;
		
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

        float multiplier = Math.Abs(data.Physics.GroundSpeed) >= 6f ? 0.5f : 0.25f;
        return (data.Rotation.VisualAngle + angleDifference * multiplier) % 360f;
    }
}
