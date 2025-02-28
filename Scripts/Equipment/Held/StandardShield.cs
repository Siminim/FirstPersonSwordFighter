using Godot;
using System;

public partial class StandardShield : HeldItem
{
    MoveAction moveAction;

    private Vector3 defaultPosition;
    private Vector3 raisedPosition;
    private Vector3 raisedRotation;

    private float slowdownDivider = 3.0f;
    private float GetSlowdownDivider() => slowdownDivider;

    public override void Assign(Player player, Node3D hand)
    {
        base.Assign(player, hand);
        moveAction = player.actionEventManager[PlayerActionEventType.Move] as MoveAction;

        if (hand == player.LeftHand)
        {
            defaultPosition = player.LeftHandDefaultPosition;
            raisedPosition = defaultPosition + new Vector3(0.3f, 0.25f, 0.45f);
            raisedRotation = new Vector3(0.0f, 0.0f, Mathf.DegToRad(-8.0f));
        }
        else
        {
            defaultPosition = player.RightHandDefaultPosition;
            raisedPosition = defaultPosition + new Vector3(-0.3f, 0.25f, 0.45f);
            raisedRotation = new Vector3(0.0f, 0.0f, Mathf.DegToRad(8.0f));
        }
    }

    public override void OnPressActivate()
    {
        moveAction.TopSpeedDivider += GetSlowdownDivider;
    }

    public override void OnHoldActivate(bool state, double delta)
    {
        if (state)
        {
            Vector3 posDiff = hand.Position - raisedPosition;
            hand.Position -= posDiff * (float)delta * 10.0f;

            Vector3 rotDiff = Rotation - raisedRotation;
            Rotation -= rotDiff * (float)delta * 10.0f;
        }
        else
        {
            Vector3 posDiff = hand.Position - defaultPosition;
            hand.Position -= posDiff * (float)delta * 10.0f;

            Vector3 targetRot = Vector3.Zero;
            Vector3 rotDiff = Rotation - targetRot;
            Rotation -= rotDiff * (float)delta * 10.0f;
        }
    }

    public override void OnReleaseActivate()
    {
        moveAction.TopSpeedDivider -= GetSlowdownDivider;
    }
}
