namespace OrbinautFramework3.Objects.Player.Physics;

public struct Position
{
    public void Update()
    {
        if (Action == Actions.Carried) return;
		
        if (IsStickToConvex)
        {
            Velocity.Clamp(-16f * Vector2.One, 16f * Vector2.One);
        }
		
        Position = Velocity.CalculateNewPosition(Position);
        Velocity.Vector = Velocity.Vector;
		
        if (IsGrounded) return;
        Velocity.AccelerationY = Gravity;
    }
}