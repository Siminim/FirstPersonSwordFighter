using Godot;
using System;

public class GravityEvent : PlayerActionEvent
{
    public Delegates.FloatReturnDelegate IncreaseGravityDelegate;

    private float maxFallVelocity = -58.8f;
    private float fastFallBoost = 6.5f;

    public override void OnEffectApplied(Player player)
    {
        base.OnEffectApplied(player);
        player.WhileInAir += Gravity;
    }

    public override void OnEffectRemoved()
    {
        player.WhileInAir -= Gravity;
    }

    private float GravityMultipliers()
    {
        if (IncreaseGravityDelegate == null || IncreaseGravityDelegate.GetInvocationList().Length == 0)
            return 1.0f;

        float gravity = 0.0f;

        foreach (Delegates.FloatReturnDelegate increaseGravityDelegate in IncreaseGravityDelegate.GetInvocationList())
        {
            gravity += increaseGravityDelegate.Invoke();
        }

        return gravity;
    }

    private void Gravity(double delta)
    {
        player.Velocity += player.GetGravity() * GravityMultipliers() * (float)delta;

        if (player.Velocity.Y <= 0.0f)
            player.Velocity += player.GetGravity().Normalized() * fastFallBoost * (float)delta;

        if (player.Velocity.Y < maxFallVelocity)
            player.Velocity = new Vector3(player.Velocity.X, maxFallVelocity, player.Velocity.Z);
    }
}
