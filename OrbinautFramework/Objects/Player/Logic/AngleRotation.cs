using System;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.StaticStorages;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.Logic;

public readonly struct AngleRotation(PlayerData data)
{
    public void Process()
    {
        float visualAngle = CalculateVisualAngle();
        data.Visual.Angle = visualAngle;
        data.Node.RotationDegrees = data.Sprite.Animation is Animations.Move or Animations.HammerDash ? 
            Angles.CircleFull - visualAngle : 0f;
    }

    private float CalculateVisualAngle()
    {
        bool isSmoothRotation = Improvements.RotationMode > 0;
        float angle = data.Movement.Angle;
        float visualAngle;
        
        if (!data.Movement.IsGrounded)
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
        float angleDifference = rangeAngle - data.Visual.Angle;
		
        float delta = Math.Abs(angleDifference);
        float clockwiseDelta = Math.Abs(angleDifference + Angles.CircleFull);
        float counterclockwiseDelta = Math.Abs(angleDifference - Angles.CircleFull);
		
        if (delta > counterclockwiseDelta)
        {
            angleDifference -= Angles.CircleFull;
        }
        else if (delta > clockwiseDelta)
        {
            angleDifference += Angles.CircleFull;
        }

        float multiplier = Math.Abs(data.Movement.GroundSpeed) >= 6f ? 0.5f : 0.25f;
        float angle = (data.Visual.Angle + angleDifference * multiplier) % Angles.CircleFull;
        return angle < 0f ? angle + Angles.CircleFull : angle;
    }
}
