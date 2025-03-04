using Godot;
using System;

public partial class HeldItem : Node3D
{
    protected Character character;
    protected Node3D hand;

    public virtual void Assign(Character character, Node3D hand)
    {
        this.character = character;
        this.hand = hand;
    }

    public virtual void OnEquip()
    {

    }

    public virtual void OnUnequip()
    {

    }

    public virtual void Activate()
    {
        
    }

    public virtual void Deactivate()
    {

    }

}

public enum HeldItemSlot
{
    LeftHand,
    RightHand
}
