using Godot;
using System;

public partial class StandardSword : HeldItem
{

    // References
    //private MoveAction moveAction;

    private AnimationPlayer animationPlayer;

    // Stats
    private float slowDownDivider = 5.0f;


    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public override void Assign(Character character, Node3D hand)
    {
        base.Assign(character, hand);
        //moveAction = character.actionEventManager[CharacterActionEventType.Move] as MoveAction;
    }

    public override void OnEquip()
    {
        //moveAction.TopSpeedModifiers.Divider += GetSlowdownDivider;
    }

    public override void OnUnequip()
    {
        //moveAction.TopSpeedModifiers.Divider -= GetSlowdownDivider;
    }

    public override void OnPressActivate()
    {
        if (animationPlayer.IsPlaying())
            return;

        animationPlayer.Play("Swing1");
    }

    private float GetSlowdownDivider()
    {
        if (animationPlayer.IsPlaying())
            return slowDownDivider;

        return 1.0f;
    }

}
