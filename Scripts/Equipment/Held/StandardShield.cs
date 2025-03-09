using Godot;
using System;

public partial class StandardShield : HeldItem
{
    private Vector3 defaultPosition;
    private Vector3 raisedPosition;
    private Vector3 raisedRotation;

    private float slowdownDivider = 3.0f;

    public override void Assign(Character character, Node3D hand)
    {
        base.Assign(character, hand);

        if (hand == character.LeftHand)
        {
            defaultPosition = character.LeftHandDefaultPosition;
            raisedPosition = defaultPosition + new Vector3(0.3f, 0.25f, 0.45f);
            raisedRotation = new Vector3(0.0f, 0.0f, Mathf.DegToRad(-8.0f));
        }
        else
        {
            defaultPosition = character.RightHandDefaultPosition;
            raisedPosition = defaultPosition + new Vector3(-0.3f, 0.25f, 0.45f);
            raisedRotation = new Vector3(0.0f, 0.0f, Mathf.DegToRad(8.0f));
        }
    }

    public override void OnEquip()
    {
        character.TopSpeedModifiers.Divider += GetSlowdownDivider;
    }

    public override void OnUnequip()
    {
        character.TopSpeedModifiers.Divider -= GetSlowdownDivider;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Active)
        {
            character.performingAction = true;
            Vector3 posDiff = hand.Position - raisedPosition;
            hand.Position -= posDiff * (float)delta * 10.0f;

            Vector3 rotDiff = Rotation - raisedRotation;
            Rotation -= rotDiff * (float)delta * 10.0f;
        }
        else
        {
            character.performingAction = false;
            Vector3 posDiff = hand.Position - defaultPosition;
            hand.Position -= posDiff * (float)delta * 10.0f;

            Vector3 targetRot = Vector3.Zero;
            Vector3 rotDiff = Rotation - targetRot;
            Rotation -= rotDiff * (float)delta * 10.0f;
        }
    }

    private float GetSlowdownDivider()
    {
        if (Active)
            return slowdownDivider;
        
        return 1.0f;
    }
}
