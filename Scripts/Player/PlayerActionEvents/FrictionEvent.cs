using Godot;
using System;

public class FrictionEvent : PlayerActionEvent
{
    public float friction = 0.8f;

    public override void OnEffectApplied(Player player)
    {
        base.OnEffectApplied(player);
        player.WhileOnGround += Friction;
    }

    public override void OnEffectRemoved()
    {
        player.WhileOnGround -= Friction;
    }

    private void Friction(double delta)
    {
        MoveAction moveAction = player.actionEventManager[PlayerActionEventType.Move] as MoveAction;

        if (moveAction.localMovementVector != Vector3.Zero || (player.Velocity.X == 0 && player.Velocity.Z == 0))
            return;

        Vector3 frictionMask = new Vector3(player.Velocity.X, 0, player.Velocity.Z);
        Vector3 frictionAmount = frictionMask.Normalized() * friction;
        player.Velocity -= frictionAmount;

        if (player.Velocity.Length() < 0.5f)
            player.Velocity = Vector3.Zero;
    }
}
