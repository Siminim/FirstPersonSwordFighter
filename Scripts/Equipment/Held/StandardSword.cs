using Godot;
using System;

public partial class StandardSword : HeldItem
{

    // References
    private MoveAction moveAction;

    private AnimationPlayer animationPlayer;

    // Stats
    private float slowDownDivider = 5.0f;

    // Variables
    private bool swinging = false;


    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public override void Assign(Player player, Node3D hand)
    {
        base.Assign(player, hand);
        moveAction = player.actionEventManager[PlayerActionEventType.Move] as MoveAction;
    }

    public override void OnEquip()
    {
        moveAction.TopSpeedDivider += GetSlowdownDivider;
    }

    public override void OnUnequip()
    {
        moveAction.TopSpeedDivider -= GetSlowdownDivider;
        swinging = false;
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
