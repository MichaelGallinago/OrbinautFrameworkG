namespace OrbinautFramework3.Objects.Player.Data;

public class RotationData
{
    public float Angle { get; set; }
    public float VisualAngle { get; set; }

    public void Init()
    {
        Angle = 0f;
        VisualAngle = 0f;
    }
}
