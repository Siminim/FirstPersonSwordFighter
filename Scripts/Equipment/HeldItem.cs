using Godot;
using System;

public partial class HeldItem : Node3D
{
    protected Character player;
    protected Node3D hand;

    public virtual void Assign(Character player, Node3D hand)
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

    public virtual void OnUpdate(bool state, double delta)
    {
        
    }

    public virtual void OnReleaseActivate()
    {

    }

}
