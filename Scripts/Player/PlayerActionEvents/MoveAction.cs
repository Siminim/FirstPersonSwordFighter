using Godot;
using System;

public class MoveAction : PlayerActionEvent
{
    public Delegates.FloatReturnDelegate TopSpeedAdditive;
    public Delegates.FloatReturnDelegate TopSpeedDivider;

    private float groundAcceleration = 5.0f;
    private float airAcceleration = 0.8f;
    private float topSpeed = 8.0f;
    private float topSpeedBoost = 12.0f;

    public Vector3 localMovementVector = Vector3.Zero;

    private float GetDefaultTopSpeed() => topSpeed;

    public override void OnEffectApplied(Player player)
    {
        base.OnEffectApplied(player);

        player.OnMove += MovementAction;
        TopSpeedAdditive += GetDefaultTopSpeed;
    }

    public override void OnEffectRemoved()
    {
        player.OnMove -= MovementAction;
        TopSpeedAdditive -= GetDefaultTopSpeed;
    }

    public float GetTotalTopSpeed()
    {
        return GetAdditiveTopSpeed() / GetTopSpeedDivider();
    }

    private float GetAdditiveTopSpeed()
    {
        if (TopSpeedAdditive == null || TopSpeedAdditive.GetInvocationList().Length == 0)
            return 0.0f;

        float speed = 0.0f;

        foreach (Delegates.FloatReturnDelegate topSpeedDelegate in TopSpeedAdditive.GetInvocationList())
        {
            speed += topSpeedDelegate.Invoke();
        }

        return speed;
    }

    private float GetTopSpeedDivider()
    {
        if (TopSpeedDivider == null || TopSpeedDivider.GetInvocationList().Length == 0)
            return 1.0f;

        float speed = 0.0f;

        foreach (Delegates.FloatReturnDelegate topSpeedDelegate in TopSpeedDivider.GetInvocationList())
        {
            speed += topSpeedDelegate.Invoke();
        }

        return speed;
    }

    private void MovementAction(Vector2 inputDir, double delta)
    {
        if (inputDir == Vector2.Zero)
        {
            localMovementVector = Vector3.Zero;
            return;
        }

        localMovementVector = (GameManager.GetPlayerCamera.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        Vector3 targetVelocity = localMovementVector * GetTotalTopSpeed();

        float xBooster = 1.0f;
        float zBooster = 1.0f;

        if (Mathf.Sign(localMovementVector.X) != Mathf.Sign(player.Velocity.X))
            xBooster = topSpeedBoost;
        if (Mathf.Sign(localMovementVector.Z) != Mathf.Sign(player.Velocity.Z))
            zBooster = topSpeedBoost;

        Vector3 velocityDif = targetVelocity - new Vector3(player.Velocity.X * xBooster, 0, player.Velocity.Z * zBooster);

        if (player.IsOnFloor())
            velocityDif *= groundAcceleration;
        else
            velocityDif *= airAcceleration;

        player.Velocity += new Vector3(velocityDif.X, 0, velocityDif.Z) * (float)delta;
    }
}