using Godot;
using System;

public partial class StandardSword : HeldItem
{
    private Area3D hitbox;

    private float moveSpeedDivider = 5.0f;
    private float rotateSpeedDivider = 5.0f;

    public override void _Ready()
    {
        hitbox = GetNode<Area3D>("Hitbox");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!character.animationPlayer.IsPlaying() && Active)
            base.Deactivate();
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
        if (Active)
            return;

        character.animationPlayer.Play("Right_Hand_Sword_Swing");
        base.Activate();
    }

    public override void Deactivate()
    {
        
    }

    private float GetSlowdownDivider()
    {
        if (Active)
            return moveSpeedDivider;

        return 1.0f;
    }

    private float GetRotateSpeedDivider()
    {
        if (Active)
            return rotateSpeedDivider;

        return 1.0f;
    }

    public void OnWeaponHitHurtbox(Area3D hurtbox)
    {
        if (hurtbox.GetParent() is Character character && character != this.character)
            character.ApplyDamage(1.0f);
    }

}
