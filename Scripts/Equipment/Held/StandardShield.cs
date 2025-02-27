using Godot;
using System;

public partial class StandardShield : HeldItem
{
    public override void OnHoldActivate(bool state, double delta)
    {
        if (state)
        {
            Vector3 targetPos = new Vector3(0.3f, 0.2f, 0.45f);
            Vector3 posDiff = Position - targetPos;
            Position -= posDiff * (float)delta * 10.0f;

            Vector3 targetRot = new Vector3(0.0f, 0.0f, Mathf.DegToRad(8.0f));
            Vector3 rotDiff = Rotation - targetRot;
            Rotation -= rotDiff * (float)delta * 10.0f;
        }
        else
        {
            Position -= Position * (float)delta * 10.0f;

            Vector3 targetRot = Vector3.Zero;
            Vector3 rotDiff = Rotation - targetRot;
            Rotation -= rotDiff * (float)delta * 10.0f;
        }
    }
}
