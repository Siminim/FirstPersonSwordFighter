using Godot;
using System;

public partial class StandardSword : HeldItem
{
    private AnimationPlayer animationPlayer;

    private float moveSpeedDivider = 5.0f;
    private float rotateSpeedDivider = 5.0f;

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public override void Assign(Character character, Node3D hand)
    {
        base.Assign(character, hand);
    }

    public override void OnEquip()
    {
        character.TopSpeedModifiers.Divider += GetSlowdownDivider;
        character.RotateBodyModifiers.Divider += GetRotateSpeedDivider;

        if (character is Player player)
        {
            player.MouseSensitivityModifiers.Divider += GetSlowdownDivider;
            player.ControllerSensitivityModifiers.Divider += GetSlowdownDivider;
        }
    }

    public override void OnUnequip()
    {
        character.TopSpeedModifiers.Divider -= GetSlowdownDivider;
        character.RotateBodyModifiers.Divider -= GetRotateSpeedDivider;

        if (character is Player player)
        {
            player.MouseSensitivityModifiers.Divider -= GetSlowdownDivider;
            player.ControllerSensitivityModifiers.Divider -= GetSlowdownDivider;
        }
    }

    public override void Activate()
    {
        if (animationPlayer.IsPlaying())
            return;

        animationPlayer.Play("Swing1");
    }

    private float GetSlowdownDivider()
    {
        if (animationPlayer.IsPlaying())
            return moveSpeedDivider;

        return 1.0f;
    }

    private float GetRotateSpeedDivider()
    {
        if (animationPlayer.IsPlaying())
            return rotateSpeedDivider;

        return 1.0f;
    }

}
