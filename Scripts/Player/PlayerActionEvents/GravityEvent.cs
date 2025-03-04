using Godot;
using System;

public class GravityEvent : PlayerActionEvent
{
    public FloatReturnDelegateModifiers GravityModifiers;

    private float maxFallVelocity = -58.8f;
    private float fastFallBoost = 6.5f;

    public override void OnEffectApplied(Player player)
    {
        base.OnEffectApplied(player);
        GravityModifiers.Additive += GetGravitySpeed;
        player.WhileInAir += Gravity;
    }

    public override void OnEffectRemoved()
    {
        GravityModifiers.Additive -= GetGravitySpeed;
        player.WhileInAir -= Gravity;
    }

    private float GetGravitySpeed()
    {
        return player.GetGravity().Length();
    }

    private Vector3 GetGravityDirection()
    {
        return player.GetGravity().Normalized();
    }

    private void Gravity(double delta)
    {
        player.Velocity += GetGravityDirection() * GravityModifiers.GetSafeFinalModifier() * (float)delta;

        if (player.Velocity.Y <= 0.0f)
            player.Velocity += player.GetGravity().Normalized() * fastFallBoost * (float)delta;

        if (player.Velocity.Y < maxFallVelocity)
            player.Velocity = new Vector3(player.Velocity.X, maxFallVelocity, player.Velocity.Z);
    }
}
