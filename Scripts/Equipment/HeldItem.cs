using Godot;
using System;

public partial class HeldItem : Node3D
{
    protected Player player;

    public virtual void Assign(Player player)
    {
        this.player = player;
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
