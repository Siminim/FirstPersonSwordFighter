using Godot;
using System;

public partial class StandardSword : HeldItem
{
    private AnimationPlayer animationPlayer;

    private Area3D hitbox;

    private float moveSpeedDivider = 5.0f;
    private float rotateSpeedDivider = 5.0f;

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        hitbox = GetNode<Area3D>("Hitbox");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!animationPlayer.IsPlaying() && Active)
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
        if (animationPlayer.IsPlaying())
            return;

        animationPlayer.Play("Swing1");
        base.Activate();
    }

    public override void Deactivate()
    {
        
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

    public void OnWeaponHitHurtbox(Area3D hurtbox)
    {
        if (hurtbox.GetParent() is Character character && character != this.character)
            character.ApplyDamage(1.0f);
    }

}
