using Godot;
using System;

public partial class HeldItem : Node3D
{
    protected Player player;
    protected Node3D hand;

    public virtual void Assign(Player player, Node3D hand)
    {
        this.player = player;
        this.hand = hand;
    }

    public virtual void OnEquip()
    {

    }

    public virtual void OnUnequip()
    {

    }

    public virtual void OnPressActivate()
    {
        
    }

    public virtual void OnHoldActivate(bool state, double delta)
    {
        
    }

    public virtual void OnReleaseActivate()
    {

    }

}
